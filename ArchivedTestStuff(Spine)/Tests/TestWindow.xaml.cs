using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmorousEditor
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {

        public TestWindow(SpineScene scene)
        {
            var bones = new Dictionary<string, SpineBone>();
            InitializeComponent();
            Canvas boneGrid = new Canvas
            {
                Width = scene.Skeleton.Width / 10,
                Height = scene.Skeleton.Height / 10,
                Background = new SolidColorBrush(Color.FromRgb(255,255,0))
            };
            MainGrid.Children.Add(boneGrid);
            foreach (var test in scene.Bones)
            {
                var tempbone = new SpineBone();
                if (test.Parent != null)
                {
                    tempbone.Rotation = bones[test.Parent].Rotation + test.Rotation;
                    tempbone.XY = RotatePoint(new Point(test.X / 10, test.Y / 10), bones[test.Parent].XY, tempbone.Rotation);
                }
                else
                {
                    tempbone.Rotation = test.Rotation;
                    tempbone.XY = RotatePoint(new Point(test.X / 10, test.Y / 10), new Point(0, 0), tempbone.Rotation);
                }
                tempbone.Length = test.Length / 10;
                AddJoint(tempbone.XY, test.Name, tempbone.Length, tempbone.Rotation);
                bones.Add(test.Name, tempbone);
            }
            void AddJoint(Point XY, string name, float length, float rotation)
            {
                Ellipse bone = new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    RenderTransformOrigin = new Point(1, 1),
                    Fill = new SolidColorBrush(Color.FromRgb(255, 0, 255))
                };
                TextBlock text = new TextBlock
                {
                    Text = name,
                };
                Rectangle rect = new Rectangle
                {
                    Width = 2,
                    Height = length,
                    RenderTransformOrigin = new Point(0.5, 1),
                    RenderTransform = new RotateTransform(rotation),
                    Fill = new SolidColorBrush(Color.FromRgb(0, 255, 255))
                };
                boneGrid.Children.Add(bone);
                boneGrid.Children.Add(text);
                boneGrid.Children.Add(rect);
                Canvas.SetRight(bone, XY.X);
                Canvas.SetBottom(bone, XY.Y);
                Canvas.SetRight(text, XY.X+2);
                Canvas.SetBottom(text, XY.Y+2);
                Canvas.SetRight(rect, XY.X);
                Canvas.SetBottom(rect, XY.Y);
            }
        }
        
        static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X = cosTheta * (pointToRotate.X - centerPoint.X) - sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X,
                Y = sinTheta * (pointToRotate.X - centerPoint.X) + cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y
            };
        }
    }

    internal class SpineBone
    {
        public float Length;

        public float Rotation;

        public Point XY;
    }
}
