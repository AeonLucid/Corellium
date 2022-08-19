using Corellium.Api.Net.Models;

namespace Corellium.Api;

public class CorelliumInstance
{
    private readonly CorelliumProject _project;
    private readonly CorelliumInstanceInfo _info;

    public CorelliumInstance(CorelliumProject project, CorelliumInstanceInfo info)
    {
        _project = project;
        _info = info;
    }
}