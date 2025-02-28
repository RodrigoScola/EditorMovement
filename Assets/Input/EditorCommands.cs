using System;
using System.Collections.Generic;

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
    }
}
