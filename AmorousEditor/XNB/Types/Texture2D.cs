using System.Windows.Media.Imaging;

namespace AmorousEditor.Types
{
    class Texture2D : BaseType
    {
        public WriteableBitmap Texture;
        public Texture2D()
        {
            Type = XNBType.Texture2D;
        }
    }
}
