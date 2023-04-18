using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using Corellium.Api.Exceptions;
using Corellium.Api.Net.Models.Agent;

namespace Corellium.Api;

public record AgentResponse(JsonObject? Object, byte[]? Data);

public class CorelliumAgent : IDisposable
{
    private const int AgentUploadIdTimeout = 15;
    private const int AgentUploadTimeout = 120;
    private const int AgentResponseTimeout = 15;
    
    private readonly CorelliumClient _client;
    private readonly CorelliumInstance _instance;
    private readonly SemaphoreSlim _connectLock;
    private readonly ConcurrentDictionary<uint, TaskCompletionSource<AgentResponse>> _pending;
    private readonly ClientWebSocket _webSocket;
    
    private uint _id;
    private Task? _readTask;
    
    public CorelliumAgent(CorelliumClient client, CorelliumInstance instance)
    {
        _client = client;
        _instance = instance;
        _connectLock = new SemaphoreSlim(1, 1);
        _pending = new ConcurrentDictionary<uint, TaskCompletionSource<AgentResponse>>();
        _webSocket = new ClientWebSocket
        {
            Options =
            {
                Proxy = client.Proxy
            }
        };
    }

    public bool Connected => _webSocket.State == WebSocketState.Open;
    public bool PendingConnect => _webSocket.State == WebSocketState.Connecting;

    public async ValueTask ConnectAsync()
    {
        var hasLock = false;

        try
        {
            hasLock = await _connectLock.WaitAsync(TimeSpan.FromSeconds(15));

            if (hasLock)
            {
                await ConnectInternalAsync();
            }
        }
        finally
        {
            if (hasLock)
            {
                _connectLock.Release();
            }
        }
    }

    private async ValueTask ConnectInternalAsync()
    {
        if (Connected)
        {
            return;
        }

        var agentEndpoint = _instance.GetAgentEndpointAsync();
        var agentUri = new Uri(agentEndpoint.Replace("https://", "wss://"));

        try
        {
            await _webSocket
                .ConnectAsync(agentUri, CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(15));
            
            _pending.Clear();
            _readTask = ReadAsync();
        }
        catch (WebSocketException e)
        {
            throw new CorelliumAgentException("Failed to connect to agent", e);
        }
    }

    private async Task ReadAsync()
    {
        try
        {
            while (Connected)
            {
                using var memoryStream = new MemoryStream();
                using var rent = MemoryPool<byte>.Shared.Rent(4096);

                // Read message into memoryStream.
                ValueWebSocketReceiveResult result;
                
                do
                {
                    result = await _webSocket.ReceiveAsync(rent.Memory, CancellationToken.None);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    await memoryStream.WriteAsync(rent.Memory.Slice(0, result.Count));
                } while (!result.EndOfMessage);
                
                // Reset stream.
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Handle message.
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = JsonSerializer.Deserialize<JsonObject>(memoryStream)!;
                    var messageId = message["id"]!.GetValue<uint>();

                    if (_pending.TryRemove(messageId, out var tcs))
                    {
                        tcs.SetResult(new AgentResponse(message, null));
                    }
                }
                else
                {
                    // TODO: Handle.
                }
            }
        }
        catch (WebSocketException e)
        {
            if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely && !Connected)
            {
                return;
            }
            
            // TODO: Replace with proper logger.
            Console.WriteLine("Error reading from agent (1)");
            Console.WriteLine(e);

            throw new CorelliumAgentException("Failed to read from agent", e);
        }
        catch (Exception e)
        {
            // TODO: Replace with proper logger.
            Console.WriteLine("Error reading from agent (2)");
            Console.WriteLine(e);

            throw new CorelliumAgentException("Failed to read from agent", e);
        }
        finally
        {
            _readTask = null;
        }
    }

    private async Task SendAsync(JsonObject message)
    {
        // Serialize.
        var serialized = JsonSerializer.SerializeToUtf8Bytes(message);
        
        // Send.
        await _webSocket
            .SendAsync(serialized, WebSocketMessageType.Text, true, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(15));
    }

    private async Task SendBinaryAsync(uint messageId, Stream fileStream)
    {
        var messageIdBuffer = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(messageIdBuffer, messageId);

        while (true)
        {
            using var rent = MemoryPool<byte>.Shared.Rent(4096);
            
            var read = await fileStream.ReadAsync(rent.Memory);
            if (read == 0)
            {
                await _webSocket
                    .SendAsync(messageIdBuffer, WebSocketMessageType.Binary, true, CancellationToken.None)
                    .WaitAsync(TimeSpan.FromSeconds(AgentUploadIdTimeout));
                return;
            }
            
            await _webSocket
                .SendAsync(messageIdBuffer, WebSocketMessageType.Binary, false, CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(AgentUploadIdTimeout));
            
            await _webSocket
                .SendAsync(rent.Memory.Slice(0, read), WebSocketMessageType.Binary, true, CancellationToken.None)
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(AgentUploadTimeout));
        }
    }

    public async Task DisconnectAsync()
    {
        await _webSocket
            .CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(15));

        if (_readTask != null)
        {
            await _readTask;
        }
    }

    public async Task<bool> ReadyAsync()
    {
        var res = await CommandAsync("app", "ready");
        return res.Object!.TryGetPropertyValue("success", out var value) && value!.GetValue<bool>();
    }

    /// <summary>
    ///     Returns an array of installed apps.
    /// </summary>
    public async Task<AgentAppList> AppListAsync()
    {
        var res = await CommandAsync("app", "list");
        return res.Object.Deserialize<AgentAppList>()!;
    }

    /// <summary>
    ///     Reads from the specified stream and uploads the data to a file on the VM.
    /// </summary>
    /// <param name="path">The file path to upload the data to.</param>
    /// <param name="stream">The stream to read the file data from.</param>
    public async Task UploadAsync(string path, Stream stream)
    {
        await CommandAsync("file", "upload", new Dictionary<string, JsonNode>
        {
            { "path", path! }
        }, stream);
    }
    
    /// <summary>
    ///     Installs an app. The app's IPA must be available on the VM's filesystem.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<bool> InstallAsync(string path)
    {
        var res = await CommandAsync("app", "install", new Dictionary<string, JsonNode>
        {
            { "path", path! }
        });
        
        return res.Object!.TryGetPropertyValue("success", out var value) && value!.GetValue<bool>();
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="bundleId"></param>
    /// <returns></returns>
    public async Task<bool> UninstallAsync(string bundleId)
    {
        var res = await CommandAsync("app", "uninstall", new Dictionary<string, JsonNode>
        {
            { "bundleID", bundleId! }
        });
        
        return res.Object!.TryGetPropertyValue("success", out var value) && value!.GetValue<bool>();
    }

    /// <summary>
    ///     Returns a temporary random filename on the VMs filesystem that by the
    ///     time of invocation of this method is guaranteed to be unique.
    /// </summary>
    /// <returns>A path, or null if failed.</returns>
    public async Task<string?> TempFileAsync()
    {
        var res = await CommandAsync("file", "temp");
        var success = res.Object!.TryGetPropertyValue("success", out var value) && value!.GetValue<bool>();
        if (success)
        {
            return res.Object!.TryGetPropertyValue("path", out var path) ? path!.GetValue<string>() : null;
        }

        return null;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="path"></param>
    public async Task<AgentStat?> StatAsync(string path)
    {
        var res = await CommandAsync("file", "stat", new Dictionary<string, JsonNode>
        {
            { "path", path! }
        });
        
        var success = res.Object!.TryGetPropertyValue("success", out var value) && value!.GetValue<bool>();
        if (!success)
        {
            return null;
        }
        
        return res.Object.Deserialize<AgentStat>()!;
    }

    public async Task<bool> InstallFileAsync(Stream fileStream)
    {
        var path = await TempFileAsync();
        if (path == null)
        {
            throw new CorelliumAgentException("Failed to get temporary file path");
        }

        await UploadAsync(path, fileStream);

        try
        {
            return await InstallAsync(path);
        }
        finally
        {
            await DeleteFile(path);
        }
    }

    public async Task<bool> DeleteFile(string path)
    {
        var res = await CommandAsync("file", "delete", new Dictionary<string, JsonNode>
        {
            { "path", path! }
        });
        
        return res.Object!.TryGetPropertyValue("success", out var value) && value!.GetValue<bool>();
    }

    private async Task<AgentResponse> CommandAsync(string type, string op, Dictionary<string, JsonNode>? extraParams = null, Stream? fileStream = null)
    {
        // Connect if not connected.
        if (!Connected)
        {
            await ConnectAsync();
        }
        
        // Prepare message.
        var messageId = Interlocked.Increment(ref _id);
        var message = new JsonObject
        {
            { "type", type },
            { "op", op },
            { "id", messageId }
        };
        
        // Merge extra params.
        if (extraParams != null)
        {
            foreach (var (key, value) in extraParams)
            {
                message.Add(key, value);
            }
        }

        // Set up response handler.
        var taskCompletion = new TaskCompletionSource<AgentResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pending.TryAdd(messageId, taskCompletion))
        {
            throw new CorelliumAgentException("Failed to add pending command");
        }
        
        // Send message.
        await SendAsync(message);

        AgentResponse response;
        
        if (fileStream != null)
        {
            await SendBinaryAsync(messageId, fileStream);
            
            // Wait for response without timeout.
            // Reason being is that it reaches here before the file is fully uploaded, which may take some time.
            response = await taskCompletion.Task;
        }
        else
        {
            // Wait for response.
            response = await taskCompletion.Task.WaitAsync(TimeSpan.FromSeconds(AgentUploadTimeout));
        }

        // Check for error.
        if (response.Object != null && response.Object.TryGetPropertyValue("error", out var errorNode))
        {
            // TODO: Log error.
        }
        
        return response;
    }

    public void Dispose()
    {
        _connectLock.Dispose();
        _webSocket.Dispose();
    }
}