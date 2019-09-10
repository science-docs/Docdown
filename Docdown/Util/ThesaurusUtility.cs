using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docdown.Util
{
    public static class ThesaurusUtility
    {
        public static async Task<string[]> FindSynonym(string word)
        {
            var encodedWord = Uri.EscapeUriString(word);
            var url = "https://www.openthesaurus.de/synonyme/search?q=" + encodedWord + "&format=application/json";
            var text = await WebUtility.SimpleTextRequest(url);
            var jsonObj = JObject.Parse(text);
            var synonyms = new LinkedList<string>();

            var synsets = jsonObj.SelectToken("synsets");
            foreach (var synset in synsets)
            {
                var terms = synset.SelectToken("terms");
                foreach (var term in terms)
                {
                    var termText = term.SelectToken("term").Value<string>();
                    synonyms.AddLast(termText);
                }
            }

            return synonyms.Distinct().ToArray();
        }
    }
}
