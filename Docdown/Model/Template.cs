using Docdown.Editor.Markdown;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Docdown.Model
{
    public class Template
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public MetaDataModel MetaData
        {
            get => metaData;
            set => metaData = value ?? new MetaDataModel();
        }

        private MetaDataModel metaData = new MetaDataModel();

        public static Template Empty { get; } = new Template();

        public static Template[] FromJson(string json)
        {
            var templates = new List<Template>();
            var array = JArray.Parse(json);

            foreach (var token in array)
            {
                var template = new Template
                {
                    Name = (string)token.SelectToken("name"),
                    Icon = (string)token.SelectToken("icon")
                };
                if (token.SelectToken("meta") is JObject meta)
                {
                    template.MetaData = MetaDataModel.Load(meta);
                }
                else
                {
                    template.MetaData = new MetaDataModel();
                }
                templates.Add(template);
            }
            return templates.ToArray();
        }
    }
}