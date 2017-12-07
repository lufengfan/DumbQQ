using System.IO;

namespace DumbQQ.Utils
{
    internal static class StreamHelper
    {
        public static byte[] ToBytes(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                const int bufferSize = 81920;
                byte[] array = new byte[bufferSize];
                int count;
                while ((count = stream.Read(array, 0, array.Length)) != 0)
                {
                    ms.Write(array, 0, count);
                }
                array = null;

                stream.Close();
                return ms.ToArray();
            }
        }
    }
}