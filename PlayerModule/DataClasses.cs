using System.Text.Json.Serialization;

namespace PlayerModule;

/*public class Authentication
{
    [JsonPropertyName("id")] public int ID { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("reward")] public int Reward { get; set; }

    [JsonPropertyName("progress_required")]
    public int ProgressRequired { get; set; }

    [JsonPropertyName("progress_per_minute")]
    public int ProgressPerMinute { get; set; }
}*/

public class PlayerConf
{
    public PlayerConf()
    {

    }

    public PlayerConf(string username, bool creationDone, bool tutorialDone)
    {
        Username = username;
        CreationDone = creationDone;
        TutorialDone = tutorialDone;
    }

    [JsonPropertyName("username")] public string? Username { get; set; }
    
    [JsonPropertyName("creationDone")] public bool CreationDone { get; set; }

    [JsonPropertyName("tutorialDone")] public bool TutorialDone { get; set; }
}