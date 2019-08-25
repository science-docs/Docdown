namespace Docdown.Editor.Commands
{
    public class AlphabeticalListCommand : ListCommand
    {
        public AlphabeticalListCommand(ListFinisher finisher) : base(Supplier, finisher)
        {
        }

        private static string Supplier(int marker)
        {
            return ((char)(marker % 26 + 'a')).ToString();
        }
    }
}
