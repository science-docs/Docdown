using Docdown.Editor.Markdown;
using Docdown.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.ViewModel.Commands
{
    public class ChangeLanguageCommand : DelegateCommand
    {
        public ChangeLanguageCommand() : base()
        {

        }

        [Delegate]
        private static void ChangeLanguage(string locale)
        {
            App.ChangeLocale(locale);
            MarkdownCompletionData.BuildHtmlData(locale);
            Settings.Default.Locale = locale;
            Settings.Default.Save();
        }
    }
}
