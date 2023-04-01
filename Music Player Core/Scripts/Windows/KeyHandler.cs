#define ReleaseMode
using Music_Player.MusicPanel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;


namespace Music_Player
{
#if ReleaseMode
    public partial class MainWindow : Window
    {
        // Externs
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey([In] IntPtr intPtr, [In] int id, [In] uint fsModifiers, [In] uint vk);
        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey([In] IntPtr intPtr, [In] int id);

        // Constants
#pragma warning disable IDE0051
        private const uint NO_MODIFIER = 0x00;
        private const uint ALT_MODIFIER = 0x01;
        private const uint CTRL_MODIFIER = 0x02;
        private const uint SHIFT_MODIFIER = 0x04;
#pragma warning restore IDE0051

        // Variables
        private HwndSource hwndSource;
        private readonly List<int> registeredIDs = new List<int>();


        // Methods
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowInteropHelper helper = new WindowInteropHelper(this);
            hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(HwndHook);
            RegisterHotKeys();
        }

        private void RegisterHotKeys()
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            RegisterKey(helper.Handle, 9000, CTRL_MODIFIER, (int)Keys.OemPeriod);
            RegisterKey(helper.Handle, 9001, CTRL_MODIFIER, (int)Keys.Oemcomma);
            RegisterKey(helper.Handle, 9002, ALT_MODIFIER, (int)Keys.OemPeriod);
            RegisterKey(helper.Handle, 9003, ALT_MODIFIER, (int)Keys.Oemcomma);
            RegisterKey(helper.Handle, 9004, CTRL_MODIFIER, (int)Keys.OemQuestion);
            RegisterKey(helper.Handle, 9005, CTRL_MODIFIER + ALT_MODIFIER, (int)Keys.Right);
            RegisterKey(helper.Handle, 9006, CTRL_MODIFIER + ALT_MODIFIER, (int)Keys.Left);
            RegisterKey(helper.Handle, 9007, CTRL_MODIFIER + ALT_MODIFIER, (int)Keys.Up);
            RegisterKey(helper.Handle, 9008, CTRL_MODIFIER + ALT_MODIFIER, (int)Keys.Down);
            RegisterKey(helper.Handle, 9009, CTRL_MODIFIER, (int)Keys.M);
            RegisterKey(helper.Handle, 9010, CTRL_MODIFIER, (int)Keys.N);
            RegisterKey(helper.Handle, 9011, NO_MODIFIER, (int)Keys.MediaPlayPause);
            RegisterKey(helper.Handle, 9012, NO_MODIFIER, (int)Keys.MediaNextTrack);
            RegisterKey(helper.Handle, 9013, NO_MODIFIER, (int)Keys.MediaPreviousTrack);
            RegisterKey(helper.Handle, 9014, CTRL_MODIFIER, (int)Keys.OemQuotes);
            RegisterKey(helper.Handle, 9015, CTRL_MODIFIER + SHIFT_MODIFIER, (int)Keys.OemPeriod);
        }

        private void RegisterKey(IntPtr handle, int id, uint modifier, uint key)
        {
            registeredIDs.Add(id);
            RegisterHotKey(handle, id, modifier, key);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                handled = true;

                switch (wParam.ToInt32())
                {
                    case 9000:
                    case 9012:
                        PlaylistControler.Instance.PlayNext();
                        break;
                    case 9001:
                    case 9013:
                        PlaylistControler.Instance.PlayPrevious();
                        break;
                    case 9002:
                        PlaylistControler.Instance.PlayNext(reversed: true);
                        break;
                    case 9003:
                        PlaylistControler.Instance.PlayPrevious(reversed: true);
                        break;
                    case 9004:
                    case 9011:
                        PlaylistControler.Instance.PauseResume();
                        break;
                    case 9005:
                        SkipForwardSeconds();
                        break;
                    case 9006:
                        SkipBackwardSeconds();
                        break;
                    case 9007:
                        VolumeUp();
                        break;
                    case 9008:
                        VolumeDown();
                        break;
                    case 9009:
                        RandomOptionChange();
                        break;
                    case 9010:
                        ChangeRandomStyle();
                        break;
                    case 9014:
                        LoopMusic(Loop_Image);
                        break;
                    case 9015:
                        SkipLooped();
                        break;

                    default:
                        handled = false;
                        break;
                }
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            hwndSource.RemoveHook(HwndHook);
            hwndSource = null;
            UnregisterHotKeys();
            base.OnClosed(e);
        }

        private void UnregisterHotKeys()
        {
            var helper = new WindowInteropHelper(this);

            foreach (int id in registeredIDs)
                UnregisterHotKey(helper.Handle, id);
        }
    }
#endif
}