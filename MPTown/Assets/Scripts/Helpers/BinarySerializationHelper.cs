using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assets.Scripts.Helpers
{
    public static class BinarySerializationHelper
    {
        public static bool Gzip = false;

        public static byte[] Serialize<T>(T data)
        {
            var bf = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            bf.Serialize(memoryStream, data);
            var serializedBuffer = memoryStream.GetBuffer();

            return Gzip ? ZipHelper.Zip(serializedBuffer) : serializedBuffer;
        }

        public static T Deserialize<T>(byte[] data)
        {
            if (Gzip)
            {
                data = ZipHelper.Unzip(data);
            }

            var bf = new BinaryFormatter();
            var memoryStream = new MemoryStream(data);
            return (T) bf.Deserialize(memoryStream);
        }
    }
}