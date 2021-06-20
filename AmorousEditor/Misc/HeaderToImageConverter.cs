using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AmorousEditor
{
    // This code is from WPF Explorer Tree by Sacha Barber (https://www.codeproject.com/Articles/21248/A-Simple-WPF-Explorer-Tree)

    [ValueConversion(typeof(string), typeof(bool))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value as string).Contains("Amorous.exe"))
            {
                Uri uri = new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/Amorous.ico");
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
            else
            {
                Uri uri = new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/folder.ico");
                using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/directory.ico")).Stream)
                {
                    Icon icon = new Icon(iconStream, 16, 16);
                    return Imaging.CreateBitmapSourceFromHBitmap(icon.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
    
}