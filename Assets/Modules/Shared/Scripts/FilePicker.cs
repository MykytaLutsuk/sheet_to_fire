using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Shared
{
    public static class FilePicker
    {
        public static string ShowFilePicker(string title, string extension)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", extension);
#elif UNITY_STANDALONE_WIN
            return ShowWindowsFilePicker(title, extension);
#elif UNITY_STANDALONE_OSX
            return ShowMacFilePicker(title, extension);
#elif UNITY_STANDALONE_LINUX
            return ShowLinuxFilePicker(title, extension);
#else
            Debug.LogError("File picker is not implemented for this platform.");
            return null;
#endif
        }

#if UNITY_STANDALONE_WIN
        private static string ShowWindowsFilePicker(string title, string extension)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = title,
                Filter = $"Files (*.{extension})|*.{extension}|All files (*.*)|*.*",
                CheckFileExists = true
            };

            return openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                ? openFileDialog.FileName
                : null;
        }
#endif

#if UNITY_STANDALONE_OSX
        [DllImport("__Internal")]
        private static extern string OpenMacFileDialog(string title, string extension);

        private static string ShowMacFilePicker(string title, string extension)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/usr/bin/osascript",
                        Arguments = $"-e \"set dialogResult to choose file with prompt \"{title}\" of type {{{extension}}}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open file picker on macOS: {ex.Message}");
                return null;
            }
        }
#endif

#if UNITY_STANDALONE_LINUX
        private static string ShowLinuxFilePicker(string title, string extension)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "zenity",
                        Arguments = $"--file-selection --title=\"{title}\" --file-filter=\"*.{extension}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open file picker on Linux: {ex.Message}");
                return null;
            }
        }
#endif
    }
}
