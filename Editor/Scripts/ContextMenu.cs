#if UNITY_EDITOR
using Editor.Scripts.Windows;
using UnityEditor;

namespace Editor.Scripts
{
    public static partial class ContextMenu
    {
        [MenuItem("MegaPint/Packages/AutoSave", false, 12)]
        private static void OpenAutoSave() => EditorWindow.GetWindow<MegaPintAutoSave>().ShowWindow();
    }
}
#endif