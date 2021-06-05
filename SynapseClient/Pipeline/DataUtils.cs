namespace SynapseClient.Pipeline
{
    public static class DataUtils
    {
        public static byte[] pack(byte[] payload)
        {
            var buffer = new byte[payload.Length + 2];
            buffer[0] = byte.MinValue;
            buffer[1] = byte.MaxValue;
            for (int i = 0; i < payload.Length; i++) buffer[i + 2] = payload[i];
            return buffer;
        }
        
        public static byte[] unpack(byte[] encoded)
        {
            var buffer = new byte[encoded.Length - 2];
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = encoded[i + 2];
            }
            return buffer;
        }

        public static bool isData(byte[] bytes)
        {
            if (bytes.Length < 2) return false;
            return bytes[0] == byte.MinValue && bytes[1] == byte.MaxValue;
        }
    }
}