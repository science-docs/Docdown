using System;
using System.Collections.Generic;
using System.Linq;
using Docdown.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Docdown.Editor
{
    public static class ThemePersistance
    {
        private static readonly Dictionary<string, Theme> internalDict
            = new Dictionary<string, Theme>();

        public static Theme Load(string name)
        {
            if (internalDict.ContainsKey(name))
            {
                return internalDict[name];
            }

            var settings = Settings.Default;
            var themeName = "Theme_" + name;
            settings.EnsureProperty(themeName, string.Empty);
            var themeJson = settings[themeName];

            if (themeJson is string json)
            {
                Theme theme;
                if (string.IsNullOrEmpty(json))
                {
                    theme = CreateDefault(name);
                }
                else
                {
                    theme = FromJson(json);
                }
                internalDict[name] = theme;
                return theme;
            }

            // This will only trigger if the saved setting is not a string
            throw new InvalidCastException();
        }

        public static void Save(Theme theme)
        {
            var name = theme.Name;
            var themeName = "Theme_" + name;
            var json = ToJson(theme);
            var settings = Settings.Default;
            settings.EnsureProperty(themeName, string.Empty);
            settings[themeName] = json;
            settings.Save();
        }

        private static Theme CreateDefault(string name)
        {
            Theme theme;
            switch (name)
            {
                case "Bib":
                case "Markdown":
                    theme = CreateMarkdown();
                    break;
                default:
                    throw new ArgumentException();
            }

            Save(theme);
            return theme;
        }

        private static Theme CreateMarkdown()
        {
            return CreateTheme("Markdown",
                new Highlight { Name = "Heading", Foreground = "#CF6A4C" },
                new Highlight { Name = "Emphasis", Foreground = "#8F9D67" },
                new Highlight { Name = "StrongEmphasis", Foreground = "#8F9D67" },
                new Highlight { Name = "InlineCode", Foreground = "#AC884C" },
                new Highlight { Name = "BlockCode", Foreground = "#AC884C" },
                new Highlight { Name = "BlockQuote", Foreground = "#8F9D67" },
                new Highlight { Name = "Link", Foreground = "#2AA198" },
                new Highlight { Name = "Image", Foreground = "#6F8F3F" },
                new Highlight { Name = "Comment", Foreground = "#5F5A60" },
                new Highlight { Name = "Tex", Foreground = "#3786D4" },
                new Highlight { Name = "Todo", Foreground = "#B8D7A3" });
        }

        private static Theme CreateTheme(string name, params Highlight[] highlights)
        {
            var theme = new Theme
            {
                Name = name
            };

            foreach (var highlight in highlights)
            {
                theme.Add(highlight);
            }

            return theme;
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
                var highlightObj = highlight.ToObject<Highlight>();
                theme.Add(highlightObj);
            }

            return theme;
        }

        private static string ToJson(Theme theme)
        {
            var themeObj = new JObject
            {
                { "name", theme.Name },
                { "highlights", JArray.FromObject(theme.Highlights.ToArray()) }
            };

            return themeObj.ToString();
        }
    }
}
