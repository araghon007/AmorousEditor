/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2013  Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: http://getgreenshot.org/
 * The Greenshot project is hosted on Sourceforge: http://sourceforge.net/projects/greenshot/
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AmorousEditor.Misc
{
    /// <summary>
    /// Taken from ShareX https://github.com/Upload/ShareX/blob/master/Greenshot.ImageEditor/Core/ClipboardHelper.cs
    /// </summary>
    static class ClipboardHelper
    {
        [DllImport("user32", SetLastError = true)]
        public static extern IntPtr GetClipboardOwner32();
        [DllImport("user32", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        /// <summary>
        /// Get the current "ClipboardOwner" but only if it isn't us!
        /// </summary>
        /// <returns>current clipboard owner</returns>
        public static string GetClipboardOwner()
        {
            string owner = null;
            try
            {
                IntPtr hWnd = GetClipboardOwner32();
                if (hWnd != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(hWnd, out int pid);
                    using (Process me = Process.GetCurrentProcess())
                    using (Process ownerProcess = Process.GetProcessById(pid))
                    {
                        // Exclude myself
                        if (ownerProcess != null && me.Id != ownerProcess.Id)
                        {
                            // Get Process Name
                            owner = ownerProcess.ProcessName;
                            // Try to get the starting Process Filename, this might fail.
                            try
                            {
                                owner = ownerProcess.Modules[0].FileName;
                            }
                            catch (Exception)
                            {
                            }
                        }
                        Debug.WriteLine(ownerProcess.ProcessName);
                    }
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Couldn't get clipboard owner");
            }
            return owner;
        }
    }
}
