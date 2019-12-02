namespace Docdown.Text.Bib
{
    internal struct Token
    {
        public TokenType Type;
        public string Value;
        public int Index;

        public Token(TokenType type, int index, string value = "")
        {
            Type = type;
            Index = index;
            Value = value;
        }
    }

    public enum TokenType
    {
        Start,
        
        Name,
        String,

        Quotation,

        LeftBrace,
        RightBrace,

        Equal,
        Comma,

        Concatenation,

        EOF
    }
}
