using System.Collections.Generic;

namespace SynapseClient.Command
{
    public class FullCommand
    {
        internal FullCommand() { }

        public List<string> Names { get; set; }

        public string Usage { get; set; }

        public string Description { get; set; }

        public ISynapseCommand SynapseCommand { get; set; }
    }
}
