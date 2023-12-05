using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace PlayerModule;

public class CloudCodeSetup : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        //config.Dependencies.AddSingleton<IAuthenticationService, AuthenticationService>();
        config.Dependencies.AddSingleton<IGameApiClient>(s => GameApiClient.Create());
    }
}