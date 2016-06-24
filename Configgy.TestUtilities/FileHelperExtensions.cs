using System;
using System.IO;

namespace Configgy.TestUtilities
{
    public static class FileHelperExtensions
    {
        public static string ToAbsolutePath(this string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }
}
