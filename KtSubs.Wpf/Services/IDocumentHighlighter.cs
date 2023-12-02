using KtSubs.Infrastructure.Entries;
using System.Collections.Generic;
using System.Windows.Documents;

namespace KtSubs.Wpf.Services
{
    public interface IDocumentHighlighter
    {
        void Highlight(FlowDocument document, DisplayEntry displayEntry, HashSet<int> indexesToSelect);
    }
}