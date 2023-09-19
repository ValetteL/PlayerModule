using System.Text.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace PlayerModule;

public interface IAuthenticationService
{
    //IList<Authentication> GetAvailableAuthentications(IExecutionContext ctx);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IGameApiClient _apiClient;

    public AuthenticationService(IGameApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private DateTime? CacheExpiryTime { get; set; }

    // Reminder: cache cannot be guaranteed to be consistent across all requests
    //private IList<Authentication>? AuthenticationCache { get; set; }

    /*public IList<Authentication> GetAvailableAuthentications(IExecutionContext ctx)
    {
        if (AuthenticationCache == null || DateTime.Now > CacheExpiryTime)
        {
            var quests = FetchAuthenticationsFromRC(ctx);
            AuthenticationCache = quests;
            CacheExpiryTime = DateTime.Now.AddMinutes(5); // data in cache expires after 5 mins
        }

        return AuthenticationCache;
    }*/
    
    /*private IList<Authentication> FetchAuthenticationsFromRC(IExecutionContext ctx)
    {
        var result = _apiClient.RemoteConfigSettings.AssignSettingsGetAsync(ctx, ctx.AccessToken, ctx.ProjectId,
            ctx.ProjectId, null, new List<string> { "QUESTS" });

        var settings = result.Result.Data.Configs.Settings;

        return JsonSerializer.Deserialize<List<Authentication>>(settings["QUESTS"].ToString());
    }*/
}