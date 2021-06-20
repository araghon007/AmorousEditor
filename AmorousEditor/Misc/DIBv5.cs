using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace AmorousEditor.Misc
{
    static class DIBv5
    {
        #region Conversion stuff thanks to https://github.com/logtcn/greenshot
        // Data structure used pretty much just for DIBv5 conversion

        [StructLayout(LayoutKind.Explicit)]
        public struct BITMAPINFOHEADER
        {
            [FieldOffset(0)]
            public uint biSize;
            [FieldOffset(4)]
            public int biWidth;
            [FieldOffset(8)]
            public int biHeight;
            [FieldOffset(12)]
            public ushort biPlanes;
            [FieldOffset(14)]
            public ushort biBitCount;
            [FieldOffset(16)]
            public BI_COMPRESSION biCompression;
            [FieldOffset(20)]
            public uint biSizeImage;
            [FieldOffset(24)]
            public int biXPelsPerMeter;
            [FieldOffset(28)]
            public int biYPelsPerMeter;
            [FieldOffset(32)]
            public uint biClrUsed;
            [FieldOffset(36)]
            public uint biClrImportant;
            [FieldOffset(40)]
            public uint bV5RedMask;
            [FieldOffset(44)]
            public uint bV5GreenMask;
            [FieldOffset(48)]
            public uint bV5BlueMask;
            [FieldOffset(52)]
            public uint bV5AlphaMask;
            [FieldOffset(56)]
            public uint bV5CSType;
            [FieldOffset(60)]
            public CIEXYZTRIPLE bV5Endpoints;
            [FieldOffset(96)]
            public uint bV5GammaRed;
            [FieldOffset(100)]
            public uint bV5GammaGreen;
            [FieldOffset(104)]
            public uint bV5GammaBlue;
            [FieldOffset(108)]
            public uint bV5Intent;      // Rendering intent for bitmap 
            [FieldOffset(112)]
            public uint bV5ProfileData;
            [FieldOffset(116)]
            public uint bV5ProfileSize;
            [FieldOffset(120)]
            public uint bV5Reserved;

            public const int DIB_RGB_COLORS = 0;

            public BITMAPINFOHEADER(int width, int height, ushort bpp)
            {
                biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));    // BITMAPINFOHEADER < DIBV5 is 40 bytes
                biPlanes = 1;   // Should allways be 1
                biCompression = BI_COMPRESSION.BI_RGB;
                biWidth = width;
                biHeight = height;
                biBitCount = bpp;
                biSizeImage = (uint)(width * height * (bpp >> 3));
                biXPelsPerMeter = 0;
                biYPelsPerMeter = 0;
                biClrUsed = 0;
                biClrImportant = 0;

                // V5
                bV5RedMask = (uint)255 << 16;
                bV5GreenMask = (uint)255 << 8;
                bV5BlueMask = (uint)255;
                bV5AlphaMask = (uint)255 << 24;
                bV5CSType = 1934772034; // sRGB
                bV5Endpoints = new CIEXYZTRIPLE
                {
                    ciexyzBlue = new CIEXYZ(0),
                    ciexyzGreen = new CIEXYZ(0),
                    ciexyzRed = new CIEXYZ(0)
                };
                bV5GammaRed = 0;
                bV5GammaGreen = 0;
                bV5GammaBlue = 0;
                bV5Intent = 4;
                bV5ProfileData = 0;
                bV5ProfileSize = 0;
                bV5Reserved = 0;
            }
            public bool IsDibV5
            {
                get
                {
                    uint sizeOfBMI = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                    return biSize >= sizeOfBMI;
                }
            }
            public uint OffsetToPixels
            {
                get
                {
                    if (biCompression == BI_COMPRESSION.BI_BITFIELDS)
                    {
                        // Add 3x4 bytes for the bitfield color mask
                        return biSize + 3 * 4;
                    }
                    return biSize;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CIEXYZ
        {
            public uint ciexyzX; //FXPT2DOT30
            public uint ciexyzY; //FXPT2DOT30
            public uint ciexyzZ; //FXPT2DOT30
            public CIEXYZ(uint FXPT2DOT30)
            {
                ciexyzX = FXPT2DOT30;
                ciexyzY = FXPT2DOT30;
                ciexyzZ = FXPT2DOT30;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CIEXYZTRIPLE
        {
            public CIEXYZ ciexyzRed;
            public CIEXYZ ciexyzGreen;
            public CIEXYZ ciexyzBlue;
        }

        public enum BI_COMPRESSION : uint
        {
            BI_RGB = 0,         // Uncompressed
            BI_RLE8 = 1,        // RLE 8BPP
            BI_RLE4 = 2,        // RLE 4BPP
            BI_BITFIELDS = 3,   // Specifies that the bitmap is not compressed and that the color table consists of three DWORD color masks that specify the red, green, and blue components, respectively, of each pixel. This is valid when used with 16- and 32-bpp bitmaps.
            BI_JPEG = 4,        // Indicates that the image is a JPEG image.
            BI_PNG = 5          // Indicates that the image is a PNG image.
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BitfieldColorMask
        {
            public uint blue;
            public uint green;
            public uint red;
            public void InitValues()
            {
                red = (uint)255 << 8;
                green = (uint)255 << 16;
                blue = (uint)255 << 24;
            }
        }



        /// <summary>
        /// A helper class which does the mashalling for structs
        /// </summary>
        public static class BinaryStructHelper
        {
            /// <summary>
            /// Get a struct from a byte array
            /// </summary>
            /// <typeparam name="T">typeof struct</typeparam>
            /// <param name="bytes">byte[]</param>
            /// <returns>struct</returns>
            public static T FromByteArray<T>(byte[] bytes) where T : struct
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    int size = Marshal.SizeOf(typeof(T));
                    ptr = Marshal.AllocHGlobal(size);
                    Marshal.Copy(bytes, 0, ptr, size);
                    return FromIntPtr<T>(ptr);
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }

            /// <summary>
            /// Get a struct from a byte array
            /// </summary>
            /// <typeparam name="T">typeof struct</typeparam>
            /// <param name="bytes">byte[]</param>
            /// <returns>struct</returns>
            public static T FromIntPtr<T>(IntPtr intPtr) where T : struct
            {
                object obj = Marshal.PtrToStructure(intPtr, typeof(T));
                return (T)obj;
            }

            /// <summary>
            /// copy a struct to a byte array
            /// </summary>
            /// <typeparam name="T">typeof struct</typeparam>
            /// <param name="obj">struct</param>
            /// <returns>byte[]</returns>
            public static byte[] ToByteArray<T>(T obj) where T : struct
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    int size = Marshal.SizeOf(typeof(T));
                    ptr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(obj, ptr, true);
                    return FromPtrToByteArray<T>(ptr);
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }

            /// <summary>
            /// copy a struct from a pointer to a byte array
            /// </summary>
            /// <typeparam name="T">typeof struct</typeparam>
            /// <param name="ptr">IntPtr to struct</param>
            /// <returns>byte[]</returns>
            public static byte[] FromPtrToByteArray<T>(IntPtr ptr) where T : struct
            {
                int size = Marshal.SizeOf(typeof(T));
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
        }

        /// <summary>
		/// Helper method so get the bitmap bytes
		/// See: http://stackoverflow.com/a/6570155
		/// </summary>
		/// <param name="bitmap">Bitmap</param>
		/// <returns>byte[]</returns>
		private static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int absStride = Math.Abs(bmpData.Stride);
            int bytes = absStride * bitmap.Height;
            long ptr = bmpData.Scan0.ToInt64();

            // Declare an array to hold the bytes of the bitmap.
            byte[] rgbValues = new byte[bytes];

            for (int i = 0; i < bitmap.Height; i++)
            {
                IntPtr pointer = new IntPtr(ptr + (bmpData.Stride * i));
                Marshal.Copy(pointer, rgbValues, absStride * (bitmap.Height - i - 1), absStride);
            }

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            return rgbValues;
        }

        #endregion

        /// <summary>
        /// Converts Bitmap to DIBv5
        /// </summary>
        /// <param name="bmp">Bitmap to be converted</param>
        /// <returns>Converted DIBv5 MemoryStream</returns>
        public static MemoryStream BitmapToDIBv5(Bitmap bmp)
        {
            // Create the stream for the clipboard
            var dibV5Stream = new MemoryStream();

            // Create the BITMAPINFOHEADER
            BITMAPINFOHEADER header = new BITMAPINFOHEADER(bmp.Width, bmp.Height, 32)
            {
                // Make sure we have BI_BITFIELDS, this seems to be normal for Format17?
                biCompression = BI_COMPRESSION.BI_BITFIELDS
            };
            // Create a byte[] to write
            byte[] headerBytes = BinaryStructHelper.ToByteArray<BITMAPINFOHEADER>(header);

            // Write the BITMAPINFOHEADER to the stream
            dibV5Stream.Write(headerBytes, 0, headerBytes.Length);

            // As we have specified BI_COMPRESSION.BI_BITFIELDS, the BitfieldColorMask needs to be added
            BitfieldColorMask colorMask = new BitfieldColorMask();

            // Make sure the values are set
            colorMask.InitValues();

            // Create the byte[] from the struct
            byte[] colorMaskBytes = BinaryStructHelper.ToByteArray<BitfieldColorMask>(colorMask);
            Array.Reverse(colorMaskBytes);

            // Write to the stream
            dibV5Stream.Write(colorMaskBytes, 0, colorMaskBytes.Length);

            // Create the raw bytes for the pixels only
            byte[] bitmapBytes = BitmapToByteArray((Bitmap)bmp);

            // Write to the stream
            dibV5Stream.Write(bitmapBytes, 0, bitmapBytes.Length);

            return dibV5Stream;
        }
    }
}
