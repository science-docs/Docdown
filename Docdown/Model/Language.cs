using Docdown.Util;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Docdown.Model
{
    public class Language
    {
        private static readonly Dictionary<string, Language> cache = new Dictionary<string, Language>();

        public string Name { get; }

        public ResourceDictionary Dictionary { get; private set; }

        public static Language Current { get; private set; } = Load("English");

        private Language(string name)
        {
            Name = name;
        }

        public string Get(string key, params object[] formatArgs)
        {
            key = key.Replace('.', '_');
            string text = Dictionary[key] as string;
            if (text != null && formatArgs != null && formatArgs.Length > 0)
            {
                text = string.Format(text, formatArgs);
            }
            return text;
        }

        public static Language Load(string name)
        {
            if (cache.TryGetValue(name, out var lang))
            {
                Current = lang;
                return lang;
            }
            else
            {
                lang = new Language(name);
                lang.LoadDictionary();
                cache[name] = lang;
                Current = lang;
                return lang;
            }
        }

        private void LoadDictionary()
        {
            Dictionary = new ResourceDictionary()
            {
                Source = App.GetResourceUri("Resources/Locale/Template.xaml")
            };

            if (Name != "English")
            {
                Dictionary.Clear();
                var resource = IOUtility.LoadResource($"Docdown.Resources.Locale.{Name}.properties");

                if (resource == null)
                {
                    throw new InvalidProgramException("No translation exists for language: " + Name);
                }

                foreach (var pair in IOUtility.ParseProperties(resource))
                {
                    Dictionary.Add(pair.Item1.Replace('.', '_'), pair.Item2);
                }
            }
        }
    }
}
