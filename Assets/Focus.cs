using System;
using System.Collections.Generic;
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

        static List<EditorWindow> windows = new();
        static EditorWindow focused;

        static List<EditorWindow> tab = new();

        static Dictionary<Rect, EditorWindow> tabPos = new();

        static FocusEditor()
        {
            EditorApplication.update += TrackActiveTab;

            fileConfig = new(Directory.GetCurrentDirectory(), "userData.json");
            // handler = new(Application.persistentDataPath, "userData.json");

            var filedata = fileConfig.Load()?.ToUserData();
            config ??= filedata;

            if (config is null)
            {
                config = new();
                fileConfig.Save(config.ToFile());
            }

            EditorCommands.Add("focus-left", FocusEditor.LeftWindow);
            EditorCommands.Add("focus-right", FocusEditor.RightWindow);
            EditorCommands.Add("focus-bottom", FocusEditor.Bottom);
            EditorCommands.Add("focus-Top", FocusEditor.Top);

            config.AddCommand(
                new()
                {
                    keys = new List<Key>()
                    {
                        new Key() { code = Keys.H, control = true },
                    },
                    commands = new List<string>() { "focus-left" },
                }
            );

            config.AddCommand(
                new()
                {
                    keys = new List<Key>()
                    {
                        new Key() { code = Keys.L, control = true },
                    },
                    commands = new List<string>() { "focus-right" },
                }
            );

            config.AddCommand(
                new()
                {
                    keys = new List<Key>()
                    {
                        new Key() { code = Keys.J, control = true },
                    },
                    commands = new List<string>() { "focus-bottom" },
                }
            );

            config.AddCommand(
                new()
                {
                    keys = new List<Key>()
                    {
                        new Key() { code = Keys.K, control = true },
                    },
                    commands = new List<string>() { "focus-top" },
                }
            );

            fileConfig.Save(config.ToFile());
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
            Setup();

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
}
