using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using AmorousEditor.Types;
using System.Windows.Media;

namespace AmorousEditor.Readers
{
    class Texture2DReader : BaseReader
    {
        public static Texture2D Read(BinaryReader reader, XNB.XNBHeader header)
        {
            var format = reader.ReadInt32();
            var width = reader.ReadInt32(); 
            var height = reader.ReadInt32(); 
            var mipcount = reader.ReadInt32(); 
            var datasize = reader.ReadInt32();
            var data = reader.ReadBytes(datasize);
            reader.Close();
            for (var i = 0; i < data.Length; i += 4)
            {
                var r = data[i];
                var b = data[i + 2];
                data[i] = b;
                data[i + 2] = r;
            }
            var bitWrite = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            bitWrite.WritePixels(new Int32Rect(0, 0, width, height), data, 4 * width, 0);
            return new Texture2D { Texture = bitWrite };
        }
    }
}
