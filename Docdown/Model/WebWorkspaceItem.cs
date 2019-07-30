using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docdown.Properties;
using Docdown.Util;

namespace Docdown.Model
{
    public class WebWorkspaceItem : WorkspaceItem<WebWorkspaceItem>
    {
        public override string Name => Path.GetFileName(fullName);

        public override string FullName => fullName;
        public WebWorkspace Workspace { get; }

        private string fullName;

        public WebWorkspaceItem(WebWorkspace workspace, WebWorkspaceItem parent, string fullName) : this(workspace, parent, fullName, null, null)
        {
        }

        public WebWorkspaceItem(WebWorkspace workspace, WebWorkspaceItem parent, string fullName, string[] files, string[] dirs)
        {
            Parent = parent;
            this.fullName = fullName;
            Workspace = workspace;
            if (fullName.EndsWith("/"))
            {
                Type = WorkspaceItemType.Directory;
                this.fullName = fullName.Remove(fullName.Length - 1);
            }
            else
            {
                SetType(Path.GetExtension(fullName));
            }
            if (IsDirectory && files != null && dirs != null)
            {
                Children.AddRange(CreateChildren(files, dirs));
            }
        }

        private IEnumerable<WebWorkspaceItem> CreateChildren(string[] files, string[] dirs)
        {
            var name = fullName + "/";
            var all = files.Concat(dirs).Where(e => e.Length > name.Length && e.StartsWith(name));
            foreach (var value in all)
            {
                yield return new WebWorkspaceItem(Workspace, this, value);
            }
        }

        public async override Task<string> Convert(CancelToken cancelToken)
        {
            string temp = IOUtility.GetHashFile(FullName);
            var settings = Settings.Default;
            var token = cancelToken?.ToCancellationToken() ?? CancellationToken.None;
            var req = await WebUtility.PostRequest(WebUtility.BuildWorkspaceConvertUrl(), token,
                MultipartFormParameter.ApiParameter(FromType, ToType, settings.Template, settings.Csl, false).Concat(
                MultipartFormParameter.FromWebWorkspace(Workspace)));

            using (var fs = File.Open(temp, FileMode.Create))
            {
                await req.Content.CopyToAsync(fs);
            }

            return temp;
        }

        public async override Task<WebWorkspaceItem> CopyExistingItem(string path)
        {
            var fileName = Path.GetFileName(path);
            var fullNewName = Path.Combine(FullName, fileName);

            var item = new WebWorkspaceItem(Workspace, this, fullNewName);
            Children.Add(item);
            await item.SaveFile(path);
            return item;
        }

        public async override Task<WebWorkspaceItem> CreateNewDirectory(string name)
        {
            string fullName = name;

            if (Parent != null)
            {
                fullName = FullName + "/" + name;
            }

            if (FindChild(fullName, out var existing))
            {
                return await Task.FromResult(existing);
            }

            fullName += "/";

            var item = new WebWorkspaceItem(Workspace, this, fullName);
            var res = await WebUtility.PostRequest(WebUtility.BuildUrl(Settings.Default.API, "workspace", "folder"),
                MultipartFormParameter.FromWebWorkspace(Workspace).Concat(MultipartFormParameter.CreateField("name", name)));
            await res.Content.ReadAsStreamAsync();
            Children.Add(item);
            return item;
        }

        public async override Task<WebWorkspaceItem> CreateNewFile(string name, string autoExtension = null, byte[] content = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (Type != WorkspaceItemType.Directory && Type != WorkspaceItemType.Web)
                throw new InvalidOperationException("Only Directories can contain files");

            if (autoExtension != null && !name.EndsWith(autoExtension))
            {
                name += autoExtension;
            }

            string fullName = name;

            if (Parent != null)
            {
                fullName = FullName + "/" + name;
            }

            if (content is null)
            {
                content = new byte[0];
            }

            if (FindChild(fullName, out var existing))
            {
                return existing;
            }

            var item = new WebWorkspaceItem(Workspace, this, fullName);

            string temp = IOUtility.GetTempFile();
            File.WriteAllBytes(temp, content);
            var res = await WebUtility.PostRequest(WebUtility.BuildUrl(Settings.Default.API, "workspace", "file"),
                MultipartFormParameter.PostWebWorkspaceItem(item, temp));
            await res.Content.ReadAsStreamAsync();
            Children.Add(item);
            return item;
        }

        public async override Task Delete()
        {
            string endPoint = IsDirectory ? "folder" : "file";
            var response = await WebUtility.DeleteRequest(WebUtility.BuildUrl(Settings.Default.API, "workspace", endPoint),
                MultipartFormParameter.GetWebWorkspaceItem(this));
            await response.Content.ReadAsStreamAsync();
            Parent.Children.Remove(this);
            Parent = null;
        }

        public async override Task<byte[]> Read()
        {
            var response = await WebUtility.GetRequest(WebUtility.BuildUrl(Settings.Default.API, "workspace", "file"),
                MultipartFormParameter.GetWebWorkspaceItem(this));

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async override Task Rename(string newName)
        {
            string fullName = newName;

            if (Parent?.Parent != null)
            {
                fullName = Parent.FullName + "/" + newName;
            }

            string endPoint;
            switch (Type)
            {
                case WorkspaceItemType.Directory:
                    endPoint = "folder";
                    break;
                case WorkspaceItemType.Web:
                    endPoint = null;
                    break;
                default:
                    endPoint = "file";
                    break;
            }
            var response = await WebUtility.MoveRequest(WebUtility.BuildUrl(Settings.Default.API, "workspace", endPoint),
                MultipartFormParameter.GetWebWorkspaceItem(this).Concat(MultipartFormParameter.CreateField("rename", fullName)));
            await response.Content.ReadAsStreamAsync();
            this.fullName = fullName;
        }

        public async override Task Save(string text)
        {
            string temp = IOUtility.GetTempFile();
            File.WriteAllText(temp, text);
            await SaveFile(temp);
        }

        public async Task SaveFile(string filePath)
        {
            var res = await WebUtility.PostRequest(WebUtility.BuildUrl(Settings.Default.API, "workspace", "file"),
                MultipartFormParameter.PostWebWorkspaceItem(this, filePath));
            await res.Content.ReadAsStreamAsync();
        }

        public override Task Update()
        {
            return null;
        }
    }
}
