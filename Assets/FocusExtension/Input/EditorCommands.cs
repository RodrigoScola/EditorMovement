using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Focus
{
    public class EditorCommands
    {
        public static EditorCommands instance = new();
        private static bool hasInitialized;

        private Dictionary<string, Action> commands = new();

        public static Dictionary<string, Action> Commands()
        {
            Assert.IsTrue(hasInitialized, "cannot do any operations without initting");
            return instance.commands;
        }

        public static Action GetCommand(string commandId)
        {
            Assert.IsTrue(hasInitialized, "cannot do any operations without initting");
            return instance.commands.GetValueOrDefault(commandId);
        }

        public static void Add(string commandId, Action command)
        {
            Assert.IsTrue(hasInitialized, "cannot do any operations without initting");
            instance.commands.TryAdd(commandId, command);
        }

        public static void Init()
        {
            hasInitialized = true;

            Add("editor.window.focus.left", FocusEditor.LeftWindow);
            Add("editor.window.focus.right", FocusEditor.RightWindow);
            Add("editor.window.focus.bottom", FocusEditor.Bottom);
            Add("editor.window.focus.top", FocusEditor.Top);

            //switching docs
            EditorCommands.Add("editor.window.switch.left", FocusEditor.FocusLeftDock);
            EditorCommands.Add("editor.window.switch.right", FocusEditor.FocusRightDock);
            //todo: make this work

            // EditorCommands.Add("editor.window.switch.top", FocusEditor.FocusRightDock);
            // EditorCommands.Add("editor.window.switch.bottom", FocusEditor.FocusRightDock);

            //general window commands

            Add("editor.window.down", FocusEditor.Down);
            Add("editor.window.up", FocusEditor.Up);
            Add("editor.window.left", FocusEditor.Left);
            Add("editor.window.right", FocusEditor.Right);
            Add("window.focus.inspector", () => FocusWindow.FocusWindowByName("Inspector"));
            Add("keyboard.down", () => FocusEditor.Keyboard("down"));
            Add("keyboard.up", () => FocusEditor.Keyboard("up"));
            Add(
                "window.display.pop",
                () =>
                {
                    try
                    {
                        var wind = EditorWindow.focusedWindow;
                        var windowType = wind.GetType(); // Get the type of the current window
                        wind.Close(); // Close it
                        EditorWindow newWindow = EditorWindow.GetWindow(windowType); // Reopen it
                        newWindow.Show();
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }
            );

            EditorCommands.Add(
                "window.display.toggle-max",
                () =>
                {
                    var wind = EditorWindow.focusedWindow;
                    if (!wind)
                    {
                        return;
                    }

                    wind.maximized = !wind.maximized;
                    Thread.Sleep(100);
                    FocusEditor.Reload();
                }
            );

            EditorCommands.Add("game.play", EditorApplication.EnterPlaymode);
            EditorCommands.Add(
                "game.pause",
                () =>
                {
                    EditorApplication.isPaused = !EditorApplication.isPaused;

                    var menus = Unsupported.GetSubmenus("Window");

                    File.WriteAllText(
                        Directory.GetCurrentDirectory() + "file.txt",
                        string.Join('\n', menus)
                    );
                }
            );

            EditorCommands.Add(
                "editor.search.contextual",
                () =>
                {
                    SearchContext context = SearchService.CreateContext("t:GameObject");
                    SearchService.ShowWindow(context).Focus();
                }
            );

            Add(
                "window.commands.find",
                () =>
                {
                    var wind = EditorWindow.focusedWindow;

                    if (wind == null)
                    {
                        return;
                    }

                    var evt = EditorGUIUtility.CommandEvent("Find");

                    wind.SendEvent(evt);
                }
            );

            Add(
                "inspector.action.nextComponent",
                () =>
                {
                    var wind = EditorWindow.focusedWindow;
                    if (!wind)
                    {
                        return;
                    }

                    var gm = Selection.activeGameObject;

                    if (gm != null)
                    {
                        Debug.Log("focusing other stuff");
                    }

                    var co = gm.GetComponents<Component>().ToList();

                    // wind.Repaint();

                    // foreach (Component component in c)
                    // {
                    //     // Get the type of the component
                    //     System.Type type = component.GetType();
                    //     UnityEngine.Debug.Log($"Component: {type.Name}");

                    //     // Retrieve all fields (public and private) of the component
                    //     FieldInfo[] fields = type.GetFields(
                    //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    //     );

                    //     // Iterate over each field
                    //     foreach (FieldInfo field in fields)
                    //     {
                    //         // Get the field's name and value
                    //         string fieldName = field.Name;
                    //         object fieldValue = field.GetValue(component);
                    //         UnityEngine.Debug.Log($"Field: {fieldName}, Value: {fieldValue}");
                    //     }
                    // }
                }
            );

            InitUnityCommands();
        }

        public static void InitUnityCommands()
        {
            var commands = Unsupported.GetSubmenus("Window");

            foreach (var command in commands)
            {
                EditorCommands.Add(command.Replace("/", "."), FocusEditor.Exec(command));
            }
        }
    }
}
