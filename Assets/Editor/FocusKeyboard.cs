using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public enum Keys
{
    A = 65,
    L = 76,
    Escape = 27,
}

[System.Serializable]
public struct Key
{
    public int code;
    public long lastChecked;
    public bool pressed;

    public bool control;
}

[InitializeOnLoad]
public static class GlobalKeyListener
{
    private static List<Key> activeKeys;

    private static long checkDelta = (long)30.4;
    private static long maxTime = (long)600.4;

    private static Dictionary<List<Key>, List<Action>> macros;

    private static Dictionary<int, Key> tempPressing;

    private static List<Key> currentKeys;

    public static GameObject gm;

    static GlobalKeyListener()
    {
        activeKeys ??= new();

        EditorApplication.update += CheckKeys;

        var a = new Key() { code = 65 };
        var d = new Key() { code = 68 };

        tempPressing ??= new();

        // macros = Ex.userdata.macros;

        var userMacros = Ex.userdata.Macros();

        macros ??= new();

        foreach (var userMacro in userMacros)
        {
            var commandsFunc = userMacro
                .commands.Select(str => EditorCommands.GetCommand(str))
                .Where(cmd => cmd != null)
                .ToList();

            macros.TryAdd(userMacro.keys, commandsFunc);
        }

        currentKeys ??= new();

        Setup();
    }

    static void Setup()
    {
        for (int key = 2; key < 256; key++)
        {
            // var pressingKey = (GetAsyncKeyState(key) & 0x8000) != 0;
            tempPressing.Add(key, new Key() { code = key });
            // }
        }
    }

    public static bool CheckIfAnyInputFieldIsFocused()
    {
        return EditorGUIUtility.editingTextField;
    }

    private static bool hasControlModifier = false;

    private static void CheckKeys()
    {
        if (
            !UnityEditorInternal.InternalEditorUtility.isApplicationActive
            || CheckIfAnyInputFieldIsFocused()
        )
        {
            return;
        }

        var tempKey = new Key();

        for (int key = 2; key < 256; key++)
        {
            var pressingKey = (GetAsyncKeyState(key) & 0x8000) != 0;

            if (pressingKey && key == 162 && hasControlModifier == false)
            {
                hasControlModifier = true;
            }

            tempPressing.TryGetValue(key, out tempKey);

            if (pressingKey && hasControlModifier && (key != 162 && key != 17))
            {
                tempKey.control = hasControlModifier;

                hasControlModifier = false;
            }

            Assert.IsNotNull(tempKey, "there is no key on temp keys");

            var wasReleased = tempKey.pressed && !pressingKey;

            tempKey.pressed = pressingKey;

            tempKey.lastChecked = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            tempPressing.Remove(key);
            tempPressing.Add(key, tempKey);

            if (wasReleased)
            {
                tempKey.pressed = true;
                currentKeys.Add(tempKey);
            }
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        currentKeys = currentKeys
            .Where(key =>
            {
                return now - key.lastChecked < maxTime;
            })
            .ToList();

        var macros = GetMacro(currentKeys, GlobalKeyListener.macros);

        if (macros is not null)
        {
            foreach (var macro in macros)
            {
                macro();
            }

            if (hasControlModifier == true)
            {
                hasControlModifier = false;
            }
            currentKeys.Clear();
        }
    }

    static List<Action> GetMacro(List<Key> keys, Dictionary<List<Key>, List<Action>> macros)
    {
        if (keys.Count == 0)
            return null;
        foreach (var macro in macros)
        {
            if (macro.Key.Count != keys.Count)
            {
                continue;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].code != macro.Key[i].code || keys[i].control != macro.Key[i].control)
                {
                    continue;
                }

                return macro.Value;
            }
        }
        return null;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
