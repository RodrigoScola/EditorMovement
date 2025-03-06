using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Focus
{
    public static class Hierarchy
    {
        private static Transform lastSelected;

        public static bool IsHierarchy(EditorWindow window)
        {
            Type hierarchyType = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor");

            return window is null || hierarchyType != window.GetType() ? false : true;
        }

        static Transform GetLastSelected()
        {
            if (lastSelected is not null)
            {
                return lastSelected;
            }

            return Components.GetRootObjects().FirstOrDefault();
        }

        public static void Down()
        {
            var components = Selection.activeGameObject;
            components ??= GetLastSelected()?.gameObject;

            var nextComponent = Components.PreviousComponent(components.transform);

            nextComponent ??= components.transform.GetChild(0);

            if (nextComponent is not null)
            {
                Selection.activeGameObject = nextComponent.gameObject;
                lastSelected = nextComponent;
            }
        }

        public static void Up()
        {
            var current = Selection.activeGameObject.transform;

            var nextComponent = Components.NextComponent(current);

            nextComponent ??= current.parent.transform;

            if (nextComponent is not null)
            {
                Selection.activeGameObject = nextComponent.gameObject;
                lastSelected = nextComponent;
            }
        }

        public static void Left()
        {
            var current = Selection.activeGameObject.transform;

            if (current.parent is not null)
            {
                Selection.activeGameObject = current.parent.gameObject;
                lastSelected = current.parent;
            }
        }

        public static void Right()
        {
            if (FocusWindow.IsExpanded(Selection.activeGameObject))
            {
                EditorWindow.focusedWindow.SendEvent(Event.KeyboardEvent("left"));
            }
            else
            {
                var e = Event.KeyboardEvent("right");
                EditorWindow.focusedWindow.SendEvent(e);
            }
        }
    }
}
