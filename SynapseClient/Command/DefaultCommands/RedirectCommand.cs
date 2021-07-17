using SynapseClient.API;
using System.Linq;

namespace SynapseClient.Command.DefaultCommands
{
    [SynapseCmdInformation(
        Name = "Redirect",
        Aliases = new[] { "rd" },
        Description = "Connects you to a different Server even if you are conntected to a server currently",
        Usage = "redirect ip"
        )]
    public class RedirectCommand : ISynapseCommand
    {
        public SynapseCommandResult Execute(SynapseCommandContext context)
        {
            if (context.Arguments.Count == 0) return new SynapseCommandResult
            {
                Response = "You have to specify a IP adress",
                Result = CommandResult.BadRequest
            };

            Client.Get.Redirect(context.Arguments.ElementAt(0));

            return new SynapseCommandResult
            {
                Response = "Started redirect to server",
                Result = CommandResult.Success
            };
        }
    }
}
