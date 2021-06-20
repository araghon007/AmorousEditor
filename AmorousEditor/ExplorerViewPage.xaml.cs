using AmorousEditor.Misc;
using AmorousEditor.SpineEdit;
using AmorousEditor.Types;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AmorousEditor
{
    /// <summary>
    /// Interaction logic for ExplorerViewPage.xaml
    /// </summary>
    public partial class ExplorerViewPage
    {
        /// <summary>
        /// Used to reference main ExplorerPage, for variables and stuff
        /// </summary>
        private readonly ExplorerPage _mainPage;

        /// <summary>
        /// Used to reference the current directory
        /// </summary>
        public DirectoryInfo Directory;

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="dirMain">Current directory</param>
        /// <param name="page"></param>
        public ExplorerViewPage(DirectoryInfo dirMain, ExplorerPage page)
        {

            InitializeComponent();
            
            _mainPage = page;
            
            Directory = dirMain;

            // I was trying to figure out what caused memory leaks and this seemed to lower the memory usage a lot (I know, I'm dumb, so you try optimizing the code)
            // Okay, now that I think about it, there's most likely a way to just clear all the controls and do everything over, instead of creating new page each time
            Unloaded += (s, e) => ViewContent.Children.Clear();

            // Adds a Folder for each directory within the current directory
            foreach (var dir in dirMain.GetDirectories())
            {
                AddFolder(dir);
            }

            // Adds a File for each file within the current directory
            foreach (var file in dirMain.GetFiles())
            {
                AddFile(file);
            }
        }

        /// <summary>
        /// Adds a file (icon, text + controls) to current page
        /// </summary>
        /// <param name="file">File to add</param>
        public void AddFile(FileInfo file)
        {
            // XNB
            if (file.Extension == ".xnb")
            {
                object xnb;
                // Had some edge case here while trying to import an image and then view it, will look into later
                try
                {
                    // Resolves XNB
                    xnb = XNB.ResolveXNB(file.FullName);
                }
                catch
                {
                    xnb = null;
                }

                // Tries to figure out whether the file is compressed or not
                bool compressed = File.OpenRead(file.FullName).IsPossiblyGZipped();

                // Null check
                if(xnb != null)
                {
                    // Texture2D (writing this because there might be more types supported in the future)
                    if (xnb.GetType() == typeof(Texture2D))
                    {
                        // Gets the texture
                        var coolImage = (xnb as Texture2D).Texture;

                        // Creates a RadioButton based on a "template"
                        var toggleButton = MiscUtils.MakeIcon( this, coolImage, $"{file.Name}{(compressed ? "" : "\n(Uncompressed)")}", true);

                        // Context Menus

                        // Creates a new ContextMenu
                        var context = new ContextMenu();

                        // Copy
                        var copy = new MenuItem()
                        {
                            Header = "Copy",
                            ToolTip = "Copies the image into clipboard"
                        };
                        copy.Click += (s, e) => ImageCopy(s, e, coolImage, file.FullName);
                        context.Items.Add(copy);

                        // Export
                        var export = new MenuItem()
                        {
                            Header = "Export",
                            ToolTip = "Allows you to select a folder to export the file into"
                        };
                        export.Click += (s, e) => ImageExport(s, e, coolImage, file.FullName);
                        context.Items.Add(export);

                        // Import
                        var import = new MenuItem()
                        {
                            Header = "Import",
                            ToolTip = "Allows you to select a file to replace the current one"
                        };
                        import.Click += (s, e) => ImageImport(s, e, file);
                        context.Items.Add(import);

                        // Delete
                        var delete = new MenuItem()
                        {
                            Header = "Delete",
                            ToolTip = "Deletes this file"
                        };
                        delete.Click += (s, e) => Delete(s, e, file);
                        context.Items.Add(delete);

                        // Switch based on compression status
                        if (compressed)
                        {
                            // Decompress if compressed
                            var decompress = new MenuItem()
                            {
                                Header = "Decompress",
                                ToolTip = "Decompresses this file"
                            };
                            decompress.Click += (s, e) => Decompress(s, e, file);
                            context.Items.Add(decompress);
                        }
                        else
                        {
                            // Compress if not compressed
                            var compress = new MenuItem()
                            {
                                Header = "Compress",
                                ToolTip = "Compresses this file"
                            };
                            compress.Click += (s, e) => Compress(s, e, file);
                            context.Items.Add(compress);
                        }


                        // Sets ContextMenu of the button to the current ContextMenu
                        toggleButton.ContextMenu = context;

                        // Adds the button to the ViewContent
                        ViewContent.Children.Add(toggleButton);
                    }
                    
                    // If we can't resolve the XNB type
                    else
                    {
                        // Gets an icon
                        using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/file.ico")).Stream)
                        {
                            // Extracts the 256x256 PNG from Icon
                            var icon = MiscUtils.ExtractVistaIconBitSource(new Icon(iconStream));

                            // Creates a RadioButton based on a "template"
                            var toggleButton = MiscUtils.MakeIcon(this, icon, file.Name, false);

                            // Adds the button to the ViewContent
                            ViewContent.Children.Add(toggleButton);
                        }
                    }
                }

                // If the XNB couldn't be resolved
                else
                {
                    // Gets an icon
                    using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/file.ico")).Stream)
                    {
                        // Extracts the 256x256 PNG from Icon
                        var icon = MiscUtils.ExtractVistaIconBitSource(new Icon(iconStream));

                        // Creates a RadioButton based on a "template"
                        var toggleButton = MiscUtils.MakeIcon(this, icon, file.Name, false);

                        // Adds the button to the ViewContent
                        ViewContent.Children.Add(toggleButton);
                    }
                }
            }

            // Spine Scenes
            else if (file.Extension == ".json" && File.Exists(file.FullName.Replace(".json", ".atlas.txt")))
            {
                // Gets an icon
                using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/mightbeuseful.ico")).Stream)
                {
                    // Extracts the 256x256 PNG from Icon
                    var icon = MiscUtils.ExtractVistaIconBitSource(new Icon(iconStream));

                    // Creates a RadioButton based on a "template"
                    var toggleButton = MiscUtils.MakeIcon(this, icon, file.Name, false);

                    // Binds an event for double clicking the button (Opens Spine Editor)
                    toggleButton.MouseDoubleClick += (s, e) => SpineClick(s, e, file);

                    // Adds the button to the ViewContent
                    ViewContent.Children.Add(toggleButton);
                }
            }

            // Spine Atlases, will rewrite the code to use double-click to open
            /*
            else if (file.Name.Contains(".atlas.txt"))
            {
                byte[] atlasfile = File.ReadAllBytes(file.FullName);
                using (MemoryStream decompressed = GZIP.Decompress(atlasfile))
                {
                    var atlasIndex = new List<string>();
                    using (var reader = new StreamReader(decompressed))
                    {
                        while (!reader.EndOfStream)
                        {
                            atlasIndex.Add(reader.ReadLine());
                        }
                    }
                    string atlasname = "";
                    WriteableBitmap atlasmap = null;
                    string outname = "";
                    bool rotate = false;
                    int x = 0;
                    int y = 0;
                    int sizex = 0;
                    int sizey = 0;
                    foreach (string line in atlasIndex)
                    {
                        if (line.Contains(".png"))
                        {
                            if (atlasname == line)
                            {

                            }
                            else
                            {
                                atlasname = line;
                                atlasmap = (WriteableBitmap)XNB.ResolveXNB(file.DirectoryName + @"\" + atlasname.Replace(".png", ".xnb"));
                            }
                            //Console.WriteLine(atlasname);
                        }
                        else if (line.Contains("  "))
                        {
                            if (line.Contains("rotate:"))
                            {
                                var lineas = line.Remove(0, 2);
                                var rotparams = lineas.Split(new String[] { " " }, StringSplitOptions.None);
                                rotate = Convert.ToBoolean(rotparams[1]);
                                //Console.WriteLine("    " + rotate);
                            }
                            else if (line.Contains("xy:"))
                            {
                                var lineas = line.Remove(0, 2);
                                var xyparams = lineas.Split(new String[] { ", ", " " }, StringSplitOptions.None);
                                x = Convert.ToInt32(xyparams[1]);
                                y = Convert.ToInt32(xyparams[2]);
                                //Console.WriteLine("    " + x + ", " + y);
                            }
                            else if (line.Contains("size:"))
                            {
                                var lineas = line.Remove(0, 2);
                                var xyparams = lineas.Split(new String[] { ", ", " " }, StringSplitOptions.None);
                                sizex = Convert.ToInt32(xyparams[1]);
                                sizey = Convert.ToInt32(xyparams[2]);
                                //Console.WriteLine("    " + sizex + ", " + sizey);
                                ExportAtlas(atlasmap, outname, rotate, x, y, sizex, sizey);
                            }
                        }
                        else if (line.Contains("size:") || line.Contains("format:") || line.Contains("filter:") || line.Contains("repeat:"))
                        {

                        }
                        else
                        {
                            outname = line;
                            //Console.WriteLine("  " + outname);
                        }
                    }
                }
            }
            */

            // Everything else
            else
            {
                // Gets an icon
                using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/file.ico")).Stream)
                {
                    // Extracts the 256x256 PNG from Icon
                    var icon = MiscUtils.ExtractVistaIconBitSource(new Icon(iconStream));

                    // Creates a RadioButton based on a "template"
                    var toggleButton = MiscUtils.MakeIcon(this, icon, file.Name, false);

                    // Adds the button to the ViewContent
                    ViewContent.Children.Add(toggleButton);
                }
            }
        }

        /// <summary>
        /// Compresses selected file
        /// </summary>
        /// <param name="file">File to be compressed</param>
        private void Compress(object sender, RoutedEventArgs routedEventArgs, FileInfo file)
        {
            var a = GZIP.Compress(File.ReadAllBytes(file.FullName));

            File.WriteAllBytes(file.FullName, a.ToArray());

            _mainPage.DirNavigate(file.Directory);
        }

        /// <summary>
        /// Decompresses selected file
        /// </summary>
        /// <param name="file">File to be decompressed</param>
        private void Decompress(object sender, RoutedEventArgs routedEventArgs, FileInfo file)
        {
            var a = GZIP.Decompress(File.ReadAllBytes(file.FullName));

            File.WriteAllBytes(file.FullName, a.ToArray());

            _mainPage.DirNavigate(file.Directory);
        }


        /// <summary>
        /// Deletes selected file
        /// </summary>
        /// <param name="file">File to be deleted</param>
        private void Delete(object sender, RoutedEventArgs routedEventArgs, FileInfo file)
        {
            file.Delete();
            _mainPage.DirNavigate(file.Directory);
        }

        /// <summary>
        /// Event handling method for clicking on a spine file
        /// 
        /// Could someone rewrite this documentation to use proper terminology?
        /// </summary>
        /// <param name="fileInf">File to open</param>
        private void SpineClick(object s, MouseButtonEventArgs e, FileInfo fileInf)
        {
            SpineUtils.OpenSpine(fileInf);
            e.Handled = true;
        }
        
        /// <summary>
        /// Allows you to replace an XNB Texture with a PNG file (may add support for other formats later)
        /// </summary>
        /// <param name="file">File to be replaced</param>
        private void ImageImport(object s, RoutedEventArgs e, FileInfo file)
        {
            // Creates a new OpenFileDialog
            var diag = new OpenFileDialog()
            {
                // png // Actually sets the File name to name of the.. well.. file?
                FileName = Path.GetFileNameWithoutExtension(file.FullName) + ".png",
                // png
                DefaultExt = "*.png",
                // png
                Filter = "Portable Network Graphics|*.png"
            };

            // Why do I bother making a documentation
            if (diag.ShowDialog() == true)
            {
                ConvertToXNB(diag.FileName, file);
            }
        }

        /// <summary>
        /// Converts an image to XNB and saves it as destination file
        /// </summary>
        /// <param name="source">Source image path</param>
        /// <param name="destination">Destination XNB file</param>
        public void ConvertToXNB(string source, FileInfo destination, bool compress = true)
        {
            // Gets the Image as a stream
            Stream imageStreamSource = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);

            // PNG Decoder
            var decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            // Conversion to XNB Texture2D
            XNB.ConvertToXNB(decoder.Frames[0], destination.FullName, compress);
            // "Refreshes" the folder view
            _mainPage.DirNavigate(destination.Directory);
        }

        /// <summary>
        /// Allows you to export a XNB Texture to a PNG file (may add support for other formats later, but png is clearly superior (I also suspect whoever made the textures just imported JPGs into the game, and I spite them for that))
        /// </summary>
        /// <param name="coolBitImage">Bitmap to export</param>
        /// <param name="filePath">File path, used only for the filename</param>
        private void ImageExport(object s, RoutedEventArgs e, BitmapSource coolBitImage, string filePath)
        {
            // bmp
            Bitmap bmp = MiscUtils.BitmapSourceToBitmap(coolBitImage);

            // Save file dialog
            var diag = new SaveFileDialog()
            {
                // png (see above)
                FileName = Path.GetFileNameWithoutExtension(filePath) + ".png",
                // png
                DefaultExt = "*.png",
                // png
                Filter = "Portable Network Graphics|*.png"
            };

            // false
            if (diag.ShowDialog() == true)
            {
                // Thankfully, Bitmaps have a Save feature
                bmp.Save(diag.FileName, ImageFormat.Png);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Allows you to copy an image to clipboard with transparency and filename
        /// </summary>
        /// <param name="coolBitImage">Bitmap to copy</param>
        /// <param name="filePath">File path, used only to get the file name</param>
        private static void ImageCopy(object s, RoutedEventArgs e, BitmapSource coolBitImage, string filePath)
        {
            // DataObject used for adding multiple data formats to Clipboard
            var data = new DataObject();

            // Bitmap
            Bitmap bmp = MiscUtils.BitmapSourceToBitmap(coolBitImage);

            data.SetData(DataFormats.Bitmap, bmp, true);
            
            // Create the stream for the clipboard
            var dibV5Stream = DIBv5.BitmapToDIBv5(bmp);

            // Set the DIBv5 to the clipboard DataObject
            data.SetData(DataFormats.Dib, dibV5Stream, true);

            // Taking a note from Chrome and the like to include a link so Discord could use it as the filename
            data.SetData(DataFormats.Html, HtmlFragment.CopyToClipboard("<img src=\"file://amorouseditor.com/" + Path.GetFileNameWithoutExtension(filePath).Replace(' ', '_').Replace("\'", "") + ".png\"/>", "Amorous Editor Clipboard", null));

            // Tries to access clipboard for a number of retries
            int retryCount = 2;
            while (retryCount >= 0)
            {
                try
                {
                    Clipboard.SetDataObject(data, true);
                    break;
                }
                catch (Exception)
                {
                    if (retryCount == 0)
                    {
                        string clipboardOwner = ClipboardHelper.GetClipboardOwner();
                        if (clipboardOwner != null)
                        {
                            Debug.WriteLine("Clipboard in use");

                            // Yes, I'm using MessageBox. Deal with it
                            MessageBox.Show("Couldn't copy image - Clipboard in use", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            Debug.WriteLine("Clipboard error");
                            MessageBox.Show("Couldn't copy image - Clipboard error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                --retryCount;
            }

            e.Handled = true;
        }

        
        /*
        /// <summary>
        /// Misleading name, so far it only adds icons for images within an atlas, I'm planning to make atlases double-clickable and this would be a function to actually export a region from Atlas
        /// </summary>
        /// <param name="atlasmap">Bitmap of the current Atlas</param>
        /// <param name="outname">Output name</param>
        /// <param name="rotate">Whether or not shoud the image be rotated (by 90°)</param>
        /// <param name="x">X coordinate within the Atlas</param>
        /// <param name="y">Y coordinate within the Atlas</param>
        /// <param name="sizex">Width of the region</param>
        /// <param name="sizey">Height of the region</param>
        void ExportAtlas(WriteableBitmap atlasmap, string outname, bool rotate, int x, int y, int sizex, int sizey)
        {
            // Not gonna bother commenting this yet

            BitmapSource outmap;
            if (rotate)
            {
                outmap = new TransformedBitmap(new CroppedBitmap(atlasmap, new Int32Rect(x, y, sizey, sizex)), new RotateTransform(90));
            }
            else
            {
                outmap = new CroppedBitmap(atlasmap, new Int32Rect(x, y, sizex, sizey));
            }

            RadioButton toggleButton = MiscUtils.MakeIcon(this, outmap, outname, false);

            var copy = new MenuItem()
            {
                Header = "Copy",
                ToolTip = "Copies the image into clipboard"
            };

            var export = new MenuItem()
            {
                Header = "Export",
                ToolTip = "Allows you to select a folder to export the file into"
            };

            var import = new MenuItem()
            {
                Header = "Import",
                ToolTip = "Allows you to select a file to replace the current one"
            };

            copy.Click += new RoutedEventHandler((s, e) => ImageCopy(s, e, outmap, outname));

            export.Click += new RoutedEventHandler((s, e) => ImageExport(s, e, outmap, outname));

            var context = new ContextMenu();

            context.Items.Add(copy);

            context.Items.Add(export);

            context.Items.Add(import);

            toggleButton.ContextMenu = context;

            // Adds the button to the ViewContent
            ViewContent.Children.Add(toggleButton);
        }
        */
        /// <summary>
        /// Used for adding folders into view, which can be double-clicked to navigate
        /// </summary>
        /// <param name="dir">Folder to add</param>
        public void AddFolder(DirectoryInfo dir)
        {
            // If NSFW Mode is disabled and the folder contains the word "Sex", it won't be shown (!Very important)
            if(!(!_mainPage.NSFWEnabled && dir.Name.Contains("Sex"))) // I love double negatives
            {
                // Used pretty much everywhere
                using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/AmorousEditor;component/Resources/Icons/directory.ico")).Stream)
                {
                    // Extracts the 256x256 PNG from Icon
                    var icon = MiscUtils.ExtractVistaIconBitSource(new Icon(iconStream));

                    // Creates a RadioButton based on a "template"
                    var toggleButton = MiscUtils.MakeIcon(this, icon, dir.Name, false);

                    // Binds an event for double clicking the button (Navigates to directory)
                    toggleButton.MouseDoubleClick += new MouseButtonEventHandler((s, e) => DirDouble(s, e, dir));

                    // Adds the button to the ViewContent
                    ViewContent.Children.Add(toggleButton);
                }
            }
        }

        /// <summary>
        /// Called when a directory is double-clicked
        /// </summary>
        private void DirDouble(object s, MouseButtonEventArgs e, DirectoryInfo dir)
        {
            // Used to prevent double-clicking using Right Mouse Button
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Navigates to selected directory
                _mainPage.DirNavigate(dir);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Called when a directory is clicked (for selection)
        /// </summary>
        private void ViewClick(object sender, MouseButtonEventArgs e)
        {
            // Gets all selected items
            var radioArr = ViewContent.Children.OfType<ToggleButton>().Where(r => r.IsChecked == true);
            
            // Checks if only Shift or Ctrl are pressed (Has to be done this way, otherwise it would also activate if no modifier keys were pressed
            if((Keyboard.Modifiers | ModifierKeys.Alt | ModifierKeys.Windows) != (ModifierKeys.Alt | ModifierKeys.Windows)){

            }
            else
            {
                foreach (var radio in radioArr)
                {
                    if (e.Source == sender)
                    {
                        radio.IsChecked = false;
                    }
                }
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Converts a file after being dropped
        /// </summary>
        private void OnDrop(object sender, DragEventArgs e)
        {
            foreach (var file in (string[]) e.Data.GetData(DataFormats.FileDrop))
            {
                try
                {
                    ConvertToXNB(file, new FileInfo(Path.Combine(Directory.FullName, Path.GetFileNameWithoutExtension(file) + ".xnb")), _mainPage.compress);
                }
                catch
                {

                }
            }
        }
    }
}
