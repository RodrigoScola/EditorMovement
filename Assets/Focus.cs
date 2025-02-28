using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Focus.Persistance;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Focus
{
    [InitializeOnLoad]
    public class FocusEditor
    {
        public static FocusConfig config;
        private static FileDataHandler<FileConfig> fileConfig;

        public EditorCommands commands = new();

        static EditorWindow[] windows;
        static EditorWindow focused;

        static List<EditorWindow> tab = new();

        static Dictionary<Rect, EditorWindow> tabPos = new();

        static FocusEditor()
        {
            EditorApplication.update += TrackActiveTab;
            EditorApplication.update += InitWindow;
            EditorApplication.hierarchyChanged += UpdateWindows;

            if (fileConfig is null)
            {
                fileConfig = new(Directory.GetCurrentDirectory(), "userData.json");
                // handler = new(Application.persistentDataPath, "userData.json");

                var filedata = fileConfig.Load()?.ToUserData();
                config ??= filedata;

                if (config is null)
                {
                    config = new();
                    fileConfig.Save(config.ToFile());
                }
            }

            EditorCommands.Add("editor.window.focus.left", FocusEditor.LeftWindow);
            EditorCommands.Add("editor.window.focus.right", FocusEditor.RightWindow);
            EditorCommands.Add("editor.window.focus.bottom", FocusEditor.Bottom);
            EditorCommands.Add("editor.window.focus.top", FocusEditor.Top);

            fileConfig.Save(config.ToFile());
        }

        static Dictionary<EditorWindow, List<EditorWindow>> dockedWindows;

        static void InitWindow()
        {
            UpdateWindows();
            EditorApplication.update -= InitWindow;
        }

        static void UpdateWindows()
        {
            windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            dockedWindows ??= new();

            dockedWindows.Clear();

            foreach (var window in windows)
            {
                try
                {
                    var w = FocusWindow.GetDockedWindows(window);
                    dockedWindows.Add(window, w);

                    UnityEngine.Debug.Log($"added {w.Count} to {window.titleContent.text}");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                    UnityEngine.Debug.LogError(window.titleContent.text);
                }
            }

            //todo: make this better.
            foreach (var win in windows.DistinctBy(f => f.position))
            {
                tabPos.TryAdd(win.position, win);
            }
        }

        static void Setup()
        {
            focused = EditorWindow.focusedWindow;

            // tab = FocusWindow.GetDockedWindows(focused);
            tab = dockedWindows.GetValueOrDefault(focused);

            Assert.IsTrue(dockedWindows.ContainsKey(focused), "the window is contained");

            Assert.IsNotNull(focused, "focused is null on setup");
        }

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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Setup();

            var el = tab.ElementAtOrDefault(tab.IndexOf(focused) - 1);

            if (el != null)
            {
                el.Focus();
            }
            else
            {
                var wind = windows;

                var first = wind.Where(w =>
                        (FocusWindow.Left(focused, w) || FocusWindow.Same(focused, w))
                        && !FocusWindow.Equal(focused, w)
                    )
                    .Where(w => !tab.Contains(w))
                    .OrderBy(w => FocusWindow.Distance(focused, w))
                    .FirstOrDefault();

                dockedWindows.TryGetValue(first, out var docks);

                if (docks.Count == 1)
                {
                    first.Focus();
                    return;
                }

                var val = tabPos.GetValueOrDefault(first.position);
                val.Focus();
            }

            stopwatch.Stop();

            TimeSpan elapsed = stopwatch.Elapsed;
            UnityEngine.Debug.Log($"Elapsed Time: {elapsed.TotalMilliseconds} ms");
        }

        [MenuItem("FocusTab/Right")]
        private static void RightWindow()
        {
            Setup();

            var dockedWindows = FocusWindow.GetDockedWindows(focused);

            var inDockIdx = tab.IndexOf(focused);

            if (tab.Count > 1 && inDockIdx != tab.Count() - 1)
            {
                tab[inDockIdx + 1].Focus();
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
}
