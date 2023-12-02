using KtSubs.Wpf.Messages;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace KtSubs.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource? source;
        private const int HOTKEY_ID = 8000;
        private const int MOD_NOREPEAT = 0x4000;

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
            RegisterHotKey();
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            uint key = (uint)KeyInterop.VirtualKeyFromKey(Key.F11);
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_NOREPEAT, key))
            {
                // log error
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    if (wParam.ToInt32() == HOTKEY_ID)
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
            UnregisterHotKey();
            base.OnClosed(e);
        }
    }
}