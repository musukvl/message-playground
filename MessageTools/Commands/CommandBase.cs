namespace MessageTools.Commands;

public abstract class CommandBase
{
    public abstract Task<int> OnExecute();
}