#if UNITY_EDITOR
using MegaPint.Editor.Scripts.PackageManager.Packages;
using UnityEditor;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial class used to store MenuItems </summary>
internal static partial class ContextMenu
{
    public static class AutoSave
    {
        private static readonly MenuItemSignature s_autoSaveSignature = new()
        {
            package = PackageKey.AutoSave, signature = "AutoSave"
        };

        #region Public Methods

        [MenuItem(MenuItemPackages + "/AutoSave", false, 12)]
        public static void OpenAutoSave()
        {
            TryOpen <Windows.AutoSave>(false, s_autoSaveSignature);
        }

        #endregion
    }
}

}
#endif
