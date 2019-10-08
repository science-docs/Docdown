using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibTeXLibrary
{
    public class UnexpectedEntryTypeExpection : ParseErrorException
    {
        public string Found { get; }
        public override string Message { get; }

        public UnexpectedEntryTypeExpection(int lineNo, int colNo, string found) : base(lineNo, colNo, found.Length)
        {
            Found = found;
            var errorMsg = $"Line {lineNo}, Col {colNo}. Unexpected entry type: {found}";
            Message = errorMsg;
        }
    }
}
