using UnityEditor;

namespace Focus
{
    enum Pages
    {
        Shortcuts = 1,
    }

    public class FocusDashboard : EditorWindow
    {
        [MenuItem("Tools/FocusWindow")]
        public static void ShowEditorWindow()
        {
            GetWindow<FocusDashboard>();
        }

        private static Pages selectedPage = Pages.Shortcuts;

        void ShowShortcuts() { }

        public void OnGUI()
        {
            if (selectedPage == Pages.Shortcuts)
            {
                ShowShortcuts();
            }
        }
    }
}
