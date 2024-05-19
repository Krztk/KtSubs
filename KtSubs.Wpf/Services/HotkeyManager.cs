using System.Runtime.InteropServices;
using System;
using System.Windows.Input;
using Serilog;
using KtSubs.Core.Settings;
using System.Diagnostics;
using System.Globalization;

namespace KtSubs.Wpf.Services
{
    public partial class HotkeyManager
    {
        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnregisterHotKey(
            IntPtr hWnd,
            int id);


        public HotkeyManager(ILogger logger)
        {
            this.logger = logger;
        }

        public const int HOTKEY_ID = 8000;
        const int MOD_NOREPEAT = 0x4000;
        readonly ILogger logger;
        nint windowHandle;

        public Hotkey CurrentHotkey { get; private set; }

        public void InitializeWindowHandle(nint handle)
        {
            windowHandle = handle;
        }

        public void RegisterHotKey(Hotkey hotkey)
        {
            logger.Debug("RegisterHotKey {0}", hotkey);
            var mods = (int)hotkey.Modifiers | MOD_NOREPEAT;
            uint vk = (uint)KeyInterop.VirtualKeyFromKey((Key)hotkey.Key);
            if (!RegisterHotKey(windowHandle, HOTKEY_ID, (uint)mods, vk))
            {
                logger.Error("Cannot register hotkey with 'vk': {0} and modifiers {1}", vk, mods);
                CurrentHotkey = Hotkey.Default();
            } else
            {
                CurrentHotkey = hotkey;
            }
        }

        public void UnregisterHotKey()
        {
            logger.Debug("UnregisterHotKey");
            UnregisterHotKey(windowHandle, HOTKEY_ID);
        }

        public void ChangeSelectionActivatorHotkey(Hotkey hotkey)
        {
            UnregisterHotKey();
            RegisterHotKey(hotkey);
        }
    }
}