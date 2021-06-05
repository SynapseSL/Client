using Org.BouncyCastle.Utilities.Encoders;
using SynapseClient.Patches;

namespace SynapseClient.Pipeline
{
    public static class ClientPipeline
    {
        public static event DataEvent<byte[]> DataReceivedEvent;
        
        public static void receive(byte[] data)
        {
            Logger.Info($"=pipeline=>  {Base64.ToBase64String(data)}");
            DataReceivedEvent?.Invoke(data);
            Logger.Info("Event invoked!");
        }

        public static void invoke(byte[] data)
        {
            var packed = DataUtils.pack(data);
            Logger.Info($"<=pipeline=  {Base64.ToBase64String(data)}");
            PipelinePatches.Transmission.CmdCommandToServer(packed, false);
        }
        
        public delegate void DataEvent<in TEvent>(TEvent ev);
    }
}