using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShowcaseProxy.FileSystem.Impl
{
    public class FileOperation : IFileOperation
    {
        public string GetPath(string path)
        {
            return path;
        }

        public bool IsRemoved(string path, string filename)
        {
            Directory.CreateDirectory(path);
            return !File.Exists(Path.Combine(path, filename));
        }

        public void Create(string path, string filename)
        {
            if (IsRemoved(path, filename))
            {
                File.Create(Path.Combine(path, filename)).Close();
            }
        }

        public string[] Read(string path, string filename)
        {
            return IsRemoved(path, filename) ?  new string[] { } : File.ReadAllLines(Path.Combine(path, filename));
        }

        public void Append(string path, string filename, IEnumerable<string> contents)
        {
            if (!IsRemoved(path, filename))
            {
                File.AppendAllLines(Path.Combine(path, filename), contents);
            }
        }

        public void Delete(string path, string filename)
        {
            if (!IsRemoved(path, filename))
            {
                File.Delete(Path.Combine(path, filename));
            }
        }

    }
}
