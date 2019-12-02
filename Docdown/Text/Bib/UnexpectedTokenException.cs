using System;
using System.Text;

namespace Docdown.Text.Bib
{
    [Serializable]
    public sealed class UnexpectedTokenException : ParseErrorException
    {
        #region Public Property
        /// <summary>
        /// Error message.
        /// </summary>
        public override string Message { get; }

        #endregion

        #region Constructor
        public UnexpectedTokenException(int lineNo, int colNo, TokenType unexpected, params TokenType[] expected)
            : base(lineNo, colNo, 1)
        {
            var errorMsg = new StringBuilder(
                $"Line {lineNo}, Col {colNo}. Unexpected token: {unexpected}. ");
            errorMsg.Append("Expected: ");
            foreach (var item in expected)
            {
                errorMsg.Append($"{item}, ");
            }
            errorMsg.Remove(errorMsg.Length - 2, 2);
            Message = errorMsg.ToString();
        }
        #endregion
    }
}
