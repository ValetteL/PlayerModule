using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace PlayerModule;

public class AuthenticationController
{
    private readonly ILogger<AuthenticationController> _logger;
    
    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }
    
    [CloudCodeFunction("Connect")]
    public async Task<string> Connect(IExecutionContext ctx, IGameApiClient apiClient)
    {
        PlayerConf playerConf;
        
        try
        {
            ApiResponse<GetItemsResponse> result = await apiClient.CloudSaveData.GetItemsAsync(
                ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                new List<string> { "playerConf" });
            
            if (result.Data.Results.Count == 0) 
                return JsonConvert.SerializeObject(new InvalidOperationException("No account found for this player"));
            
            playerConf = JsonConvert.DeserializeObject<PlayerConf>(result.Data.Results.First().Value.ToString());
        }
        catch (Exception e)
        {
            return "Error while retrieving player account";
        }

        return JsonConvert.SerializeObject(playerConf);
    }
    
    [CloudCodeFunction("Register")]
    public async Task<string> Register(IExecutionContext ctx, IGameApiClient apiClient, string username)
    {
        PlayerConf playerConf = new PlayerConf();
        
        try
        {
            playerConf.Username = username;
            playerConf.CreationDone = false;
            playerConf.TutorialDone = false;
            
            var result = await apiClient.CloudSaveData
                .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody("playerConf", playerConf));
            
            result = await apiClient.CloudSaveData
                .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody("progression", new PlayerProgression()));

            Gear gear = new Gear();
            //_logger.LogInformation("gear equipments : " + string.Join("\n", gear.Equipments.Select(e => e.ToString())));
            /*JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Include;*/
            
            result = await apiClient.CloudSaveData
                .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody("gear", gear));

            List<Stat> characterStats = new List<Stat>()
            {
                new() { statType = StatType.Health, value = 100 },
                new() { statType = StatType.Strength, value = 15 },
                new() { statType = StatType.Luck, value = 1 },
                new() { statType = StatType.Agility, value = 1 },
                new() { statType = StatType.Armor, value = 0 },
                new() { statType = StatType.Intelligence, value = 0 },
            };
            result = await apiClient.CloudSaveData
                .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody("characterStats", characterStats));
        }
        catch (Exception e)
        {
            return JsonConvert.SerializeObject(e);
        }

        return JsonConvert.SerializeObject(playerConf);
    }
    
    /*[CloudCodeFunction("AssignAuthentication")]
    public async Task<string> AssignAuthentication(IExecutionContext ctx, IAuthenticationService authenticationService, IGameApiClient apiClient)
    {
        var questData = await GetQuestData(ctx, apiClient);

        if (questData?.QuestName != null) return "Player already has a quest in progress!";

        var availableQuests = authenticationService.GetAvailableAuthentications(ctx);
        var random = new Random();
        var index = random.Next(availableQuests.Count);
        var quest = availableQuests[index];

        questData = new PlayerConf(quest.Name, quest.Reward, quest.ProgressRequired, quest.ProgressPerMinute,
            DateTime.Now);

        await SetQuestData(ctx, apiClient, "quest-data", JsonSerializer.Serialize(questData));

        return $"Player was assigned quest: {quest.Name}!";
    }*/

    /*[CloudCodeFunction("PerformAction")]
    public async Task<string> PerformAction(IExecutionContext ctx, IGameApiClient apiClient)
    {
        var questData = await GetQuestData(ctx, apiClient);

        if (questData?.QuestName == null) return "Player does not have a quest in progress!";

        if (questData.ProgressLeft == 0) return "Player has already completed their quest!";

        if (DateTime.Now < questData.LastProgressTime.AddSeconds(60 / questData.ProgressPerMinute)) return "Player cannot make quest progress yet!";

        questData.LastProgressTime = DateTime.Now;
        questData.ProgressLeft--;

        await SetQuestData(ctx, apiClient, "quest-data", JsonSerializer.Serialize(questData));

        return "Player made quest progress!";
    }*/

    /*private async Task<string> SetQuestData(IExecutionContext ctx, IGameApiClient apiClient, string key, string value)
    {
        var result = await apiClient.CloudSaveData
            .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody(key, value));

        return result.Data.ToJson();
    }*/
}