using System;
using System.Collections.Generic;
using System.Text;

namespace ShowcaseProxy.FileSystem
{
    public interface IFileOperation
    {
        string GetPath(string path);
        bool IsRemoved(string path, string filename);
        void Create(string path, string filename);
        string[] Read(string path, string filename);
        void Append(string path, string filename, IEnumerable<string> contents);
        void Delete(string path, string filename);

    }
}
