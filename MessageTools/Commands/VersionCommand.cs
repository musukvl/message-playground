using McMaster.Extensions.CommandLineUtils;

namespace MessageTools.Commands;

[Command("version", Description = "Returns tool version")]
public class VersionCommand
{
    public int OnExecute()
    {
        Console.WriteLine(GetType().Assembly.GetName().Version);
        return 0;
    }
}