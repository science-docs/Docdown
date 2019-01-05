﻿using Docdown.Util;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Docdown.Model
{
    public class Workspace
    {
        public WorkspaceItem Item { get; set; }
        public WorkspaceItem SelectedItem { get; set; }
        public ConverterType FromType => FromSelectedItem();
        public ConverterType ToType { get; set; }
        public Template SelectedTemplate { get; set; }
        public Template[] Templates { get; set; }

        public Workspace(string path)
        {
            Item = new WorkspaceItem(path);
        }

        private ConverterType FromSelectedItem()
        {
            if (SelectedItem == null)
                return ConverterType.Text;

            switch (SelectedItem.Type)
            {
                case WorkspaceItemType.Markdown:
                    return ConverterType.Markdown;
                case WorkspaceItemType.Latex:
                    return ConverterType.Latex;
                default:
                    return ConverterType.Text;
            }
        }

        public void LoadTemplates()
        {
            string templatesUrl = WebUtility.BuildTemplatesUrl();

            var response = WebUtility.SimpleGetRequest(templatesUrl);

            string text;
            try
            {
                using (var res = WebUtility.SimpleGetRequest(templatesUrl))
                {
                    using (var rs = res.GetResponseStream())
                    {
                        using (var sr = new StreamReader(rs))
                        {
                            text = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                text = "[]";
            }

            Templates = Template.FromJson(text);
        }

        public void SelectTemplate(string name)
        {
            SelectedTemplate = Templates?.FirstOrDefault(e => e.Name == name);
        }

        public override string ToString()
        {
            return Item?.ToString() ?? base.ToString();
        }
    }
}
