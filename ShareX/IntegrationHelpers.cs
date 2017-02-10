﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2017 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Microsoft.Win32;
using Newtonsoft.Json;
using ShareX.HelpersLib;
using ShareX.Properties;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ShareX
{
    public static class IntegrationHelpers
    {
        private static readonly string ApplicationName = "ShareX";
        private static readonly string ApplicationPath = string.Format("\"{0}\"", Application.ExecutablePath);

        private static readonly string StartupTargetPath =
#if STEAM
            Helpers.GetAbsolutePath("../ShareX_Launcher.exe");
#else
            Application.ExecutablePath;
#endif

        private static readonly string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string StartupRegistryValue = $"\"{StartupTargetPath}\" -silent";

        private static readonly string ShellExtMenuFiles = @"Software\Classes\*\shell\" + ApplicationName;
        private static readonly string ShellExtMenuFilesCmd = ShellExtMenuFiles + @"\command";

        private static readonly string ShellExtMenuDirectory = @"Software\Classes\Directory\shell\" + ApplicationName;
        private static readonly string ShellExtMenuDirectoryCmd = ShellExtMenuDirectory + @"\command";

        private static readonly string ShellExtMenuFolders = @"Software\Classes\Folder\shell\" + ApplicationName;
        private static readonly string ShellExtMenuFoldersCmd = ShellExtMenuFolders + @"\command";

        private static readonly string ShellExtDesc = string.Format("Upload with {0}", ApplicationName); // TODO: Translate
        private static readonly string ShellExtIcon = ApplicationPath + ",0";
        private static readonly string ShellExtPath = ApplicationPath + " \"%1\"";

        private static readonly string ShellCustomUploaderExtensionPath = @"Software\Classes\.sxcu";
        private static readonly string ShellCustomUploaderExtensionValue = "ShareX.sxcu";
        private static readonly string ShellCustomUploaderAssociatePath = @"Software\Classes\" + ShellCustomUploaderExtensionValue;
        private static readonly string ShellCustomUploaderAssociateValue = "ShareX custom uploader";
        private static readonly string ShellCustomUploaderIconPath = ShellCustomUploaderAssociatePath + @"\DefaultIcon";
        private static readonly string ShellCustomUploaderIconValue = ApplicationPath + ",0";
        private static readonly string ShellCustomUploaderCommandPath = ShellCustomUploaderAssociatePath + @"\shell\open\command";
        private static readonly string ShellCustomUploaderCommandValue = ApplicationPath + " -CustomUploader \"%1\"";

        private static readonly string ChromeNativeMessagingHosts = @"SOFTWARE\Google\Chrome\NativeMessagingHosts\com.getsharex.sharex";

        public static bool CheckStartupShortcut()
        {
            return ShortcutHelpers.CheckShortcut(Environment.SpecialFolder.Startup, StartupTargetPath);
        }

        public static void CreateStartupShortcut(bool create)
        {
            ShortcutHelpers.SetShortcut(create, Environment.SpecialFolder.Startup, StartupTargetPath, "-silent");
        }

        public static bool CheckStartWithWindows()
        {
            try
            {
                return RegistryHelpers.CheckRegistry(StartupRegistryPath, ApplicationName, StartupRegistryValue);
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return false;
        }

        public static void CreateStartWithWindows(bool create)
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true))
                {
                    if (rk != null)
                    {
                        if (create)
                        {
                            rk.SetValue(ApplicationName, StartupRegistryValue, RegistryValueKind.String);
                        }
                        else
                        {
                            rk.DeleteValue(ApplicationName, false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }

        public static bool CheckShellContextMenuButton()
        {
            try
            {
                return RegistryHelpers.CheckRegistry(ShellExtMenuFilesCmd, null, ShellExtPath) && RegistryHelpers.CheckRegistry(ShellExtMenuDirectoryCmd, null, ShellExtPath);
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return false;
        }

        public static void CreateShellContextMenuButton(bool create)
        {
            try
            {
                if (create)
                {
                    UnregisterShellContextMenuButton();
                    RegisterShellContextMenuButton();
                }
                else
                {
                    UnregisterShellContextMenuButton();
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }

        private static void RegisterShellContextMenuButton()
        {
            RegistryHelpers.CreateRegistry(ShellExtMenuFiles, ShellExtDesc);
            RegistryHelpers.CreateRegistry(ShellExtMenuFiles, "Icon", ShellExtIcon);
            RegistryHelpers.CreateRegistry(ShellExtMenuFilesCmd, ShellExtPath);

            RegistryHelpers.CreateRegistry(ShellExtMenuDirectory, ShellExtDesc);
            RegistryHelpers.CreateRegistry(ShellExtMenuDirectory, "Icon", ShellExtIcon);
            RegistryHelpers.CreateRegistry(ShellExtMenuDirectoryCmd, ShellExtPath);
        }

        private static void UnregisterShellContextMenuButton()
        {
            RegistryHelpers.RemoveRegistry(ShellExtMenuFiles, true);
            RegistryHelpers.RemoveRegistry(ShellExtMenuDirectory, true);
            RegistryHelpers.RemoveRegistry(ShellExtMenuFolders, true);
        }

        public static bool CheckCustomUploaderExtension()
        {
            try
            {
                return RegistryHelpers.CheckRegistry(ShellCustomUploaderExtensionPath, null, ShellCustomUploaderExtensionValue) &&
                    RegistryHelpers.CheckRegistry(ShellCustomUploaderCommandPath, null, ShellCustomUploaderCommandValue);
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return false;
        }

        public static void CreateCustomUploaderExtension(bool create)
        {
            try
            {
                if (create)
                {
                    UnregisterCustomUploaderExtension();
                    RegisterCustomUploaderExtension();
                }
                else
                {
                    UnregisterCustomUploaderExtension();
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }

        private static void RegisterCustomUploaderExtension()
        {
            RegistryHelpers.CreateRegistry(ShellCustomUploaderExtensionPath, ShellCustomUploaderExtensionValue);
            RegistryHelpers.CreateRegistry(ShellCustomUploaderAssociatePath, ShellCustomUploaderAssociateValue);
            RegistryHelpers.CreateRegistry(ShellCustomUploaderIconPath, ShellCustomUploaderIconValue);
            RegistryHelpers.CreateRegistry(ShellCustomUploaderCommandPath, ShellCustomUploaderCommandValue);

            NativeMethods.SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED, HChangeNotifyFlags.SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }

        private static void UnregisterCustomUploaderExtension()
        {
            RegistryHelpers.RemoveRegistry(ShellCustomUploaderExtensionPath);
            RegistryHelpers.RemoveRegistry(ShellCustomUploaderAssociatePath, true);
        }

        private static void CreateChromeHostManifest(string filepath)
        {
            Helpers.CreateDirectoryFromFilePath(filepath);

            var manifest = new
            {
                name = "com.getsharex.sharex",
                description = "ShareX",
                path = Program.NativeMessagingHostFilePath,
                type = "stdio",
                allowed_origins = new string[] { "chrome-extension://nlkoigbdolhchiicbonbihbphgamnaoc/" }
            };

            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);

            File.WriteAllText(filepath, json, Encoding.UTF8);
        }

        public static void RegisterChromeSupport()
        {
            CreateChromeHostManifest(Program.ChromeHostManifestFilePath);

            RegistryHelpers.CreateRegistry(ChromeNativeMessagingHosts, Program.ChromeHostManifestFilePath);
        }

        public static void UnregisterChromeSupport()
        {
            if (File.Exists(Program.ChromeHostManifestFilePath))
            {
                File.Delete(Program.ChromeHostManifestFilePath);
            }

            RegistryHelpers.RemoveRegistry(ChromeNativeMessagingHosts);
        }

        public static bool CheckSendToMenuButton()
        {
            return ShortcutHelpers.CheckShortcut(Environment.SpecialFolder.SendTo, Application.ExecutablePath);
        }

        public static void CreateSendToMenuButton(bool create)
        {
            ShortcutHelpers.SetShortcut(create, Environment.SpecialFolder.SendTo, Application.ExecutablePath);
        }

        public static bool CheckSteamShowInApp()
        {
            return File.Exists(Program.SteamInAppFilePath);
        }

        public static void SteamShowInApp(bool showInApp)
        {
            string path = Program.SteamInAppFilePath;

            try
            {
                if (showInApp)
                {
                    File.Create(path).Dispose();
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
                MessageBox.Show(e.ToString(), "ShareX - " + Resources.TaskManager_task_UploadCompleted_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(Resources.ApplicationSettingsForm_cbSteamShowInApp_CheckedChanged_For_settings_to_take_effect_ShareX_needs_to_be_reopened_from_Steam_,
                "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Uninstall()
        {
            CreateStartupShortcut(false);
            CreateShellContextMenuButton(false);
            CreateCustomUploaderExtension(false);
            CreateSendToMenuButton(false);
        }
    }
}