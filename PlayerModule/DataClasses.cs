using System.Text.Json.Serialization;

namespace PlayerModule;

public class Authentication
{
    [JsonPropertyName("id")] public int ID { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("reward")] public int Reward { get; set; }

    [JsonPropertyName("progress_required")]
    public int ProgressRequired { get; set; }

    [JsonPropertyName("progress_per_minute")]
    public int ProgressPerMinute { get; set; }
}

public class AuthenticationData
{
    public AuthenticationData()
    {

    }

    public AuthenticationData(string questName, int reward, int progressLeft, int progressPerMinute, DateTime questStartTime)
    {
        QuestName = questName;
        Reward = reward;
        ProgressLeft = progressLeft;
        ProgressPerMinute = progressPerMinute;
        QuestStartTime = questStartTime;
        LastProgressTime = new DateTime();
    }

    [JsonPropertyName("quest-name")] public string? QuestName { get; set; }

    [JsonPropertyName("reward")] public long Reward { get; set; }

    [JsonPropertyName("progress-left")] public long ProgressLeft { get; set; }

    [JsonPropertyName("progress-per-minute")]
    public long ProgressPerMinute { get; set; }

    [JsonPropertyName("quest-start-time")] public DateTime QuestStartTime { get; set; }

    [JsonPropertyName("last-progress-time")] public DateTime LastProgressTime { get; set; }
}