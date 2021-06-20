using AmorousEditor.Misc;
using AmorousEditor.Types;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace AmorousEditor.SpineEdit
{
    /// <summary>
    /// Spine Utilities
    /// </summary>
    static class SpineUtils
    {
        /// <summary>
        /// Opens a spine scene in editor
        /// </summary>
        /// <param name="fileInf">Spine scene file to open</param>
        public static void OpenSpine(FileInfo fileInf)
        {
            byte[] file = File.ReadAllBytes(fileInf.FullName);
            byte[] fileAtlas = File.ReadAllBytes(fileInf.FullName.Replace(".json", ".atlas.txt"));
            using (var stream = GZIP.Decompress(file))
            using (var streamAtlas = GZIP.Decompress(fileAtlas))
            {
                using (TextReader json = new StreamReader(stream))
                using (var atlas = new StreamReader(streamAtlas))
                {
                    var bitmaps = new Dictionary<string, Bitmap>();

                    var atlasIndex = new List<string>();

                    while (!atlas.EndOfStream)
                    {
                        atlasIndex.Add(atlas.ReadLine());
                    }

                    string atlasname = "";
                    WriteableBitmap atlasmap = null;
                    foreach (string line in atlasIndex)
                    {
                        if (line.Contains(".png"))
                        {
                            if (atlasname != line)
                            {
                                atlasname = line;
                                atlasmap = (XNB.ResolveXNB(fileInf.DirectoryName + @"\" + atlasname.Replace(".png", ".xnb")) as Texture2D).Texture;
                                bitmaps.Add(atlasname, MiscUtils.BitmapSourceToBitmap(atlasmap));
                            }
                        }
                    }

                    atlas.BaseStream.Seek(0, SeekOrigin.Begin);

                    Bitmap background;

                    try
                    {
                        background = MiscUtils.BitmapSourceToBitmap((XNB.ResolveXNB(fileInf.DirectoryName + @"\Background.xnb") as Texture2D).Texture);
                    }
                    catch
                    {
                        background = null;
                    }


                    var GLTest = new SpineEditor(atlas, fileInf.DirectoryName, json, bitmaps, background);
                    GLTest.Show();
                }
            }
        }
    }
}
