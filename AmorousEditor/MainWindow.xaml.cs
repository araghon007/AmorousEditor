using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.IO;

namespace AmorousEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        /// <summary>
        /// Main method, used only for initialization
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // A little hack to save some memory
            Explorer.Navigated += (sender, e) =>
            {
                // If I recall correctly, frames are weird and they cache(?) previously navigated items, and I'd rather have it not do that
                Explorer.NavigationService.RemoveBackEntry();
            };

            Explorer.Unloaded += (sender, e) =>
            {
                Explorer.Content = null;
            };
        }
        
        /// <summary>
        /// Shows file dialog for finding the Amorous executable
        /// </summary>
        private void SelectExe()
        {
            // Creates new OpenFileDialog
            var filediag = new OpenFileDialog
            {
                //Sets the filter to only accept the Amorous executable
                DefaultExt = "Amorous.Game.Windows.exe",
                Filter = "Amorous executable|Amorous.Game.Windows.exe"
            };

            var amorousDir = GetAmorousDir();

            // Tries to open the file dialog in Amorous installation directory
            // If the directory doesn't exist, file dialog is opened in default path
            if (amorousDir != string.Empty)
                filediag.InitialDirectory = amorousDir;

            // If the file has been selected filediag.ShowDialog() should be true, otherwise false
            if (filediag.ShowDialog() == true)
            {
                //Creates new ExplorerPage, which will show up in the Explorer frame and passes the path to the executable to the newly created ExplorerPage
                Explorer.Navigate(new ExplorerPage(filediag.FileName, this));
            }
        }

        /// <summary>
        /// Shows a new file dialog when the Open option in the top menu is selected
        /// </summary>
        private void OpenClick(object sender, RoutedEventArgs e)
        {
            SelectExe();
            e.Handled = true;
        }

        /// <summary>
        /// Exits the application when the Exit option in the top menu is selected
        /// </summary>
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Exit();
            e.Handled = true;
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Called when an object is being dragged onto the window
        /// </summary>
        private void DragExeEnter(object sender, DragEventArgs e)
        {
            // Need to figure out how to do drag and drop properly, to reject any operation other than file drop so it doesn't trigger the drop method
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Checks if the filename of dragged object is the same as the Amorous executable
                if (Path.GetFileName((e.Data.GetData(DataFormats.FileDrop) as string[]).First()) == "Amorous.Game.Windows.exe")
                {
                    //If it is the same, the edited overlay is shown in Explorer frame
                    Explorer.Navigate(new DropOverlay(true));
                }
                else
                {
                    //If it isn't the same, the original overlay is shown in Explorer frame
                    Explorer.Navigate(new DropOverlay(false));
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Called when the dragged object leaves the window
        /// </summary>
        private void DragExeLeave(object sender, DragEventArgs e)
        {
            //Removes everything from the Explorer frame
            Explorer.Content = null;
            e.Handled = true;
        }

        /// <summary>
        /// Called when the dragged object is dropped onto the window
        /// </summary>
        private void DropExe(object sender, DragEventArgs e)
        {
            // For now I'll just do a double check
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Creates new variable which stores the path to the dropped object
                string exePath = (e.Data.GetData(DataFormats.FileDrop) as string[]).First();

                // Checks if the filename of dragged object is the same as the Amorous executable
                if (Path.GetFileName(exePath) == "Amorous.Game.Windows.exe")
                {
                    // If it is the same, new ExplorerPage is created, shown in the Explorer frame and the path to the Amorous executable is passed into it
                    Explorer.Navigate(new ExplorerPage(exePath, this));
                }
                else
                {
                    // If it isn't the same, everything from the Explorer frame is removed
                    Explorer.Content = null;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// If file thumbnails are disabled via menu options, the option to enable/disable file check is set to be checkable
        /// </summary>
        private void FileThumbClick(object sender, RoutedEventArgs e)
        {
            // If file thumbnails are disabled
            if (FileThumb.IsChecked)
            {
                // Make the Type Check option checkable
                TypeCheck.IsEnabled = true;
            }
            else
            {
                // If not, check it and disable checking
                TypeCheck.IsChecked = true;
                TypeCheck.IsEnabled = false;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Called when the About button in menu is clicked, and shows the AboutBox dialog
        /// </summary>
        private void AboutClick(object sender, RoutedEventArgs e)
        {
            var aboutWin = new AboutBox();
            aboutWin.Show();
            e.Handled = true;
        }

        /// <summary>
        /// Gets the Amorous directory by searching Steam library folders.
        /// </summary>
        /// <returns>Amorous directory</returns>
        private string GetAmorousDir()
        {
            // Gets Steam install directory
            var dir = GetSteamDir().Replace("/", @"\");
            
            // Checks in default directory
            if (Directory.Exists(dir + @"\SteamApps\common\Amorous")) 
                return dir + @"\SteamApps\common\Amorous";
            
            // Tries to read libraryfolders.vdf to find Steam's library locations
            if (File.Exists(dir + @"\SteamApps\libraryfolders.vdf"))
            {
                var vdf = File.ReadAllLines(dir + @"\SteamApps\libraryfolders.vdf");
                // Get only the lines regarding library locations which start at the 5th line
                for (int i = 4; i < vdf.Length - 1; i++)
                {
                    var libraries = vdf[i].Split('"');
                    // Need to replace the double slashes from the vdf or else everything breaks (I think)
                    var lib = libraries[3].Replace(@"\\", @"\") + @"\SteamApps\common\Amorous";
                    if (Directory.Exists(lib))
                        return lib;
                }
            }

            // If all fails, return an empty string
            return string.Empty;
        }

        /// <summary>
        /// Gets the Steam directory using registry values.
        /// </summary>
        /// <returns>Steam directory</returns>
        private static string GetSteamDir()
        {
            string dir;
            if ((dir = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null)) != null) 
                return dir;
            if ((dir = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Wow6432Node\Valve\Steam", "InstallPath", null)) != null) 
                return dir;
            if ((dir = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null)) != null) 
                return dir;
            if ((dir = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null)) != null) 
                return dir;
            // If all else fails, just try the default install directory
            return @"C:\Program Files (x86)\Steam";
        }

        /// <summary>
        /// Handles importing images through the toolbar
        /// </summary>
        private void ImportClick(object sender, RoutedEventArgs e)
        {
            if (Explorer.Content is ExplorerPage page && page.Viewer.Content is ExplorerViewPage view)
            {
                // Creates a new OpenFileDialog
                var dialog = new OpenFileDialog()
                {
                    // png
                    DefaultExt = "*.png",
                    // png
                    Filter = "Portable Network Graphics|*.png",
                    // We want all the selections
                    Multiselect = true
                };

                if (dialog.ShowDialog() == true)
                {
                    // Converts each image to XNB and compresses depending on settings
                    foreach(var file in dialog.FileNames)
                        view.ConvertToXNB(file, new FileInfo(Path.Combine(view.Directory.FullName, Path.GetFileNameWithoutExtension(file) + ".xnb")), Compress.IsChecked);
                }
            }
        }
    }
}
