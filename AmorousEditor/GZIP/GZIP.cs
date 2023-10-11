// Thanks to https://www.dotnetperls.com/decompress and https://www.dotnetperls.com/compress

// One thing to note is that Amorous, instead of using XNB compression compresses all the files in Content-Release (Yes, all of them) using GZIP compression
// So you need to decompress them, and after editing them, you need to compress them again, or the game will crash

using System.IO;
using System.IO.Compression;

namespace AmorousEditor
{
    static class GZIP
    {
        /// <summary>
        /// Decompresses a GZIP Compressed byte array //TODO: Why am I converting a stream to an array to a stream? I see, buffer
        /// </summary>
        /// <param name="gzip">Byte array to be decompressed</param>
        /// <returns>GZIP Decompressed MemoryStream</returns>
        /// See <see cref="Compress(byte[])"/> to compress
        public static MemoryStream Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                MemoryStream memory = new MemoryStream();
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);
                memory.Position = 0;
                return memory;
            }
        }

        public static MemoryStream Decompress(Stream gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(gzip, CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                MemoryStream memory = new MemoryStream();
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);
                memory.Position = 0;
                return memory;
            }
        }

        /// <summary>
        /// Compresses a byte array using GZIP Compression
        /// </summary>
        /// <param name="raw">byte array to be compressed</param>
        /// <returns>GZIP Compressed MemoryStream</returns>
        /// See <see cref="Compress(byte[])"/> to decompress
        public static MemoryStream Compress(byte[] raw)
        {
            MemoryStream memory = new MemoryStream();

            using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
            {
                gzip.Write(raw, 0, raw.Length);
            }
            memory.Position = 0;
            return memory;
        }
    }
}
