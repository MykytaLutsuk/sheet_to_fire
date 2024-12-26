using UnityEngine;

namespace Shared
{
    public static class FilePicker
    {
        public static string ShowFilePicker(string title, string extension)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", extension);
#else
        Debug.LogError("File picker is not implemented for this platform.");
        return null;
#endif
        }
    }   
}