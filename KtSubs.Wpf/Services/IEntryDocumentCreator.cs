using KtSubs.Infrastructure.Entries;
using System.Windows.Documents;

namespace KtSubs.Wpf.Services
{
    public interface IEntryDocumentCreator
    {
        FlowDocument Create(DisplayEntry displayEntry);
    }
}