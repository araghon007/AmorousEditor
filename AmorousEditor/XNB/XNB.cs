using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using AmorousEditor.Misc;

namespace AmorousEditor
{
    public enum XNBType
    {
        Texture2D,
        SoundEffect,
        Song,
        SpriteFont,
        Effect,
        Unknown
    }

    public class XNB
    {
        public class XNBReader
        {
            public string reader;
            public int version;
        }

        public class XNBHeader
        {
            public char target;
            public int formatVersion;
            public bool hidef;
            public bool compressed;
            public uint fileSize;
            public XNBReader[] xnbReaders;
        }

        public static XNBType GetTypeOnly(string path)
        {
            byte[] file = File.ReadAllBytes(path);
            try
            {
                using (MemoryStream decompressed = GZIP.Decompress(file))
                {
                    var xnbhead = new XNBHeader();

                    var reader = new BinaryReader(decompressed);
                    CheckHeader(reader, xnbhead);
                    xnbhead.fileSize = reader.ReadUInt32();
                    if (xnbhead.fileSize != reader.BaseStream.Length)
                    {
                        Debug.WriteLine("XNB file has been truncated");
                    }
                    var readerCount = reader.Read();
                    var xnbReader = reader.ReadString().Split(',').First().Split('.').Last().Replace("Reader", "");
                    if (Enum.TryParse(xnbReader, out XNBType why))
                    {
                        return (XNBType)Enum.Parse(typeof(XNBType), xnbReader);
                    }
                    else
                    {
                        return XNBType.Unknown;
                    }
                }
            }
            catch
            {
                using (MemoryStream decompressed = new MemoryStream(file))
                {
                    var xnbhead = new XNBHeader();

                    var reader = new BinaryReader(decompressed);
                    CheckHeader(reader, xnbhead);
                    xnbhead.fileSize = reader.ReadUInt32();
                    if (xnbhead.fileSize != reader.BaseStream.Length)
                    {
                        Debug.WriteLine("XNB file has been truncated");
                    }
                    var readerCount = reader.Read();
                    var xnbReader = reader.ReadString().Split(',').First().Split('.').Last().Replace("Reader", "");
                    if (Enum.TryParse(xnbReader, out XNBType why))
                    {
                        return (XNBType)Enum.Parse(typeof(XNBType), xnbReader);
                    }
                    else
                    {
                        return XNBType.Unknown;
                    }
                }
            }
            
        }

        public static void ConvertToXNB(BitmapFrame bmp, string path, bool compress)
        {
            using (var stream = new MemoryStream())
            {
                // Calculate stride of the frame.
                int stride = bmp.PixelWidth * ((bmp.Format.BitsPerPixel + 7) / 8);

                // Calculate size of the buffer needed.
                int arraySize = stride * bmp.PixelHeight;

                // Create buffer.
                byte[] img = new byte[arraySize];

                // Copy frame pixels to the byte array.
                bmp.CopyPixels(img, stride, 0);

                var xnbhead = new XNBHeader();
                var writer = new BinaryWriter(stream);
                writer.Write("XNBw".ToCharArray());
                writer.Write((byte)5);
                writer.Write(false);
                writer.Write(img.Length + 85);
                writer.Write(true);
                writer.Write("Microsoft.Xna.Framework.Content.Texture2DReader");
                writer.Write(0);
                writer.Write(false);
                writer.Write(true);
                writer.Write(0);
                writer.Write(bmp.PixelWidth);
                writer.Write(bmp.PixelHeight);
                writer.Write(1);
                writer.Write(img.Length);
                
                for (var i = 0; i < img.Length; i += 4)
                {
                    byte r = img[i + 0];
                    byte g = img[i + 1];
                    byte b = img[i + 2];
                    byte a = img[i + 3];

                    img[i + 2] = Alpha(r, a);
                    img[i + 1] = Alpha(g, a);
                    img[i + 0] = Alpha(b, a);
                }
                
                writer.Write(img);
                writer.Close();
                if (compress)
                {
                    using (var memstr = GZIP.Compress((writer.BaseStream as MemoryStream).ToArray()))
                    {
                        File.WriteAllBytes(path, memstr.ToArray());
                    }
                }
                else
                {
                    File.WriteAllBytes(path, (writer.BaseStream as MemoryStream).ToArray());
                }
            }
        }

        // Easy way of premultiplying alpha, thanks to https://stackoverflow.com/questions/25867024
        private static byte Alpha(byte source, byte alpha)
        {
            return (byte)((float)source * (float)alpha / (float)byte.MaxValue + 0.5f);
        }

        public static object ResolveXNB(string path)
        {
            Stream file = File.OpenRead(path);

            if (file.IsPossiblyGZipped())
            {
                file = GZIP.Decompress(file);
            }
            var xnbhead = new XNBHeader();
            var reader = new BinaryReader(file);
            if (CheckHeader(reader, xnbhead))
            {
                xnbhead.fileSize = reader.ReadUInt32();
                if (xnbhead.fileSize != reader.BaseStream.Length)
                {
                    Debug.WriteLine("XNB file has been truncated");
                }
                var readerCount = reader.Read();
                var readerArray = new XNBReader[readerCount];
                for (int i = 0; i < readerCount; i++)
                {
                    var xnbread = new XNBReader
                    {
                        reader = reader.ReadString(),
                        version = reader.ReadInt32()
                    };
                    readerArray[i] = xnbread;
                }
                xnbhead.xnbReaders = readerArray;
                var shared = reader.Read();
                var readerindex = reader.Read();
                var type = Type.GetType("AmorousEditor.Readers." + xnbhead.xnbReaders[readerindex - 1].reader.Split(',').First().Split('.').Last());
                if (type != null)
                {
                    return type.GetMethod("Read").Invoke(null, new object[] { reader, xnbhead });
                }
            }
            return null;
        }

        public static bool CheckHeader(BinaryReader reader, XNBHeader xnbhead)
        {
            string magic = Encoding.Default.GetString(reader.ReadBytes(3));
            if (magic == "XNB")
            {
                xnbhead.target = Encoding.Default.GetChars(reader.ReadBytes(1))[0];
                xnbhead.formatVersion = reader.ReadByte();
                byte flags = reader.ReadByte();
                xnbhead.hidef = (flags & 0x1) != 0;
                xnbhead.compressed = (flags & 0x80) != 0;
                return true;
            }
            else
            {
                Debug.WriteLine("Invalid file magic found, expected \"XNB\", found " + magic + '"');
                return false;
            }
        }
    }
}
