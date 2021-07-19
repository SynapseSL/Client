using System.Linq;
using SynapseClient.API;

namespace SynapseClient.Command.DefaultCommands
{
    [SynapseCmdInformation(
        Name = "Test",
        Description = "Test Command for testing stuff",
        Usage = "",
        Aliases = new string[] { }
        )]
    public class TestCommand : ISynapseCommand
    {
        public SynapseCommandResult Execute(SynapseCommandContext context)
        {
            if (context.Arguments.Count == 0) return new SynapseCommandResult
            {
                Response = "You need to specify a subcommand",
                Result = CommandResult.BadRequest
            };

            switch (context.Arguments.ElementAt(0))
            {
                case "1":
                    Client.Get.Reconnect();
                    return new SynapseCommandResult
                    {
                        Response = "Reconnect",
                        Result = CommandResult.Success
                    };

                case "2":
                    return new SynapseCommandResult
                    {
                        Response = Client.Get.ServerIp + ":" + Client.Get.ServerPort,
                        Result = CommandResult.Success
                    };

                case "3":
                    UnityEngine.Component.FindObjectOfType<NewMainMenu>().Connect("scpsl.nordholz.games:7778");
                    return new SynapseCommandResult
                    {
                        Response = "connect",
                        Result = CommandResult.Success
                    };

                default: return new SynapseCommandResult
                {
                    Response = "Invalid subcommand",
                    Result = CommandResult.BadRequest
                };
            }
        }
    }
}
