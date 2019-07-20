namespace Docdown.ViewModel
{
    public class InputBoxViewModel : ObservableObject
    {
        public string Title { get; set; }

        public string Message { get; set; }
        public string Text { get; set; }

        public InputBoxViewModel()
        {

        }

        public InputBoxViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public InputBoxViewModel(string title, string message, string pretext)
        {
            Title = title;
            Message = message;
            Text = pretext;
        }
    }
}
