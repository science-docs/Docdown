using System.Collections.Generic;

namespace PandocMark.Syntax
{
    public sealed class MetaDataEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<MetaDataEntry> Entries { get; } = new List<MetaDataEntry>();
        public int NameStartPosition { get; set; }
        public int NameLength => Name.Length;
        public int ValueStartPosition { get; set; }
        public int ValueLength { get; set; }
    }

    public sealed class MetaData
    {
        public List<MetaDataEntry> Entries { get; } = new List<MetaDataEntry>();
        public int FenceLength { get; set; }
    }
}
