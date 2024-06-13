#if UNITY_EDITOR
#if UNITY_INCLUDE_TESTS
using MegaPint.Editor.Scripts.Tests.Utility;
using MegaPint.Editor.Scripts.Windows;
using NUnit.Framework;

namespace MegaPint.Editor.Scripts.Tests
{

/// <summary> Unit tests regarding the menuItems of the package </summary>
internal class MenuItemTests
{
    #region Tests

    [Test] [Order(1)]
    public void AutoSave()
    {
        TestsUtility.ValidateMenuItemLink(Constants.AutoSave.Links.AutoSaveWindow, typeof(AutoSave));
    }

    #endregion
}

}
#endif
#endif
