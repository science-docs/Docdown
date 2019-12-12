using System.Collections.Generic;

namespace PandocMark.Syntax
{
    /// <summary>
    /// Contains additional data for document elements. Used in the <see cref="Block.Document"/> property.
    /// </summary>
    public class DocumentData
    {
        /// <summary>
        /// Gets or sets the dictionary containing resolved link references. Only set on the document node, <see langword="null"/>
        /// and not used for all other elements.
        /// </summary>
        public Dictionary<string, Reference> ReferenceMap { get; set; }

        public List<Issue> Issues { get; set; }

        public DocumentData()
        {
            Issues = new List<Issue>();
            ReferenceMap = new Dictionary<string, Reference>();
        }

        public DocumentData(IEnumerable<Reference> references) : this()
        {
            if (references is null)
            {
                throw new System.ArgumentNullException(nameof(references));
            }

            foreach (var reference in references)
            {
                if (!ReferenceMap.ContainsKey(reference.Label))
                {
                    ReferenceMap.Add(reference.Label, reference);
                }
            }
        }

    }
}
