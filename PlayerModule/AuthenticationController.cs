using System.Text.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudSave.Model;

namespace PlayerModule;

public class AuthenticationController
{
    [CloudCodeFunction("Connect")]
    public async Task<string> AssignAuthentication(IExecutionContext ctx, IAuthenticationService authenticationService, IGameApiClient apiClient)
    {
        var questData = await GetQuestData(ctx, apiClient);

        if (questData?.QuestName != null) return "Player already has a quest in progress!";

        var availableQuests = authenticationService.GetAvailableAuthentications(ctx);
        var random = new Random();
        var index = random.Next(availableQuests.Count);
        var quest = availableQuests[index];

        questData = new AuthenticationData(quest.Name, quest.Reward, quest.ProgressRequired, quest.ProgressPerMinute,
            DateTime.Now);

        await SetQuestData(ctx, apiClient, "quest-data", JsonSerializer.Serialize(questData));

        return $"Player was assigned quest: {quest.Name}!";
    }

    [CloudCodeFunction("PerformAction")]
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
    }

    private async Task<AuthenticationData?> GetQuestData(IExecutionContext ctx, IGameApiClient apiClient)
    {
        var result = await apiClient.CloudSaveData.GetItemsAsync(
            ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
            new List<string> { "quest-data" });

        if (result.Data.Results.Count == 0) return null;

        return JsonSerializer.Deserialize<AuthenticationData>(result.Data.Results.First().Value.ToString());
    }

    private async Task<string> SetQuestData(IExecutionContext ctx, IGameApiClient apiClient, string key, string value)
    {
        var result = await apiClient.CloudSaveData
            .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody(key, value));

        return result.Data.ToJson();
    }
}