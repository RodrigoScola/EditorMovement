using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Focus
{
    public class FocusWindow
    {
        public static float Distance(float aX, float aY, float bX, float bY) =>
            Vector2.Distance(new Vector2(aX, aY), new Vector2(bX, bY));

        public static float Distance(Vector2 a, Vector2 b) => Vector2.Distance(a, b);

        public static float Distance(Rect a, Rect b) =>
            Vector2.Distance(new Vector2(a.x, a.y), new Vector2(b.x, b.y));

        public static float Distance(EditorWindow a, EditorWindow b) =>
            Distance(a.position, b.position);

        public static bool Top(EditorWindow a, EditorWindow b) => a.position.y > b.position.y;

        public static bool Bottom(EditorWindow a, EditorWindow b) => a.position.y < b.position.y;

        public static bool Left(EditorWindow a, EditorWindow b) => a.position.x > b.position.x;

        public static bool Right(EditorWindow a, EditorWindow b) => a.position.x < b.position.x;

        public static bool Equal(EditorWindow a, EditorWindow b) =>
            a.GetInstanceID() == b.GetInstanceID();

        public static bool Same(EditorWindow a, EditorWindow b) =>
            a.position.x == b.position.x && a.position.y == b.position.y;

        public static List<EditorWindow> GetDockedWindows(EditorWindow targetWindow)
        {
            Assert.IsNotNull(targetWindow, "window is null");
            List<EditorWindow> dockedWindows = new List<EditorWindow>();

            // Access the internal 'm_Parent' property of the target window
            var parentField = typeof(EditorWindow).GetField(
                "m_Parent",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            if (parentField == null)
            {
                UnityEngine.Debug.LogError("Unable to find 'm_Parent' field.");
                return dockedWindows;
            }

            var parentValue = parentField.GetValue(targetWindow);
            if (parentValue == null)
            {
                UnityEngine.Debug.LogError("Unable to get 'm_Parent' value.");
                return dockedWindows;
            }

            // 'm_Parent' is of type 'DockArea'
            var panesField = parentValue
                .GetType()
                .GetField("m_Panes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (panesField == null)
            {
                UnityEngine.Debug.LogError("Unable to find 'm_Panes' field.");
                return dockedWindows;
            }

            var panesList = panesField.GetValue(parentValue) as System.Collections.IList;
            if (panesList == null)
            {
                UnityEngine.Debug.LogError("Unable to get 'm_Panes' list.");
                return dockedWindows;
            }

            // Iterate through the list of panes (which are EditorWindows)
            foreach (var pane in panesList)
            {
                if (pane is EditorWindow window)
                {
                    dockedWindows.Add(window);
                }
            }

            return dockedWindows;
        }

        public static bool IsWindowToRight(EditorWindow reference, EditorWindow candidate)
        {
            List<EditorWindow> dockedWindows = GetDockedWindows(reference);
            if (dockedWindows.Count == 0)
                return false;

            int referenceIndex = dockedWindows.IndexOf(reference);
            int candidateIndex = dockedWindows.IndexOf(candidate);

            if (referenceIndex == -1 || candidateIndex == -1)
                return false; // One of the windows is not found in the dock

            // A window to the right should be after the reference window in the list
            if (candidateIndex > referenceIndex)
            {
                // Extra check: Ensure it is visually positioned to the right
                Rect referenceRect = reference.position;
                Rect candidateRect = candidate.position;

                return candidateRect.xMin >= referenceRect.xMax;
            }

            return false;
        }

        public static EditorWindow GetActiveTab(EditorWindow window)
        {
            // Access the internal 'm_Parent' property of the EditorWindow
            var parentField = typeof(EditorWindow).GetField(
                "m_Parent",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            if (parentField == null)
            {
                UnityEngine.Debug.LogError("Unable to find 'm_Parent' field.");
                return null;
            }

            var dockArea = parentField.GetValue(window);
            if (dockArea == null)
            {
                UnityEngine.Debug.LogError("Unable to get 'm_Parent' value.");
                return null;
            }

            // Access the 'selected' field of the DockArea
            var selectedField = dockArea
                .GetType()
                .GetField("selected", BindingFlags.NonPublic | BindingFlags.Instance);
            if (selectedField == null)
            {
                UnityEngine.Debug.LogError("Unable to find 'selected' field.");
                return null;
            }

            int selectedIndex = (int)selectedField.GetValue(dockArea);

            // Access the 'm_Panes' field to get the list of docked windows
            var panesField = dockArea
                .GetType()
                .GetField("m_Panes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (panesField == null)
            {
                UnityEngine.Debug.LogError("Unable to find 'm_Panes' field.");
                return null;
            }

            var panesList = panesField.GetValue(dockArea) as System.Collections.IList;
            if (panesList == null || selectedIndex < 0 || selectedIndex >= panesList.Count)
            {
                UnityEngine.Debug.LogError("Invalid 'selected' index or 'm_Panes' list.");
                return null;
            }

            // Return the active EditorWindow
            return panesList[selectedIndex] as EditorWindow;
        }

        public static bool IsExpanded(GameObject obj)
        {
            var _sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType(
                "UnityEditor.SceneHierarchyWindow"
            );
            var _getExpandedIDs = _sceneHierarchyWindowType.GetMethod(
                "GetExpandedIDs",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var _lastInteractedHierarchyWindow = _sceneHierarchyWindowType.GetProperty(
                "lastInteractedHierarchyWindow",
                BindingFlags.Public | BindingFlags.Static
            );
            if (_lastInteractedHierarchyWindow == null)
            {
                return false;
            }
            var _expandedIDs =
                _getExpandedIDs.Invoke(_lastInteractedHierarchyWindow.GetValue(null), null)
                as int[];

            return _expandedIDs.Contains(obj.GetInstanceID());
        }

        private static EditorWindow GetHierarchyWindow()
        {
            Type hierarchyType = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor");
            if (hierarchyType == null)
                return null;

            return EditorWindow.GetWindow(hierarchyType);
        }

        [MenuItem("Tools/Check If Selected Object Is Expanded")]
        public static void CheckIfExpanded()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.Log("No GameObject selected.");
                return;
            }

            bool isExpanded = IsExpanded(selected);
            Debug.Log($"{selected.name} is {(isExpanded ? "expanded" : "collapsed")}");
        }

        public static bool InBounds(Vector2 min, Vector2 max, Vector2 point) =>
            point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y;

        public static void FocusWindowByName(string windowName)
        {
            GetEditorWindowByName(windowName)?.Focus();
        }

        private static EditorWindow GetEditorWindowByName(string windowName)
        {
            // Find all open EditorWindows
            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                // Check if the window title matches the provided name
                if (window.titleContent.text == windowName)
                {
                    return window;
                }
            }
            return null; // If no matching window is found
        }
    }
}
