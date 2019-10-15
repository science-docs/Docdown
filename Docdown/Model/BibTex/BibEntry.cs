using System;
using System.Collections.Generic;
using System.Text;

namespace BibTeXLibrary
{
    public class BibEntry
    {
        #region Private Field
        /// <summary>
        /// Entry's type
        /// </summary>
        private EntryType _type;

        /// <summary>
        /// Store all tags
        /// </summary>
        private readonly Dictionary<string, string> _tags = new Dictionary<string, string>();
        #endregion

        #region Public Property

        public List<BibEntryItem> Items { get; } = new List<BibEntryItem>();

        public int SourcePosition { get; set; }

        public int SourceLength { get; set; }

        public string Address
        {
            get { return this["address"]; }
            set { this["address"] = value; }
        }

        public string Annote
        {
            get { return this["annote"]; }
            set { this["annote"] = value; }
        }

        public string Author
        {
            get { return this["author"]; }
            set { this["author"] = value; }
        }

        public string Booktitle
        {
            get { return this["booktitle"]; }
            set { this["booktitle"] = value; }
        }

        public string Chapter
        {
            get { return this["chapter"]; }
            set { this["chapter"] = value; }
        }

        public string Crossref
        {
            get { return this["crossref"]; }
            set { this["crossref"] = value; }
        }

        public string Edition
        {
            get { return this["edition"]; }
            set { this["edition"] = value; }
        }

        public string Editor
        {
            get { return this["editor"]; }
            set { this["editor"] = value; }
        }

        public string Howpublished
        {
            get { return this["howpublished"]; }
            set { this["howpublished"] = value; }
        }

        public string Institution
        {
            get { return this["institution"]; }
            set { this["institution"] = value; }
        }

        public string Journal
        {
            get { return this["journal"]; }
            set { this["journal"] = value; }
        }

        public string Mouth
        {
            get { return this["mouth"]; }
            set { this["mouth"] = value; }
        }

        public string Note
        {
            get { return this["note"]; }
            set { this["note"] = value; }
        }

        public string Number
        {
            get { return this["number"]; }
            set { this["number"] = value; }
        }

        public string Organization
        {
            get { return this["organization"]; }
            set { this["organization"] = value; }
        }

        public string Pages
        {
            get { return this["pages"]; }
            set { this["pages"] = value; }
        }

        public string Publisher
        {
            get { return this["publisher"]; }
            set { this["publisher"] = value; }
        }

        public string School
        {
            get { return this["shcool"]; }
            set { this["school"] = value; }
        }

        public string Series
        {
            get { return this["series"]; }
            set { this["series"] = value; }
        }

        public string Title
        {
            get { return this["title"]; }
            set { this["title"] = value; }
        }

        public string Volume
        {
            get { return this["volume"]; }
            set { this["volume"] = value; }
        }

        public string Year
        {
            get { return this["year"]; }
            set { this["year"] = value; }
        }

        /// <summary>
        /// Entry's type
        /// </summary>
        public string Type
        {
            get
            {
                return Enum.GetName(typeof(EntryType), _type);
            }
            set
            {
                if (Enum.TryParse<EntryType>(value, true, out var result))
                {
                    _type = result;
                }
            }
        }

        /// <summary>
        /// Entry's key
        /// </summary>
        public string Key { get; set; }
        #endregion

        #region Public Method
        /// <summary>
        /// To BibTeX entry
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var bib = new StringBuilder("@");
            bib.Append(Type);
            bib.Append('{');
            bib.Append(Key);
            bib.Append(",");
            bib.Append(Config.LineFeed);

            foreach(var tag in _tags)
            {
                bib.Append(Config.Retract);
                bib.Append(tag.Key);
                bib.Append(" = {");
                bib.Append(tag.Value);
                bib.Append("},");
                bib.Append(Config.LineFeed);
            }

            bib.Append("}");

            return bib.ToString();
        }
        #endregion

        #region Public Indexer
        /// <summary>
        /// Get value by given tagname(index) or
        /// create new tag by index and value.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[string index]
        {
            get
            {
                index = index.ToLower();
                return _tags.ContainsKey(index) ? _tags[index] : string.Empty;
            }
            set
            {
                _tags[index.ToLower()] = value;
            }
        }
        #endregion

        public BibEntry()
        {

        }

        public BibEntry(int sourcePosition)
        {
            SourcePosition = sourcePosition;
        }
    }

    public class BibEntryItem
    {
        public int SourcePosition { get; set; }
        public int SourceLength { get; set; }
        public EntryItemType Type { get; set; }

        public BibEntryItem(int position, int length, EntryItemType type)
        {
            Type = type;
            SourcePosition = position;
            SourceLength = length;
        }
    }

    public enum EntryItemType
    {
        None,
        Type,
        Key,
        Name,
        Value
    }

    public enum EntryType
    {
        Article,
        Book,
        Booklet,
        Conference,
        InBook,
        InCollection,
        InProceedings,
        Manual,
        Mastersthesis,
        Misc,
        PhDThesis,
        Patent,
        Collection,
        Electronic,
        Proceedings,
        TechReport,
        Unpublished,
        Online
    }
}
