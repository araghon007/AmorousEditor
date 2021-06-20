using System.IO;

namespace AmorousEditor.Misc
{
    /// <summary>
    /// Taken from https://stackoverflow.com/a/26234928 and converted to use a stream instead of byte array, since this fits my needs more.
    /// Also modified to only look at the magic number.
    /// </summary>
    public static class CompressionHelper
    {
        public static byte[] GZipMagic = { 0x1f, 0x8b };

        /// <summary>
        /// Looks at the magic number to see if the file is compressed
        /// </summary>
        /// <param name="fileStream">The file stream</param>
        /// <returns>whether or not the file is gzip compressed</returns>
        public static bool IsPossiblyGZipped(this Stream fileStream)
        {
            // I'm also saving the current stream position, just in case
            var currPos = fileStream.Position;
            fileStream.Seek(0, SeekOrigin.Begin);

            var compressed = true;

            // Compare bytes
            for (int i = 0; i < GZipMagic.Length; i++)
            {
                if (fileStream.ReadByte() != GZipMagic[i])
                {
                    compressed = false;
                    break;
                }
            }

            // And go back to the saved position
            fileStream.Seek(currPos, SeekOrigin.Begin);

            return compressed;
        }
    }
}
