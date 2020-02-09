using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Docdown.Util;

namespace Docdown.Model
{
    public class PreviewWorkspaceItem : IWorkspaceItem
    {
        public string RelativeName => Name;

        public string Name { get; set; }

        public string FullName { get; set; }

        public IFileSystemInfo FileInfo { get; set; }
        public WorkspaceItemType Type { get; set; }

        public List<IWorkspaceItem> Children => new List<IWorkspaceItem>();

        public IWorkspace Workspace { get; set; }
        public IWorkspaceItem Parent { get; set; }

        public IWorkspaceItem TopParent => this;

        public ConverterType FromType => ConverterType.Undefined;

        public ConverterType ToType { get; set; }

        public bool IsDirectory => false;

        public bool IsFile => true;

        public bool IsExcluded { get; set; }

        public bool IsExcludedEffectively => false;

        public Task<IWorkspaceItem> CopyExistingFolder(string path)
        {
            return Task.FromResult<IWorkspaceItem>(null);
        }

        public Task<IWorkspaceItem> CopyExistingItem(string path)
        {
            return Task.FromResult<IWorkspaceItem>(null);
        }

        public Task<IWorkspaceItem> CreateNewDirectory(string name)
        {
            return Task.FromResult<IWorkspaceItem>(null);
        }

        public Task<IWorkspaceItem> CreateNewFile(string name, string autoExtension = null, byte[] content = null)
        {
            return Task.FromResult<IWorkspaceItem>(null);
        }

        public Task Delete()
        {
            return Task.CompletedTask;
        }

        public byte[] Read()
        {
            return Workspace.FileSystem.File.ReadAllBytes(FullName);
        }

        public Task Rename(string newName)
        {
            return Task.CompletedTask;
        }

        public Task Save(string text)
        {
            return Task.CompletedTask;
        }

        public Task Update()
        {
            return Task.CompletedTask;
        }
    }
}
