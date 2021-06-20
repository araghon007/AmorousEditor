/*
 * Copyright (c) 2007 Michael Hansen
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 * 
 * Thanks to DaWanderer at https://www.codeproject.com/Articles/20645/WPF-TreeView-Selection
 * Used for easy selecting of items in TreeView based on selected folder in File Explorer
*/

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;

namespace SynesthesiaM
{
    public static class TreeViewExtensions
    {
        /// <summary>
        /// Selects an item in a TreeView using a path
        /// </summary>
        /// <param name="treeView">The TreeView to select an item in</param>
        /// <param name="path">The path to the selected item.
        /// Components of the path are separated with Path.DirectorySeparatorChar.
        /// Items in the control are converted by calling the ToString method.</param>
        public static void SetSelectedItem(this TreeView treeView, string path)
        {
            treeView.SetSelectedItem(path, item => item.ToString());
        }

        /// <summary>
        /// Selects an item in a TreeView using a path and a custom conversion method
        /// </summary>
        /// <param name="treeView">The TreeView to select an item in</param>
        /// <param name="path">The path to the selected item.
        /// Components of the path are separated with Path.DirectorySeparatorChar.</param>
        /// <param name="convertMethod">A custom method that converts items in the control to their respective path component</param>
        public static void SetSelectedItem(this TreeView treeView, string path,
            Func<object, string> convertMethod)
        {
            treeView.SetSelectedItem(path, convertMethod, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Selects an item in a TreeView using a path and a custom path separator character.
        /// </summary>
        /// <param name="treeView">The TreeView to select an item in</param>
        /// <param name="path">The path to the selected item</param>
        /// <param name="separatorChar">The character that separates path components</param>
        public static void SetSelectedItem(this TreeView treeView, string path,
            char separatorChar)
        {
            treeView.SetSelectedItem(path, item => item.ToString(), separatorChar);
        }

        /// <summary>
        /// Selects an item in a TreeView using a path, a custom conversion method,
        /// and a custom path separator character.
        /// </summary>
        /// <param name="treeView">The TreeView to select an item in</param>
        /// <param name="path">The path to the selected item</param>
        /// <param name="convertMethod">A custom method that converts items in the control to their respective path component</param>
        /// <param name="separatorChar">The character that separates path components</param>
        public static void SetSelectedItem(this TreeView treeView, string path,
            Func<object, string> convertMethod, char separatorChar)
        {
            treeView.SetSelectedItem<string>(
                path.Split(new char[] { separatorChar },
                    StringSplitOptions.RemoveEmptyEntries),
                (x, y) => x == y,
                convertMethod
            );
        }

        /// <summary>
        /// Selects an item in a TreeView using a custom item chain
        /// </summary>
        /// <typeparam name="T">The type of the items present in the control and the chain</typeparam>
        /// <param name="treeView">The TreeView to select an item in</param>
        /// <param name="items">The chain of items to walk. The last item in the chain will be selected</param>
        public static void SetSelectedItem<T>(this TreeView treeView, IEnumerable<T> items)
            where T : class
        {
            // Use a default compare method with the '==' operator
            treeView.SetSelectedItem<T>(items,
                (x, y) => x == y
            );
        }

        /// <summary>
        /// Selects an item in a TreeView using a custom item chain and item comparison method
        /// </summary>
        /// <typeparam name="T">The type of the items present in the control and the chain</typeparam>
        /// <param name="treeView">The TreeView to select an item in</param>
        /// <param name="items">The chain of items to walk. The last item in the chain will be selected</param>
        /// <param name="compareMethod">The method used to compare items in the control with items in the chain</param>
        public static void SetSelectedItem<T>(this TreeView treeView, IEnumerable<T> items,
            Func<T, T, bool> compareMethod)
        {
            treeView.SetSelectedItem<T>(items, compareMethod, null);
        }

        /// <summary>
        /// Selects an item in a TreeView using a custom item chain, an item comparison method,
        /// and an item conversion method.
        /// </summary>
        /// <typeparam name="T">The type of the items present in the control and the chain</typeparam>
        /// <param name="treeView">The TreeView to select an item in</param>
        /// <param name="items">The chain of items to walk. The last item in the chain will be selected</param>
        /// <param name="compareMethod">The method used to compare items in the control with items in the chain</param>
        /// <param name="convertMethod">The method used to convert items in the control to be compared with items in the chain</param>
        public static void SetSelectedItem<T>(this TreeView treeView, IEnumerable<T> items,
            Func<T, T, bool> compareMethod, Func<object, T> convertMethod)
        {
            // Setup default options for a TreeView
            UIUtility.SetSelectedItem<T>(treeView,
                new SetSelectedInfo<T>()
                {
                    Items = items,
                    CompareMethod = compareMethod,
                    ConvertMethod = convertMethod,
                    OnSelected = delegate(ItemsControl container, SetSelectedInfo<T> info)
                    {
                        var treeItem = (TreeViewItem)container;
                        treeItem.IsSelected = true;
                        treeItem.BringIntoView();
                    },
                    OnNeedMoreItems = delegate(ItemsControl container, SetSelectedInfo<T> info)
                    {
                        ((TreeViewItem)container).IsExpanded = true;
                    }
                }
            );
        }
    }
}
