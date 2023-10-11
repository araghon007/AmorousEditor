using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using SynesthesiaM;

namespace AmorousEditor
{
    /// <summary>
    /// Interaction logic for ExplorerPage.xaml
    /// </summary>
    public partial class ExplorerPage
    {
        // Declares variables used for paths to various Amorous folders and the Amorous executable

        /// <summary>
        /// FileInfo for the Amorous Executable
        /// </summary>
        public FileInfo AmorousExe;

        /// <summary>
        /// DirectoryInfo for the Content-Release folder
        /// </summary>
        private DirectoryInfo AmorousContent;

        /// <summary>
        /// DirectoryInfo for the Content-Mods folder
        /// </summary>
        private DirectoryInfo AmorousModContent;

        /// <summary>
        /// DirectoryInfo for the Saves folder
        /// </summary>
        private DirectoryInfo AmorousSaves;

        /// <summary>
        /// DirectoryInfo for the folder used for Exporting
        /// </summary>
        public DirectoryInfo ExportFolder;

        // Declares variables used for settings for the game and the editor

        /// <summary>
        /// Whether or not to show file thumbnails
        /// </summary>
        public bool showThumb;

        /// <summary>
        /// Whether or not to do XNB Type check (Still decompresses file, and shows an icon corresponding with particular file type)
        /// </summary>
        public bool checkType;

        public bool compress;

        /// <summary>
        /// Whether or not to enable NSFW Mode (adds/removes file corresponding with enabling NSFW Mode for Amorous)
        /// </summary>
        public bool NSFWEnabled;

        /// <summary>
        /// Creates Explorer page used for browsing files within the Amorous directory
        /// </summary>
        /// <param name="exe">Path to the Amorous executable</param>
        /// <param name="main">Main window, used for getting settings</param>
        public ExplorerPage(string exe, MainWindow main)
        {
            InitializeComponent();

            // Sets the AmorousExe file by creating FileInfo of the file referenced by the path exe
            AmorousExe = new FileInfo(exe);

            // Sets the AmorousContent directory by creating DirectoryInfo of the Content-Release folder within the Amorous folder
            AmorousContent = new DirectoryInfo(AmorousExe.DirectoryName + @"\Content-Release");

            // Sets the AmorousModContent directory by creating DirectoryInfo of the Content-Mods folder within the Amorous folder
            AmorousModContent = new DirectoryInfo(AmorousExe.DirectoryName + @"\Content-Mods");

            // Sets the AmorousSaves directory by creating DirectoryInfo of the Saves folder within the Amorous folder
            AmorousSaves = new DirectoryInfo(AmorousExe.DirectoryName + @"\Saves");

            // Sets the default export folder (will use user settings later on)
            ExportFolder = new DirectoryInfo(AmorousExe.DirectoryName + @"\Export");

            // Sets variables and binds events used for settings
            showThumb = main.FileThumb.IsChecked;
            main.FileThumb.Click += ThumbClicked;

            checkType = main.TypeCheck.IsChecked;
            main.TypeCheck.Click += TypeClicked;

            compress = main.Compress.IsChecked;
            main.Compress.Click += CompressClicked;

            // NSFW Mode - Checked by default if the file used for enabling NSFW Mode is present
            main.NSFWMode.IsEnabled = true;
            main.NSFWMode.IsChecked = NSFWEnabled = File.Exists(AmorousExe.DirectoryName + @"\ShowMeSomeBooty");
            main.NSFWMode.Click += NSFWClicked;

            Viewer.Navigated += (sender, e) =>
            {
                Viewer.NavigationService.RemoveBackEntry();
            };

            Viewer.Unloaded += (sender, e) =>
            {
                Viewer.Content = null;
            };

            // Creates the folder tree
            MakeTreeContent();
        }

        private void CompressClicked(object sender, RoutedEventArgs e)
        {
            compress = (sender as MenuItem).IsChecked;
            e.Handled = true;
        }

        /// <summary>
        /// Disables file thumbnails if checked
        /// </summary>
        private void ThumbClicked(object sender, RoutedEventArgs e)
        {
            showThumb = (sender as MenuItem).IsChecked;
            e.Handled = true;
        }

        /// <summary>
        /// Disables file checking all together if unchecked
        /// </summary>
        private void TypeClicked(object sender, RoutedEventArgs e)
        {
            checkType = (sender as MenuItem).IsChecked;
            e.Handled = true;
        }

        /// <summary>
        /// Enables/Disables NSFW Mode based on whether the setting is checked or not
        /// </summary>
        private void NSFWClicked(object sender, RoutedEventArgs e)
        {
            if((sender as MenuItem).IsChecked)
            {
                EnableNSFW();
            }
            else
            {
                DisableNSFW();
            }
            MakeTreeContent();
            Viewer.Navigate(null);
            e.Handled = true;
        }

        /// <summary>
        /// Disables NSFW Mode by deleting the file used for enabling NSFW Mode
        /// </summary>
        private void DisableNSFW()
        {
            File.Delete(AmorousExe.DirectoryName + @"\ShowMeSomeBooty");
            NSFWEnabled = false;
        }

        /// <summary>
        /// Enables NSFW Mode by creating the file used for enabling NSFW Mode
        /// </summary>
        private void EnableNSFW()
        {
            File.Create(AmorousExe.DirectoryName + @"\ShowMeSomeBooty");
            NSFWEnabled = true;
        }
        

        /// <summary>
        /// Creates a folder tree (there is probably a better way of doing this)
        /// </summary>
        private void MakeTreeContent()
        {
            ModContentFolder.Items.Clear();
            // For each sub-directory within the Content-Mods directory
            if(AmorousModContent.Exists)
            foreach (DirectoryInfo dir in AmorousModContent.GetDirectories())
            {
                // Creates new TreeViewItem and sets the header as the name of the directory
                TreeViewItem dirtree = new TreeViewItem() { Header = dir.Name };

                // Creates new event handler for when the directory is selected
                dirtree.Selected += new RoutedEventHandler((s, e) => DirClick(s, e, dir));

                // Adds ModContentFolder item into Content folder TreeView
                ModContentFolder.Items.Add(dirtree);

                // Creates new array of DirectoryInfos which are the subfolders of the current subfolder (if any)
                DirectoryInfo[] dirsub = dir.GetDirectories();

                // If there are no more subfolders dirsub should be null
                if (dirsub != null)
                {
                    // Adds more subdirectories of the current directory
                    MakeSubTree(dirsub, dirtree);
                }
            }

            ContentFolder.Items.Clear();
            // For each sub-directory within the Content-Release directory
            if(AmorousContent.Exists)
            foreach (DirectoryInfo dir in AmorousContent.GetDirectories())
            {
                // Creates new TreeViewItem and sets the header as the name of the directory
                TreeViewItem dirtree = new TreeViewItem() { Header = dir.Name };

                // Creates new event handler for when the directory is selected
                dirtree.Selected += new RoutedEventHandler((s, e) => DirClick(s, e, dir));

                // Adds the item into Content folder TreeView
                ContentFolder.Items.Add(dirtree);

                // Creates new array of DirectoryInfos which are the subfolders of the current subfolder (if any)
                DirectoryInfo[] dirsub = dir.GetDirectories();

                // If there are no more subfolders dirsub should be null
                if (dirsub != null)
                {
                    // Adds more subdirectories of the current directory
                    MakeSubTree(dirsub, dirtree);
                }
            }
        }

        /// <summary>
        /// Navigates to selected directory when a folder is double-clicked
        /// </summary>
        private void DirClick(object s, RoutedEventArgs e, DirectoryInfo dir)
        {
            Viewer.Navigate(new ExplorerViewPage(dir, this));
            e.Handled = true;
        }

        /// <summary>
        /// To be entirely honest, I forgot what's going on here
        /// </summary>
        /// <param name="dir">Directory to navigate to</param>
        public void DirNavigate(DirectoryInfo dir)
        {
            Viewer.Navigate(new ExplorerViewPage(dir, this));
            if (dir.FullName.Contains("Content-Mods"))
            {
                Tree.SetSelectedItem("Mods" + dir.FullName.Replace(AmorousModContent.FullName, ""), myConvertFunction);
            }
            else
            {
                Tree.SetSelectedItem("Content" + dir.FullName.Replace(AmorousContent.FullName, ""), myConvertFunction);
            }
        }

        /// <summary>
        /// Same with this, but apparently it's a convert function for something
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string myConvertFunction(object item)
        {
            return (item as TreeViewItem).Header.ToString();
        }

        /// <summary>
        /// Creates a subtree for each folder within a folder (looped(there is probably a much better way of doing this))
        /// </summary>
        private void MakeSubTree(DirectoryInfo[] dirsub, TreeViewItem content)
        {
            // For each sub-directory within the current directory (dirsub)
            foreach (DirectoryInfo dir in dirsub)
            {
                // Checks if nsfw mode is disabled, and if yes, it hides any folders containing the word "Sex"
                if (!(dir.Name.Contains("Sex") && !NSFWEnabled))
                {
                    // Creates new TreeViewItem and sets the header as the name of the directory
                    TreeViewItem dirtree = new TreeViewItem() { Header = dir.Name };

                    // Creates new event handler for when the directory is selected
                    dirtree.Selected += new RoutedEventHandler((s, e) => DirClick(s, e, dir));

                    // Adds the item into current directory TreeViewItem
                    content.Items.Add(dirtree);

                    // Creates new array of DirectoryInfos which are the subfolders of the current subfolder (if any)
                    DirectoryInfo[] dirsub1 = dir.GetDirectories();

                    // If there are no more subfolders dirsub should be null
                    if (dirsub != null)
                    {
                        // Adds more subdirectories of the current directory
                        MakeSubTree(dirsub1, dirtree);
                    }
                }
            }
        }

        /// <summary>
        /// Called when user double clicks the Amorous executable
        /// </summary>
        private void OpenExe(object sender, MouseButtonEventArgs e)
        {
            // Creates new ProcessStartInfo
            var pstart = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    // Path to the Amorous executable
                    FileName = AmorousExe.FullName,

                    // Path to the Amorous directory (required, otherwise the executable would run from the directory the editor was launched from)
                    WorkingDirectory = AmorousExe.DirectoryName,

                    // Sets UseShellExecute to false (required, otherwise the executable would run from the directory the editor was launched from)
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                }
            };

            // Opens Amorous console and runs the process
            var AmorousDebug = new ConsoleEmulation(pstart);
            AmorousDebug.Show();
            e.Handled = true;
        }

        private void ContentFolder_OnSelected(object sender, RoutedEventArgs e)
        {
            Viewer.Navigate(new ExplorerViewPage(AmorousContent, this));
            e.Handled = true;
        }

        private void ModContentFolder_OnSelected(object sender, RoutedEventArgs e)
        {
            try
            {
                Viewer.Navigate(new ExplorerViewPage(AmorousModContent, this));
                e.Handled = true;
            }
            catch
            {

            }
        }
    }
}
