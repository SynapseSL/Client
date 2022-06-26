using Neuron.Core.Logging;
using Neuron.Core.Modules;
using Neuron.Modules.Commands;
using Neuron.Modules.Configs;
using Neuron.Modules.Patcher;
using Ninject;

namespace SynapseClient.ClientModule
{
    [Module(
        Name = "Synapse Client",
        Description = "Synapse Client neuron module",
        Dependencies = new []
        {
            typeof(CommandsModule),
            typeof(ConfigsModule),
            typeof(PatcherModule)
        }
    )]
    public class ClientModule : Module
    {
        public override void Load(IKernel kernel)
        {
            
        }

        public override void Enable()
        {
            
        }

        public override void Disable()
        {
            
        }
    }
}