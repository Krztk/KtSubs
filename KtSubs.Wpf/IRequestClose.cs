using System.ComponentModel;

namespace KtSubs.Wpf
{
    public interface IRequestClose
    {
        void OnClose();

        event CancelEventHandler CloseHandler;
    }
}