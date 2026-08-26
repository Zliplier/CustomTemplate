using UnityEditor;

namespace HierarchyCustomizer
{
    public static class HierarchyCustomizerMenu
    {
        [MenuItem("Tools/Hierarchy Customizer/Clear All Customizations")]
        private static void ClearAll()
        {
            if (EditorUtility.DisplayDialog(
                "Clear All Customizations",
                "This removes every color and icon assigned in the Hierarchy and Project windows. This cannot be undone.",
                "Clear All", "Cancel"))
            {
                CustomizerDatabase.Instance.ClearAll();
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.RepaintProjectWindow();
            }
        }

        [MenuItem("Tools/Hierarchy Customizer/Select Database Asset")]
        private static void SelectDatabase()
        {
            Selection.activeObject = CustomizerDatabase.Instance;
            EditorGUIUtility.PingObject(CustomizerDatabase.Instance);
        }
    }
}
