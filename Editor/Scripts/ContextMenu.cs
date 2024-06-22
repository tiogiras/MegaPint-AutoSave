#if UNITY_EDITOR
using MegaPint.Editor.Scripts.Windows;
using UnityEditor;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial class used to store MenuItems </summary>
internal static partial class ContextMenu
{
    #region Private Methods

    [MenuItem(MenuItemPackages + "/AutoSave", false, 12)]
    private static void OpenAutoSave()
    {
        TryOpen <AutoSave>(false);
    }

    #endregion
}

}
#endif
