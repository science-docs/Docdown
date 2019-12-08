using IWshRuntimeLibrary;
using System;
using System.IO;

namespace Docdown.Installer
{
    public static class Shortcuts
    {
        public static void AddStartMenuShortcut(string path, string name)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string shortcut = Path.Combine(folder, "Programs", name + ".lnk");
            AddShortcut(shortcut, path);
        }

        public static void AddDesktopShortcut(string path, string name)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcut = Path.Combine(folder, name + ".lnk");
            AddShortcut(shortcut, path);
        }

        public static void AddShortcut(string shortcutPath, string path)
        {
            string directory = Path.GetDirectoryName(shortcutPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = path;
            shortcut.Save();
        }
    }
}
