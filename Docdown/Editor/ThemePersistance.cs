using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docdown.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Docdown.Editor
{
    public static class ThemePersistance
    {
        public static Theme Load(string name)
        {
            var settings = Settings.Default;
            var themeName = "Theme_" + name;
            var themeJson = settings[themeName];

            if (themeJson == null)
            {
                return CreateDefault(name);
            }
            else if (themeJson is string json)
            {
                return FromJson(json);
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public static void Save(Theme theme)
        {
            var name = theme.Name;
            var themeName = "Theme_" + name;
            var json = ToJson(theme);
            var settings = Settings.Default;
            settings[themeName] = json;
            settings.Save();
        }

        private static Theme CreateDefault(string name)
        {
            throw new NotImplementedException();
        }

        private static Theme CreateMarkdown()
        {
            throw new NotImplementedException();
        }

        private static Theme FromJson(string json)
        {
            var jObj = JObject.Parse(json);

            var theme = new Theme();
            var name = (string)jObj.SelectToken("name");
            theme.Name = name;
            var highlights = jObj.SelectToken("highlights");

            foreach (var highlight in highlights)
            {
                var highlightObj = (Highlight)JsonConvert.DeserializeObject(highlight.ToString());
                theme.Add(highlightObj);
            }

            return theme;
        }

        private static string ToJson(Theme theme)
        {
            var themeObj = new JObject
            {
                { "name", theme.Name },
                { "highlights", JsonConvert.SerializeObject(theme.Highlights.ToArray()) }
            };

            return themeObj.ToString();
        }
    }
}
