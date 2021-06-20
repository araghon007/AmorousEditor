using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace AmorousEditor
{
    /// <summary>
    /// Interaction logic for DropOverlay.xaml
    /// </summary>
    public partial class DropOverlay
    {

        // This method requires you to pass a boolean stating whether or not the file that's being dragged is the Amorous executable
        /// <summary>
        /// Creates DropOverlay, which should overlay the main window and should be called when a file is dragged onto it
        /// </summary>
        /// <param name="amorous">A boolean determining whether or not the object being dragged is the Amorous executable</param>
        public DropOverlay(bool amorous)
        {
            InitializeComponent();

            // If the value is true, it changes the text using the code here, otherwise, it uses the original text which you can see in DropOverlay.xaml
            if (amorous)
            {
                // Sets the text
                DropText.Text = "Drop to use";
                Check.Text = "✔";

                // Creates TranslateTransform (Will be used later)
                TranslateTransform animtrans = new TranslateTransform();

                // Sets foreground image for the check mark and sets TileMode to Tile (Will be used for later)
                Check.Foreground = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/AmorousEditor;component/Resources/Images/rainbow.png")),
                    TileMode = TileMode.Tile,
                    Transform = animtrans
                };
                
                // Creates DoubleAnimation which repeats forever
                DoubleAnimation anim = new DoubleAnimation()
                {
                    From = 0,
                    To = 61,
                    Duration = TimeSpan.FromSeconds(4),
                    RepeatBehavior = RepeatBehavior.Forever,
                };

                // Registers TranslateTransform using a name and then sets that as the target for Storyboard
                RegisterName("AnimatedTranslateTransform", animtrans);
                Storyboard.SetTargetName(anim, "AnimatedTranslateTransform");
                Storyboard.SetTargetProperty(anim, new PropertyPath(TranslateTransform.YProperty));

                // Creates Storyboard for the DoubleAnimation created earlier
                Storyboard translationStoryboard = new Storyboard();
                translationStoryboard.Children.Add(anim);

                // Begins the Storyboard once Check has loaded
                Check.Loaded += (s, e) =>
                {
                    // Begins the Storyboard and makes it controllable
                    translationStoryboard.Begin(Check, true);
                    // Seeks the Storyboard based on current time of day, so the animation looks like it's playing even when the overlay isn't shown
                    translationStoryboard.Seek(Check, DateTime.Now.TimeOfDay, TimeSeekOrigin.BeginTime);
                };

                // I tried making it using gradients.. you will be missed forever

                /*
                Check.Foreground = new LinearGradientBrush(new GradientStopCollection()
                {
                    new GradientStop(Color.FromRgb(231,0,0),0d),
                    new GradientStop(Color.FromRgb(231,0,0),1d/6d),
                    new GradientStop(Color.FromRgb(255,140,0),1d/6d),
                    new GradientStop(Color.FromRgb(255,140,0),2d/6d),
                    new GradientStop(Color.FromRgb(255,239,0),2d/6d),
                    new GradientStop(Color.FromRgb(255,239,0),3d/6d),
                    new GradientStop(Color.FromRgb(0,129,31),3d/6d),
                    new GradientStop(Color.FromRgb(0,129,31),4d/6d),
                    new GradientStop(Color.FromRgb(0,68,255),4d/6d),
                    new GradientStop(Color.FromRgb(0,68,255),5d/6d),
                    new GradientStop(Color.FromRgb(118,0,137),5d/6d),
                    new GradientStop(Color.FromRgb(118,0,137),6d/6d)
                }, 90);


                int realindex = 0;
                int index = 0;
                int oh = 0;

                foreach (var stop in (Check.Foreground as LinearGradientBrush).GradientStops)
                {
                    
                    DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames()
                    {
                        Duration = TimeSpan.FromSeconds(4)
                    };
                    if (index == 0)
                    {
                        anim.KeyFrames = new DoubleKeyFrameCollection()
                        {
                            new DiscreteDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))),
                            new LinearDoubleKeyFrame(1,KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))),
                        };
                    }
                    else
                    {
                        anim.KeyFrames = new DoubleKeyFrameCollection()
                        {
                            new DiscreteDoubleKeyFrame(index/6d,KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))),
                            new LinearDoubleKeyFrame(1,KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4-(index/6*4)))),
                            new DiscreteDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4-(index/6*4)))),
                            new LinearDoubleKeyFrame(1-(index/6d),KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))),
                        };
                    }
                    RegisterName("GradientStop" + realindex, stop);
                    var storyboard = new Storyboard
                    {
                        Children = new TimelineCollection()
                        {
                            anim
                        }
                    };
                    Storyboard.SetTargetName(anim, "GradientStop" + realindex);
                    Storyboard.SetTargetProperty(anim, new PropertyPath(GradientStop.OffsetProperty));
                    storyboard.Begin(Check);
                    if (oh == 1) {
                        oh = 0;
                    }
                    else
                    {
                        index++;
                        oh++;
                    }
                    realindex++;
                    Debug.WriteLine(index);
                }
                */
                
            }
        }
    }
}
