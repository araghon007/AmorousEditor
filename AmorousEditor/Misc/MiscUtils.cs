using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace AmorousEditor.Misc
{
    /// <summary>
    /// Miscellaneous Utilities
    /// </summary>
    static class MiscUtils
    {
        /// <summary>
        /// Converts a BitmapSource to Bitmap
        /// </summary>
        /// <param name="bitmapSource">BitmapSource to convert</param>
        /// <returns>A Bitmap converted from the BitmapSource</returns>
        public static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            // Creates new Bitmap based on Width and Height of the BitmapSource
            Bitmap bmp = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, PixelFormat.Format32bppPArgb);

            // Creates BitmapData for the newly created Bitmap
            BitmapData bitdata = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

            // Copies pixel data from the BitmapSource to the Bitmap
            bitmapSource.CopyPixels(Int32Rect.Empty, bitdata.Scan0, bitdata.Height * bitdata.Stride, bitdata.Stride);

            // Unlocks Bitmap bits
            bmp.UnlockBits(bitdata);

            // Returns the converted Bitmap
            return bmp;
        }

        // Method for extracting a full 256x256 icon thanks to SLA80 https://stackoverflow.com/a/1945764

        // Based on: http://www.codeproject.com/KB/cs/IconExtractor.aspx
        // And a hint from: http://www.codeproject.com/KB/cs/IconLib.aspx

        /// <summary>
        /// Extracts the 256x256 PNG from an icon
        /// </summary>
        /// <param name="icoIcon">Icon to extract the image from</param>
        /// <returns>The extracted Bitmap</returns>
        public static Bitmap ExtractVistaIcon(Icon icoIcon)
        {
            Bitmap bmpPngExtracted = null;
            try
            {
                byte[] srcBuf = null;
                using (MemoryStream stream = new MemoryStream())
                { icoIcon.Save(stream); srcBuf = stream.ToArray(); }
                const int SizeICONDIR = 6;
                const int SizeICONDIRENTRY = 16;
                int iCount = BitConverter.ToInt16(srcBuf, 4);
                for (int iIndex = 0; iIndex < iCount; iIndex++)
                {
                    int iWidth = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex];
                    int iHeight = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex + 1];
                    int iBitCount = BitConverter.ToInt16(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 6);
                    if (iWidth == 0 && iHeight == 0 && iBitCount == 32)
                    {
                        int iImageSize = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 8);
                        int iImageOffset = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 12);
                        MemoryStream destStream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(destStream);
                        writer.Write(srcBuf, iImageOffset, iImageSize);
                        destStream.Seek(0, SeekOrigin.Begin);
                        bmpPngExtracted = new Bitmap(destStream); // This is PNG! :) // Not my comment
                        break;
                    }
                }
            }
            catch { return null; }
            return bmpPngExtracted;
        }

        /// <summary>
        /// Same as ExtractVistaIcon, but returns a BitmapSource instead
        /// </summary>
        /// <param name="icoIcon"></param>
        /// <returns></returns>
        public static BitmapSource ExtractVistaIconBitSource(Icon icoIcon)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(ExtractVistaIcon(icoIcon).GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        /// <summary>
        /// Creates a RadioButton
        /// </summary>
        /// <param name="self">Reference to ExplorerViewPage this method should be called from</param>
        /// <param name="image">Icon to use</param>
        /// <param name="name">Name to use</param>
        /// <param name="shadow">Whether or not to show a shadow (used for images)</param>
        /// <returns>RadioButton, styled like icons in default Windows Explorer</returns>
        public static ToggleButton MakeIcon(ExplorerViewPage self, ImageSource image, string name, bool shadow)
        {
            var toggleButton = new ToggleButton()
            {
                Width = 105,
                MaxHeight = 161,
                MinHeight = 116,
                Padding = new Thickness(3, 1, 3, 0),
                Margin = new Thickness(0, 0, 3, 16),
                Style = (Style)self.Resources["RadioStyle"],
            };

            var grid = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                MinWidth = 97,
                MinHeight = 112,
            };

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(96) });
            grid.RowDefinitions.Add(new RowDefinition());

            toggleButton.Content = grid;

            var icon = new System.Windows.Controls.Image()
            {
                Source = image,
                MaxWidth = 96,
                MaxHeight = 96,
                Width = image.Width,
                Height = image.Height,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            grid.Children.Add(icon);
            Grid.SetRow(icon, 0);

            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality);

            var text = new TextBlock()
            {
                Text = name,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.WordEllipsis,
                LineHeight = 15,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                MaxHeight = 64,
            };

            grid.Children.Add(text);
            Grid.SetRow(text, 1);

            toggleButton.Click += new RoutedEventHandler((s, e) =>
            {
                // Gets all selected items
                var radioArr = self.ViewContent.Children.OfType<ToggleButton>().Where(r => r.IsChecked == true);

                // Checks if only Shift or Ctrl are pressed (Has to be done this way, otherwise it would also activate if no modifier keys were pressed
                if ((Keyboard.Modifiers | ModifierKeys.Alt | ModifierKeys.Windows) != (ModifierKeys.Alt | ModifierKeys.Windows))
                {

                }
                else
                {
                    foreach (var radio in radioArr)
                    {
                        if (radio != s)
                        {
                            radio.IsChecked = false;
                        }
                    }
                }
            });

            return toggleButton;
        }
    }
}
