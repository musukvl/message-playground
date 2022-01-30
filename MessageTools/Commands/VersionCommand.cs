namespace MessageTools.Commands;

public class VersionCommand
{
    public int OnExecute()
    {
        Console.WriteLine(GetType().Assembly.GetName().Version);
        return 0;
    }
}