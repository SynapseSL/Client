namespace SynapseClient.Command
{
    public interface ISynapseCommand
    {
        SynapseCommandResult Execute(SynapseCommandContext context);
    }
}
