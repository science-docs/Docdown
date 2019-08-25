using Docdown.Editor.Markdown;
using Docdown.Properties;
using System;

namespace Docdown.ViewModel.Commands
{
    public class ChangeDocumentLanguageCommand : DelegateCommand
    {
        public ChangeDocumentLanguageCommand() : base()
        {

        }

        [Delegate]
        public void ChangeDocumentLanguage(string language)
        {
            MarkdownValidator.LoadForbiddenWords(language);
            MarkdownCompletionData.BuildHtmlData(language);
            Settings.Default.DocumentLocale = language;
            Settings.Default.Save();
        }
    }
}
