using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmorousEditor.Misc
{
    static class ImageUtils
    {
        public static void OpenImage( Bitmap bitmap )
        {
            var imageView = new ImageView( bitmap );
            imageView.Show();
        }
    }
}
