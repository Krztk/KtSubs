using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace KtSubs.Wpf
{
    public class Dialog : ObservableObject
    {
        public event CancelEventHandler? CloseHandler;

        public bool DialogStatus { get; private set; }

        public Dialog()
        {
            DialogStatus = false;
        }

        public void Close(bool dialogStatus)
        {
            DialogStatus = dialogStatus;
            CloseHandler?.Invoke(this, new CancelEventArgs());
        }

        public virtual void OnClose(bool dialogResult) { }
        public virtual void OnOpen() { }
    }
}