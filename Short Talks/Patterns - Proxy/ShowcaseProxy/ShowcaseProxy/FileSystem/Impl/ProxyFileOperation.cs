using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShowcaseProxy.FileSystem.Impl
{
    public class ProxyFileOperation : IFileOperation
    {
        private string Layer { get; }

        private IFileOperation Operation {get; }
        private FileOperation SimpleFileOperation { get; }

        public ProxyFileOperation(IFileOperation operation, string layer = "my_layer")
        {
            Operation = operation;
            SimpleFileOperation = new FileOperation();
            Layer = layer ?? Guid.NewGuid().ToString();
        }
        
        private string GetRemovedPath(string path)
        {
            return Path.Combine(GetPath(path), "~");
        }
        public string GetPath(string path)
        {
            return Path.Combine(Operation.GetPath(path), Layer);
        }

        public bool IsRemoved(string path, string filename)
        {
            return SimpleFileOperation.IsRemoved(GetPath(path), filename) || !SimpleFileOperation.IsRemoved(GetRemovedPath(path), filename);
        }

        public void Create(string path, string filename)
        {
            if (IsRemoved(path, filename))
            {
                SimpleFileOperation.Create(GetPath(path), filename);
                SimpleFileOperation.Delete(GetRemovedPath(path), filename);
            }
        }

        public string[] Read(string path, string filename)
        {
            var output = new List<string>();
            if (!IsRemoved(path, filename))
            {
                output.AddRange(Operation.Read(path, filename));
                output.AddRange(SimpleFileOperation.Read(GetPath(path), filename));
            }
            return output.ToArray();
        }

        public void Append(string path, string filename, IEnumerable<string> contents)
        {
            Create(path, filename);
            SimpleFileOperation.Append(GetPath(path), filename, contents);
        }

        public void Delete(string path, string filename)
        {
            if (!IsRemoved(path, filename))
            {
                SimpleFileOperation.Create(GetRemovedPath(path), filename);
            }
        }
        
    }
}
