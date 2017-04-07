using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Assets.Scripts.Helpers
{
    public static class BinarySerializationHelper
    {
        public static byte[] Serialize<T>(T data)
        {
            var bf = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            bf.Serialize(memoryStream, data);
            return memoryStream.GetBuffer();
        }

        public static T Deserialize<T>(byte[] data)
        {
            var bf = new BinaryFormatter();
            var memoryStream = new MemoryStream(data);
            return (T)bf.Deserialize(memoryStream);
        }
    }
}
