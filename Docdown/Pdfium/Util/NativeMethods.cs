﻿using System;
using System.Runtime.InteropServices;

namespace PdfiumViewer.Wpf.Util
{
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
    }
}
