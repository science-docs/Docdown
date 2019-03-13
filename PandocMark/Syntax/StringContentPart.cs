namespace PandocMark.Syntax
{
    /// <summary>
    /// Represents a part of <see cref="StringContent"/>.
    /// </summary>
    internal struct StringPart
    {
        public StringPart(string source, int startIndex, int length)
        {
            Source = source;
            StartIndex = startIndex;
            Length = length;
        }

        /// <summary>
        /// Gets or sets the string object this part is created from.
        /// </summary>
        public string Source;

        /// <summary>
        /// Gets or sets the index at which this part starts.
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// Gets or sets the length of the part.
        /// </summary>
        public int Length;

        public override string ToString()
        {
            if (Source is null)
                return null;

            return Source.Substring(StartIndex, Length);
        }
    }
}
