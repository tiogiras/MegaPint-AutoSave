using System.IO;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial lookup table for constants containing AutoSave values  </summary>
internal static partial class Constants
{
    public static class AutoSave
    {
        public static class Resources
        {
            public static class UserInterface
            {
                public static readonly string WindowsPath = Path.Combine(s_userInterfacePath, "Windows");
            }

            private static readonly string s_userInterfacePath = Path.Combine(s_resourcesPath, "User Interface");
        }

        private static readonly string s_basePath = Path.Combine("MegaPint", "AutoSave");

        private static readonly string s_resourcesPath = Path.Combine(s_basePath, "Resources");
    }
}

}
