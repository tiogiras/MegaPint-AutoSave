#if UNITY_EDITOR
using Editor.Scripts.Windows;
using UnityEditor;

namespace Editor.Scripts
{
    internal static partial class ContextMenu
    {
        [MenuItem(MenuItemPackages + "/AutoSave", false, 12)]
        private static void OpenAutoSave() => TryOpen<MegaPintAutoSave>(false);
    }
}
#endif