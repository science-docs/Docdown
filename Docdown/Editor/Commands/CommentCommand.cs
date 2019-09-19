namespace Docdown.Editor.Commands
{
    public class CommentCommand : SorroundCommand
    {
        public CommentCommand() : base("<!--", "-->", true)
        {
        }
    }
}
