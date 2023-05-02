using System.Reflection;

namespace ForkDiscordBotTest;

public abstract class AbstractTestBase<T>
{
    protected abstract T Tested { get; set; }

    private MethodInfo GetMethod(string methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
            Assert.Fail("methodName cannot be null or whitespace");

        var method = Tested.GetType()
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null)
            Assert.Fail($"{methodName} method not found");

        return method;
    }
}