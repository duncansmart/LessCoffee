using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace DotSmart
{
    static class FileUtil
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        public static string GetShortName(string path)
        {
            StringBuilder lpszShortPath = new StringBuilder();
            uint len = GetShortPathName(path, lpszShortPath, 0);

            if (len == 0)
                throw new Win32Exception();

            lpszShortPath.Capacity = (int)len;
            len = GetShortPathName(path, lpszShortPath, len);
            return lpszShortPath.ToString();
        }
    }
}
