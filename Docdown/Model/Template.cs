using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Docdown.Model
{
    public class Template
    {
        public string Name { get; set; }

        public static Template Empty { get; } = new Template();

        public static Template[] FromJson(string json)
        {
            List<Template> templates = new List<Template> { Empty };

            JArray array = JArray.Parse(json);

            foreach (var token in array)
            {
                var template = new Template
                {
                    Name = (string)token.SelectToken("name")
                };
                templates.Add(template);
            }
            return templates.ToArray();
        }
    }
}