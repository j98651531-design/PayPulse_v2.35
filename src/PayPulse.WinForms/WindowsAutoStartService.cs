using Microsoft.Win32;
using System.Windows.Forms;

namespace PayPulse.WinForms
{
    /// <summary>
    /// Helper service that toggles 'Run on Windows startup' using HKCU\...\Run.
    /// </summary>
    public class WindowsAutoStartService
    {
                private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private readonly string _appName;

        public WindowsAutoStartService(string appName)
        {
            _appName = string.IsNullOrWhiteSpace(appName) ? "PayPulse" : appName;
        }

        public bool IsEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                if (key == null) return false;
                var value = key.GetValue(_appName) as string;
                return !string.IsNullOrEmpty(value);
            }
        }

        public void Enable()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null) return;
                                                var exePath = "\"" + Application.ExecutablePath + "\"";
                key.SetValue(_appName, exePath);
            }
        }

        public void Disable()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null) return;
                if (key.GetValue(_appName) != null)
                {
                    key.DeleteValue(_appName, false);
                }
            }
        }
    }
}
