using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Shared
{
    public static class FileOpener
    {
        public static void OpenFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.LogError("File path is invalid or the file does not exist.");
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.OpenWithDefaultApp(filePath);
#elif UNITY_STANDALONE_WIN
            // Для Windows відкриваємо файл через стандартну програму
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
#elif UNITY_STANDALONE_OSX
            // Для macOS використовуємо команду "open"
            Process.Start("open", $"\"{filePath}\"");
#elif UNITY_STANDALONE_LINUX
            // Для Linux використовуємо команду "xdg-open"
            Process.Start("xdg-open", filePath);
#else
            Debug.LogError("OpenFile is not implemented for this platform.");
#endif
        }
    }
}