using System;
using System.Collections.Generic;

public class EditorCommands
{
    private static Dictionary<string, Action> commands = new();

    public static Action GetCommand(string commandId)
    {
        return commands.GetValueOrDefault(commandId);
    }

    public static void Add(string commandId, Action command)
    {
        commands.TryAdd(commandId, command);
    }
}
