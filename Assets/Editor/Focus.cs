using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Focus;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Ex : EditorWindow
{
    public static UserData userdata;
    private static FileDataHandler handler;

    static List<EditorWindow> windows = new();
    static EditorWindow focused;

    static List<EditorWindow> tab = new();

    static Dictionary<Rect, EditorWindow> tabPos = new();

    static Ex()
    {
        EditorApplication.update += TrackActiveTab;

        handler = new(Application.persistentDataPath, "userData.json");

        userdata = handler.Load();
        userdata ??= new();

        if (userdata is null)
        {
            userdata = new();
            handler.Save(userdata);
        }

        EditorCommands.Add("focus-left", Ex.LeftWindow);
        EditorCommands.Add("focus-right", Ex.RightWindow);
        EditorCommands.Add("focus-bottom", Ex.Bottom);
        EditorCommands.Add("focus-Top", Ex.Top);

        userdata.AddMacro(
            new()
            {
                keys = new List<Key>()
                {
                    new Key() { code = 72, control = true },
                },
                commands = new List<string>() { "focus-left" },
            }
        );

        userdata.AddMacro(
            new()
            {
                keys = new List<Key>()
                {
                    new Key() { code = 76, control = true },
                },
                commands = new List<string>() { "focus-right" },
            }
        );

        userdata.AddMacro(
            new()
            {
                keys = new List<Key>()
                {
                    new Key() { code = 74, control = true },
                },
                commands = new List<string>() { "focus-bottom" },
            }
        );

        userdata.AddMacro(
            new()
            {
                keys = new List<Key>()
                {
                    new Key() { code = 75, control = true },
                },
                commands = new List<string>() { "focus-top" },
            }
        );

        handler.Save(userdata);
    }

    static void Setup()
    {
        windows = Resources.FindObjectsOfTypeAll<EditorWindow>().ToList();
        focused = EditorWindow.focusedWindow;
        Assert.IsNotNull(focused, "focused is null on setup");
        tab = FocusWindow.GetDockedWindows(focused);

        //todo: make this better.
        foreach (var win in windows.DistinctBy(f => f.position))
        {
            tabPos.TryAdd(win.position, win);
        }
    }

    private static int currentIndex;

    [MenuItem("FocusTab/PreviousComponent")]
    public static void Down()
    {
        Setup();
        if (Hierarchy.IsHierarchy(focused))
        {
            Hierarchy.Down();
            return;
        }

        var e = Event.KeyboardEvent("down");
        EditorWindow.focusedWindow.SendEvent(e);

        // Type hierarchyType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor");

        // var isinspector = focused is null || hierarchyType != focused.GetType() ? false : true;

        // if (!isinspector)
        //     return;

        // return;
    }

    [MenuItem("FocusTab/ParentComponent")]
    static void Left()
    {
        Setup();

        if (Hierarchy.IsHierarchy(focused))
        {
            Hierarchy.Left();
        }

        var e = Event.KeyboardEvent("left");
        EditorWindow.focusedWindow.SendEvent(e);
    }

    [MenuItem("FocusTab/ExpandTree")]
    static void Right()
    {
        Setup();
        if (Hierarchy.IsHierarchy(focused))
        {
            Hierarchy.Right();

            return;
        }

        Type hierarchyType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor");

        var isinspector = focused is null || hierarchyType != focused.GetType() ? false : true;

        Debug.Log("sending the right event");

        // if (!isinspector)
        //     return;

        var e = Event.KeyboardEvent("right");
        EditorWindow.focusedWindow.SendEvent(e);
    }

    [MenuItem("FocusTab/NextComponent")]
    static void Up()
    {
        Setup();
        if (Hierarchy.IsHierarchy(focused))
        {
            Hierarchy.Up();
            return;
        }

        var e = Event.KeyboardEvent("up");
        EditorWindow.focusedWindow.SendEvent(e);
    }

    [MenuItem("FocusTab/Left")]
    private static void LeftWindow()
    {
        Setup();

        Debug.Log($"Settup complete, {focused is null}");

        var inDockIdx = tab.IndexOf(focused);

        if (tab.Count > 1 && inDockIdx != 0)
        {
            tab.ElementAt(inDockIdx - 1).Focus();
        }
        else
        {
            var wind = windows;

            wind = wind.Where(w =>
                {
                    return (FocusWindow.Left(focused, w) || FocusWindow.Same(focused, w))
                        && !FocusWindow.Equal(focused, w);
                })
                .ToList();

            wind = wind.OrderBy(w =>
                {
                    return FocusWindow.Distance(focused, w);
                })
                .ToList();
            wind = wind.Where(w => !tab.Contains(w)).ToList();

            var first = wind.FirstOrDefault();

            var docks = FocusWindow.GetDockedWindows(first);

            if (docks.Count() > 1)
            {
                var val = tabPos.GetValueOrDefault(first.position);
                val.Focus();
            }
            else
            {
                first.Focus();
            }
        }
    }

    [MenuItem("FocusTab/Right")]
    private static void RightWindow()
    {
        Setup();

        var dockedWindows = FocusWindow.GetDockedWindows(focused);

        var inDockIdx = tab.IndexOf(focused);

        if (tab.Count > 1 && inDockIdx != tab.Count() - 1)
        {
            tab.ElementAt(inDockIdx + 1).Focus();
        }
        else
        {
            var wind = windows
                .Where(w =>
                    (FocusWindow.Right(focused, w) || FocusWindow.Same(focused, w))
                    && !FocusWindow.Equal(focused, w)
                )
                .OrderBy(w => FocusWindow.Distance(focused, w))
                .Where(w => !tab.Contains(w))
                .FirstOrDefault();

            var docks = FocusWindow.GetDockedWindows(wind);

            var val = tabPos.GetValueOrDefault(wind.position);
            if (docks.Count() > 1 && val != null)
            {
                val.Focus();
            }
            else
            {
                wind.Focus();
            }
        }
    }

    [MenuItem("FocusTab/Top")]
    private static void Top()
    {
        Setup();

        var wind = windows
            .Where(w => FocusWindow.Top(focused, w) && !FocusWindow.Equal(focused, w))
            .OrderBy(w => FocusWindow.Distance(focused, w))
            .Where(w => !tab.Contains(w))
            .FirstOrDefault();

        var docks = FocusWindow.GetDockedWindows(wind);

        if (docks.Count() > 1)
        {
            var val = tabPos.GetValueOrDefault(wind.position);
            val.Focus();
        }
        else
        {
            wind.Focus();
        }
    }

    [MenuItem("FocusTab/Bottom")]
    private static void Bottom()
    {
        Setup();

        var wind = windows
            .Where(w => FocusWindow.Bottom(focused, w) && !FocusWindow.Equal(focused, w))
            .OrderBy(w => FocusWindow.Distance(focused, w))
            .Where(w => !tab.Contains(w))
            .FirstOrDefault();

        var val = wind;

        if (FocusWindow.GetDockedWindows(wind).Count() > 1)
        {
            val = tabPos.GetValueOrDefault(wind.position);
        }

        val.Focus();
    }

    static void TrackActiveTab()
    {
        var currentActiveWindow = EditorWindow.focusedWindow;

        if (currentActiveWindow == null)
            return;

        tabPos.TryGetValue(currentActiveWindow.position, out var val);

        if (val is null)
        {
            tabPos.Remove(currentActiveWindow.position);
            tabPos.Add(currentActiveWindow.position, currentActiveWindow);
        }
        else if (currentActiveWindow != val)
        {
            tabPos.Remove(currentActiveWindow.position);
            tabPos.Add(currentActiveWindow.position, currentActiveWindow);
        }
    }
}

public class FocusComponents
{
    public static Transform NextComponent(Transform component)
    {
        Debug.Log($"componens {component.GetInstanceID()}");

        var parent = component.transform.parent;

        var ind = component.GetSiblingIndex();

        if (parent is null)
        {
            var root = GetRootObjects();
            ind = Array.IndexOf(root, component);

            try
            {
                return root[ind - 1];
            }
            catch (Exception)
            {
                return null;
            }
        }

        try
        {
            return parent.GetChild(ind - 1);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Transform PreviousComponent(Transform component)
    {
        var parent = component.transform.parent;

        var ind = component.GetSiblingIndex();

        if (parent is not null)
        {
            try
            {
                return parent.GetChild(ind + 1);
            }
            catch (Exception)
            {
                return null;
            }
        }

        var root = GetRootObjects();
        ind = Array.IndexOf(root, component);

        try
        {
            return root[ind + 1];
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Transform[] GetRootObjects()
    {
        return UnityEngine
            .SceneManagement.SceneManager.GetActiveScene()
            .GetRootGameObjects()
            .Select(go => go.transform)
            .ToArray();
    }
}

public class FocusWindow
{
    public static float Distance(EditorWindow a, EditorWindow b) =>
        Vector2.Distance(
            new Vector2(a.position.x, a.position.y),
            new Vector2(b.position.x, b.position.y)
        );

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
            _getExpandedIDs.Invoke(_lastInteractedHierarchyWindow.GetValue(null), null) as int[];

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
}
