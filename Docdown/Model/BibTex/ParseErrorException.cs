using System;

namespace BibTeXLibrary
{
    public abstract class ParseErrorException : ApplicationException
    {
        #region Public Field
        /// <summary>
        /// Line No.
        /// </summary>
        public readonly int LineNo;

        /// <summary>
        /// Col No.
        /// </summary>
        public readonly int ColNo;

        public readonly int Length;
        #endregion

        #region Constructor

        protected ParseErrorException(int lineNo, int colNo, int length)
        {
            LineNo = lineNo;
            ColNo  = colNo;
            Length = length;
        }
        #endregion
    }
}
