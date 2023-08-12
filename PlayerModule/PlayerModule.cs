using Unity.Services.CloudCode.Core;

namespace PlayerModule;
public class PlayerModule
{
    [CloudCodeFunction("SayHello")]
    public string Hello(string name)
    {
        return $"Hello, {name}!";
    }
}
