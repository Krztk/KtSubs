using System.Collections.Generic;

namespace KtSubs.Wpf.Messages
{
    public record AddSelectedWordsMessage(IList<string> words);
}