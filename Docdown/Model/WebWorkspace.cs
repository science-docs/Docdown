using Docdown.Properties;
using Docdown.Util;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.Model
{
    public class WebWorkspace : Workspace<WebWorkspaceItem>
    {
        public User User { get; }
        public string Name => Item.Name;

        public override WebWorkspaceItem Item { get; }

        public override WebWorkspaceItem SelectedItem { get; set; }

        public override event WorkspaceChangeEventHandler WorkspaceChanged;

        public WebWorkspace(User user, string name)
        {
            User = user;
            var json = WebUtility.SimpleTextRequest(WebUtility.BuildUrl(Settings.Default.API, "workspace"), 
                MultipartFormParameter.CreateField("name", name), MultipartFormParameter.CreateField("token", user.Token));

            JObject workspaceJson = JObject.Parse(json);

            var files = workspaceJson.SelectToken("files").Values<string>().ToArray();
            var dirs = workspaceJson.SelectToken("directories").Values<string>().Select(e => e + "/").ToArray();

            Item = new WebWorkspaceItem(this, null, name)
            {
                Type = WorkspaceItemType.Web
            };

            foreach (var dir in GetTopLevelItems(dirs, true))
            {
                Item.Children.Add(new WebWorkspaceItem(this, Item, dir, files, dirs));
            }

            foreach (var file in GetTopLevelItems(files, false))
            {
                Item.Children.Add(new WebWorkspaceItem(this, Item, file));
            }
        }

        private string[] GetTopLevelItems(IEnumerable<string> items, bool dirs)
        {
            int count = dirs ? 1 : 0;
            return items.Where(e => e.Count(c => c == '/') == count).ToArray();
        }
    }
}
