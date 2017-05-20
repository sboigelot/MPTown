using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public static class ZipHelper
    {
        public static byte[] Zip(byte[] bytes)
        {
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    using (GZipStream zipStream = new GZipStream(ms, CompressionMode.Compress, false))
            //    {
            //        zipStream.Write(bytes, 0, bytes.Length);
            //    }
            //    return ms.GetBuffer();
            //}
            
            using (var inputStream = new MemoryStream(bytes))
            using (var outputStream = new MemoryStream())
            using (var gStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                byte[] buffer = new byte[1024];
                int nRead;
                while ((nRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    gStream.Write(buffer, 0, nRead);
                }
                return outputStream.GetBuffer();
            }
        }

        public static byte[] Unzip(byte[] bytes)
        {
            //using (var outputStream = new MemoryStream())
            //{
            //    using (var zipStream = new GZipStream(outputStream, CompressionMode.Compress))
            //    {
            //        zipStream.Write(bytes, 0, bytes.Length);
            //    }
            //    return outputStream.GetBuffer();
            //}
            
            using (var inputStream = new MemoryStream())
            using (var outputStream = new MemoryStream(bytes))
            using (var gStream = new GZipStream(outputStream, CompressionMode.Decompress))
            {
                byte[] buffer = new byte[1024];
                int nRead;
                while ((nRead = gStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    inputStream.Write(buffer, 0, nRead);
                }

                return inputStream.GetBuffer();
            }
        }
    }
}