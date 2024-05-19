using KtSubs.Wpf.Messages;
using KtSubs.Wpf.Services;
using System;
using System.Windows;
using System.Windows.Interop;

namespace KtSubs.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private HwndSource? source;

        private readonly IEventAggregator eventAggregator;

        public MainView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            this.eventAggregator = eventAggregator;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    if (wParam.ToInt32() == HotkeyManager.HOTKEY_ID)
                    {
                        OnHotKeyPressed();
                        handled = true;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            eventAggregator.SendMessage(new HotkeyPressedMessage());
        }

        protected override void OnClosed(EventArgs e)
        {
            source?.RemoveHook(HwndHook);
            source = null;
            base.OnClosed(e);
        }
    }
}