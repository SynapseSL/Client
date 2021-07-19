using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SynapseClient.API;
using SynapseClient.API.Mods;
using SynapseClient.Command.DefaultCommands;
using Console = GameCore.Console;

namespace SynapseClient.Command
{
    public class SynapseCommandHandler
    {
        public static SynapseCommandHandler Get => Client.Get.CommandHandler;
            
        internal SynapseCommandHandler() { }

        public List<FullCommand> AllCommands { get; } = new List<FullCommand>();

        public FullCommand RegisterSynapseCommand(Type cmdtype, Type modtype = null, ClientMod mod = null)
        {
            if (cmdtype.IsSubclassOf(typeof(ISynapseCommand)) || cmdtype == typeof(ISynapseCommand)) return null;

            var constructor = modtype == null ? null : cmdtype.GetConstructor(new Type[] { modtype });

            if(constructor == null)
            {
                var cmd = (ISynapseCommand)Activator.CreateInstance(cmdtype);
                return RegisterSynapseCommand(cmd);
            }
            else
            {
                var cmd = (ISynapseCommand)Activator.CreateInstance(cmdtype,new object[] { mod });
                return RegisterSynapseCommand(cmd);
            }
        }

        public FullCommand RegisterSynapseCommand(ISynapseCommand cmd)
        {
            var attribute = cmd.GetType().GetCustomAttribute<SynapseCmdInformation>();

            if(attribute == null)
            {
                Logger.Error($"Synapse-Command: {cmd.GetType()} does not contain SynapseCmdInformations an therefore can't be registered as Command");
                return null;
            }

            var names = new List<string> { attribute.Name };
            names.AddRange(attribute.Aliases);

            var fullcmd = new FullCommand()
            {
                SynapseCommand = cmd,
                Description = attribute.Description,
                Usage = attribute.Usage,
                Names = names
            };

            AllCommands.Add(fullcmd);
            Logger.Info($"Successfully registered {attribute.Name} - Command");
            return fullcmd;
        }

        public bool ExecuteCommand(string commandline)
        {
            if (commandline == null || commandline.StartsWith(".")) return false;

            var args = commandline.Split(' ');
            if (args.Count() == 0) return false;

            var command = AllCommands.FirstOrDefault(x => x.Names.Any(y => y.ToLower() == args[0].ToLower()));
            if (command == null) return false;

            try
            {
                var result = command.SynapseCommand.Execute(new SynapseCommandContext
                {
                    Arguments = new ArraySegment<string>(args, 1, args.Count() - 1)
                });

                var color = UnityEngine.Color.white;

                switch (result.Result)
                {
                    case CommandResult.BadRequest:
                        color = UnityEngine.Color.magenta;
                        break;

                    case CommandResult.Error:
                        color = UnityEngine.Color.red;
                        break;

                    case CommandResult.NoPermission:
                        color = UnityEngine.Color.cyan;
                        break;

                    case CommandResult.Success:
                        color = UnityEngine.Color.green;
                        break;
                }

                Console.AddLog(result.Response, color);
                Console.singleton._clientCommandLogs.Add(commandline);
                return true;
            }
            catch(Exception e)
            {
                Logger.Error(e);
                Console.AddLog("Error ocurred while executing the command", UnityEngine.Color.red);
                Console.singleton._clientCommandLogs.Add(commandline);
                return true;
            }
        }

        internal void RegisterSynapseCommands()
        {
            RegisterSynapseCommand(new RedirectCommand());

#if DEBUG
            RegisterSynapseCommand(new TestCommand());
#endif
        }
    }
}
