using System;

namespace SynapseClient.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SynapseCmdInformation : Attribute
    {
        public string Name { get; set; }

        public string[] Aliases { get; set; }

        public string Usage { get; set; }

        public string Description { get; set; }
    }
}
