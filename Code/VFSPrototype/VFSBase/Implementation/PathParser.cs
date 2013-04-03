using System;
using System.Collections.Generic;
using System.Linq;

namespace VFSBase.Implementation
{
    internal static class PathParser
    {
        private const char PathSeperator = '/';
        private const string PathSeperatorString = "/";
        private static readonly string[] ForbiddenSubstrings = new[] { "\0" };

        public static string NormalizeName(string path)
        {
            return NormalizePath(path.Replace(PathSeperatorString, ""));
        }

        public static string NormalizePath(string path)
        {
            var doublePathSeperator = string.Format("{0}{0}", PathSeperatorString);

            // Remove forbidden substrings
            foreach (var forbiddenSubstring in ForbiddenSubstrings) path = path.Replace(forbiddenSubstring, "");

            // Trims all directory names and file names
            var l = path.Split(new[] { PathSeperator }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            path = string.Join(PathSeperatorString, l);

            // Remove all subsequent PathSeperators
            while (path.Contains(doublePathSeperator)) path = path.Replace(doublePathSeperator, PathSeperatorString);

            // Remove trailing PathSeperator
            if (path.EndsWith(PathSeperatorString)) path = path.Substring(0, path.Length - 1);

            // Remove front PathSeperator
            if (path.StartsWith(PathSeperatorString)) path = path.Substring(1, path.Length - 1);

            return path;
        }

        public static string GetParent(string path)
        {
            var l = NormalizePath(path).Split(PathSeperator).ToList();
            l.RemoveAt(l.Count - 1);
            return string.Join(PathSeperatorString, l);
        }

        public static string GetNodeName(string path)
        {
            return NormalizePath(path).Split(PathSeperator).Last();
        }

        public static IEnumerable<string> SplitPath(string path)
        {
            return NormalizePath(path).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}