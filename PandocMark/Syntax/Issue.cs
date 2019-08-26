using System;
using System.Collections.Generic;
using System.Text;

namespace PandocMark.Syntax
{
    public enum IssueType
    {
        None,
        Info,
        Warning,
        Error
    }

    public class Issue
    {
        public IssueType Type { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public object Tooltip { get; set; }

        public Issue()
        {

        }

        public Issue(IssueType type, int offset, int length) : this(type, offset, length, null)
        {
        }

        public Issue(IssueType type, int offset, int length, object tooltip)
        {
            Type = type;
            Offset = offset;
            Length = length;
            Tooltip = tooltip;
        }
    }
}
