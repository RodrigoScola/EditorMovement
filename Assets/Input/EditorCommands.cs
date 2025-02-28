using System;
using System.Collections.Generic;
using Focus.Persistance;

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

        public static void InitBasicCommands(FocusConfig config)
        {
            config.AddCommand(
                Macro.New().Key(Key.New(Keys.H).Control(true)).Command("editor.window.focus.left")
            );

            config.AddCommand(
                Macro.New().Key(Key.New(Keys.L).Control(true)).Command("editor.window.focus.right")
            );

            config.AddCommand(
                Macro.New().Key(Key.New(Keys.J).Control(true)).Command("editor.window.focus.bottom")
            );

            config.AddCommand(
                Macro.New().Key(Key.New(Keys.K).Control(true)).Command("editor.window.focus.top")
            );
        }
    }
}
