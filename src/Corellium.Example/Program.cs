using Corellium.Api;

var corellium = new CorelliumClient(new CorelliumOptions(
    endpoint: "https://app.corellium.com",
    apiToken: ""
));

var projects = await corellium.GetProjectsAsync();

foreach (var project in projects)
{
    var instances = await project.GetInstancesAsync();
    var instance = instances.FirstOrDefault();
    if (instance == null)
    {
        Console.WriteLine("No instance found");
        return;
    }
    
}
