#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Editor.Scripts
{
    internal static partial class DisplayContent
    {
        private const string BasePathAutoSave = "AutoSave/User Interface/Display Content Tabs/";
        
        private static void UnsubscribeAutoSave()
        {
            s_onSelectedTabChanged -= OnTabChangedAutoSave;
            s_onSelectedPackageChanged -= UnsubscribeAutoSave;
        }
        
        // Called by reflection
        private static void AutoSave(VisualElement root)
        {
            var tabs = root.Q <GroupBox>("Tabs");
            var tabContentParent = root.Q <GroupBox>("TabContent");

            RegisterTabCallbacks(tabs, tabContentParent, 4);

            SetTabContentLocations(BasePathAutoSave + "Tab0", BasePathAutoSave + "Tab1", BasePathAutoSave + "Tab2", BasePathAutoSave + "Tab3");

            s_onSelectedTabChanged += OnTabChangedAutoSave;
            s_onSelectedPackageChanged += UnsubscribeAutoSave;

            SwitchTab(tabContentParent, 0);
        }
        
        private static void OnTabChangedAutoSave(int tab, VisualElement root)
        {
            
        }
    }
}
#endif