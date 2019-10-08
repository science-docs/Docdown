using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BibTeXLibrary
{
    using Next = Tuple<ParserState, BibBuilderState>;
    using Action = Dictionary<TokenType, Tuple<ParserState, BibBuilderState>>;
    using StateMap = Dictionary<ParserState, Dictionary<TokenType, Tuple<ParserState, BibBuilderState>>>;

    public sealed class BibParser : IDisposable
    {
        #region Const Field
        /// <summary>
        /// State tranfer map
        /// curState --Token--> (nextState, BibBuilderAction)
        /// </summary>
        private static readonly StateMap StateMap = new StateMap
        {
            {ParserState.Begin,       new Action {
                { TokenType.Start,         new Next(ParserState.InStart,     BibBuilderState.Create) } } },
            {ParserState.InStart,     new Action {
                { TokenType.Name,          new Next(ParserState.InEntry,     BibBuilderState.SetType) } } },
            {ParserState.InEntry,     new Action {
                { TokenType.LeftBrace,     new Next(ParserState.InKey,       BibBuilderState.Skip) } } },
            {ParserState.InKey,       new Action {
                { TokenType.RightBrace,    new Next(ParserState.OutEntry,    BibBuilderState.Build) },
                { TokenType.Name,          new Next(ParserState.OutKey,      BibBuilderState.SetKey) },
                { TokenType.Comma,         new Next(ParserState.InTagName,   BibBuilderState.Skip) } } },
            {ParserState.OutKey,      new Action {
                { TokenType.Comma,         new Next(ParserState.InTagName,   BibBuilderState.Skip) } } },
            {ParserState.InTagName,   new Action {
                { TokenType.Name,          new Next(ParserState.InTagEqual,  BibBuilderState.SetTagName) },
                { TokenType.RightBrace,    new Next(ParserState.OutEntry,    BibBuilderState.Build) } } },
            {ParserState.InTagEqual,  new Action {
                { TokenType.Equal,         new Next(ParserState.InTagValue,  BibBuilderState.Skip) } } },
            {ParserState.InTagValue,  new Action {
                { TokenType.String,        new Next(ParserState.OutTagValue, BibBuilderState.SetTagValue) } } },
            {ParserState.OutTagValue, new Action {
                { TokenType.Concatenation, new Next(ParserState.InTagValue,  BibBuilderState.Skip) },
                { TokenType.Comma,         new Next(ParserState.InTagName,   BibBuilderState.SetTag) },
                { TokenType.RightBrace,    new Next(ParserState.OutEntry,    BibBuilderState.Build) } } },
            {ParserState.OutEntry,    new Action {
                { TokenType.Start,         new Next(ParserState.InStart,     BibBuilderState.Create) } } },
        }; 
        #endregion

        #region Private Field
        /// <summary>
        /// Input text stream.
        /// </summary>
        private readonly TextReader _inputText;

        /// <summary>
        /// Line No. counter.
        /// </summary>
        private int _lineCount = 1;

        /// <summary>
        /// Column counter.
        /// </summary>
        private int _colCount;
        #endregion

        #region Constructor
        public BibParser(TextReader inputText)
        {
            _inputText = inputText;
        }
        #endregion

        #region Public Static Method

        public static List<BibEntry> Parse(string text)
        {
            using (var sr = new StringReader(text))
            {
                return Parse(sr);
            }
        }

        /// <summary>
        /// Parse by given input text reader.
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public static List<BibEntry> Parse(TextReader inputText)
        {
            using (var parser = new BibParser(inputText))
            { 
                return parser.GetAllResult();
            }
        } 
        #endregion

        #region Public Method
        /// <summary>
        /// Get all result from Parser.
        /// </summary>
        /// <returns></returns>
        public List<BibEntry> GetAllResult()
        {
            return Parser().ToList();
        }

        #endregion

        #region Private Method
        private IEnumerable<BibEntry> Parser()
        {
            var curState  = ParserState.Begin;
            var nextState = ParserState.Begin;
            int tokenStart = 0;

            BibEntry bib = null;
            var tagValueBuilder = new StringBuilder();
            var tagName = "";

            // Fetch token from Tokenizer and build BibEntry
            foreach (var token in Tokenizer())
            {
                tokenStart = token.Index;
                // Transfer state
                if(StateMap[curState].ContainsKey(token.Type))
                {
                    nextState = StateMap[curState][token.Type].Item1;
                }
                else
                {
                    var expected = from pair in StateMap[curState]
                                   select pair.Key;
                    throw new UnexpectedTokenException(_lineCount, _colCount, token.Type, expected.ToArray());
                }
                // Build BibEntry
                switch (StateMap[curState][token.Type].Item2)
                {
                    case BibBuilderState.Create:
                        bib = new BibEntry(tokenStart);
                        break;
                    case BibBuilderState.SetType:
                        Debug.Assert(bib != null, "bib != null");
                        if (!Enum.TryParse<EntryType>(token.Value, true, out var result))
                        {
                            throw new UnexpectedEntryTypeExpection(_lineCount, _colCount, token.Value);
                        }
                        bib.Type = token.Value;
                        break;

                    case BibBuilderState.SetKey:
                        Debug.Assert(bib != null, "bib != null");
                        bib.Key = token.Value;
                        break;

                    case BibBuilderState.SetTagName:
                        tagName = token.Value;
                        break;

                    case BibBuilderState.SetTagValue:
                        tagValueBuilder.Append(token.Value);
                        break;

                    case BibBuilderState.SetTag:
                        Debug.Assert(bib != null, "bib != null");
                        bib[tagName] = tagValueBuilder.ToString();
                        tagValueBuilder.Clear();
                        tagName = string.Empty;
                        break;

                    case BibBuilderState.Build:
                        if(tagName != string.Empty)
                        {
                            Debug.Assert(bib != null, "bib != null");
                            bib[tagName] = tagValueBuilder.ToString();
                            tagValueBuilder.Clear();
                            tagName = string.Empty;
                        }
                        bib.SourceLength = tokenStart - bib.SourcePosition + 1;
                        yield return bib;
                        break;
                }
                curState = nextState;
            }
            if(curState != ParserState.OutEntry)
            {
                var expected = from pair in StateMap[curState]
                               select pair.Key;
                throw new UnexpectedTokenException(_lineCount, _colCount, TokenType.EOF, expected.ToArray());
            }
        }

        /// <summary>
        /// Tokenizer for BibTeX entry.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Token> Tokenizer()
        {
            int code;
            int index = 0;
            char c;
            var braceCount = 0;

            while ((code = Peek()) != -1)
            {
                c = (char)code;

                if (c == '%' && _colCount == 0)
                {
                    var line = _inputText.ReadLine();
                    index += line.Length + Environment.NewLine.Length;
                }
                else if (c == '@')
                {
                    yield return new Token(TokenType.Start, index);
                }
                else if (char.IsLetter(c))
                {
                    var value = new StringBuilder();

                    while (true)
                    {
                        c = (char)Read(ref index);
                        value.Append(c);

                        if ((code = Peek()) == -1) break;
                        c = (char)code;

                        if (!char.IsLetterOrDigit(c) &&
                            c != '-' &&
                            c != '.' &&
                            c != '_') break;
                    }
                    yield return new Token(TokenType.Name, index, value.ToString());
                    goto ContinueExcute;
                }
                else if (char.IsDigit(c))
                {
                    var value = new StringBuilder();

                    while (true)
                    {
                        c = (char)Read(ref index);
                        value.Append(c);

                        if ((code = Peek()) == -1) break;
                        c = (char)code;

                        if (!char.IsDigit(c)) break;
                    }
                    yield return new Token(TokenType.String, index, value.ToString());
                    goto ContinueExcute;
                }
                else if (c == '"')
                {
                    var value = new StringBuilder();

                    ReadNoCol(ref index);
                    while ((code = Peek()) != -1)
                    {
                        if (c != '\\' && code == '"') break;

                        c = (char)Read(ref index);
                        value.Append(c);

                    }
                    yield return new Token(TokenType.String, index, value.ToString());
                }
                else if (c == '{')
                {
                    if (braceCount++ == 0)
                    {
                        yield return new Token(TokenType.LeftBrace, index);
                    }
                    else
                    {
                        var value = new StringBuilder();
                        Read(ref index);
                        while (braceCount > 1 && Peek() != -1)
                        {
                            c = (char)Read(ref index);
                            if      (c == '{') braceCount++;
                            else if (c == '}') braceCount--;
                            if (braceCount > 1) value.Append(c);
                        }
                        yield return new Token(TokenType.String, index, value.ToString());
                        goto ContinueExcute;
                    }
                }
                else if (c == '}')
                {
                    braceCount--;
                    yield return new Token(TokenType.RightBrace, index);
                }
                else if (c == ',')
                {
                    yield return new Token(TokenType.Comma, index);
                }
                else if (c == '#')
                {
                    yield return new Token(TokenType.Concatenation, index);
                }
                else if (c == '=')
                {
                    yield return new Token(TokenType.Equal, index);
                }
                else if (c == '\n')
                {
                    _colCount = 0;
                    _lineCount++;
                }
                else if (!char.IsWhiteSpace(c))
                {
                    throw new UnrecognizableCharacterException(_lineCount, _colCount, c);
                }

                // Move to next char if possible
                if (_inputText.Peek() != -1)
                {
                    ReadNoCol(ref index);
                }

                // Don't move
                ContinueExcute:
                ;
            }
        }

        /// <summary>
        /// Peek next char but not move forward.
        /// </summary>
        /// <returns></returns>
        private int Peek()
        {
            return _inputText.Peek();
        }

        /// <summary>
        /// Read next char and move forward.
        /// </summary>
        /// <returns></returns>
        private int Read(ref int index)
        {
            _colCount++;
            return ReadNoCol(ref index);
        }

        private int ReadNoCol(ref int index)
        {
            index++;
            return _inputText.Read();
        }

        #endregion

        #region Impement Interface "IDisposable"
        /// <summary>
        /// Dispose stream resource.
        /// </summary>
        public void Dispose()
        {
            _inputText.Dispose();
        }
        #endregion
    }

    enum ParserState
    {
        Begin,
        InStart,
        InEntry,
        InKey,
        OutKey,
        InTagName,
        InTagEqual,
        InTagValue,
        OutTagValue,
        OutEntry
    }

    enum BibBuilderState
    {
        Create,
        SetType,
        SetKey,
        SetTagName,
        SetTagValue,
        SetTag,
        Build,
        Skip
    }
}
