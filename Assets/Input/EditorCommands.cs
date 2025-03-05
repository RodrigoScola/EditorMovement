using System;
using System.Collections.Generic;
using Focus.Persistance;
using UnityEditor;
using UnityEditor.Search;

namespace Focus
{
    public class EditorCommands
    {
        public static EditorCommands instance = new();

        private Dictionary<string, Action> commands = new();

        public static Action GetCommand(string commandId)
        {
            return instance.commands.GetValueOrDefault(commandId);
        }

        public static void Add(string commandId, Action command)
        {
            instance.commands.TryAdd(commandId, command);
        }

        public static void Init()
        {
            EditorCommands.Add("editor.window.focus.left", FocusEditor.LeftWindow);
            EditorCommands.Add("editor.window.focus.right", FocusEditor.RightWindow);
            EditorCommands.Add("editor.window.focus.bottom", FocusEditor.Bottom);
            EditorCommands.Add("editor.window.focus.top", FocusEditor.Top);

            //switching docs
            EditorCommands.Add("editor.window.switch.left", FocusEditor.FocusLeftDock);
            EditorCommands.Add("editor.window.switch.right", FocusEditor.FocusRightDock);
            //todo: make this work

            // EditorCommands.Add("editor.window.switch.top", FocusEditor.FocusRightDock);
            // EditorCommands.Add("editor.window.switch.bottom", FocusEditor.FocusRightDock);

            //general window commands

            EditorCommands.Add("editor.window.down", FocusEditor.Down);
            EditorCommands.Add("editor.window.up", FocusEditor.Up);
            EditorCommands.Add("editor.window.left", FocusEditor.Left);
            EditorCommands.Add("editor.window.right", FocusEditor.Right);
            EditorCommands.Add(
                "window.focus.inspector",
                () => FocusWindow.FocusWindowByName("Inspector")
            );
            EditorCommands.Add("keyboard.down", () => FocusEditor.Keyboard("down"));
            EditorCommands.Add("keyboard.up", () => FocusEditor.Keyboard("up"));

            EditorCommands.Add(
                "window.display.pop",
                () =>
                {
                    EditorWindow.focusedWindow.ShowModal();
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
        }
    }
}
