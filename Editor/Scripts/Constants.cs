﻿#if UNITY_EDITOR
using System.IO;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial lookup table for constants containing AutoSave values  </summary>
internal static partial class Constants
{
    public static class AutoSave
    {
        public static class UserInterface
        {
            private static readonly string s_windows = Path.Combine(s_userInterface, "Windows");
            public static readonly string AutoSaveWindow = Path.Combine(s_windows, "Auto Save");
        }

        private static readonly string s_base = Path.Combine("MegaPint", "AutoSave");
        private static readonly string s_userInterface = Path.Combine(s_base, "User Interface");
    }
}

}
#endif
