using System.Net;
using Flurl.Http.Configuration;

namespace Corellium.Api.Net.Handler;

internal class ProxyHttpClientFactory : DefaultHttpClientFactory
{
    private readonly IWebProxy _proxy;

    public ProxyHttpClientFactory(IWebProxy proxy)
    {
        _proxy = proxy;
    }
    
    public override HttpMessageHandler CreateMessageHandler()
    {
        var handler = (HttpClientHandler) base.CreateMessageHandler();

        handler.UseProxy = true;
        handler.Proxy = _proxy;
        
        return handler;
    }
}