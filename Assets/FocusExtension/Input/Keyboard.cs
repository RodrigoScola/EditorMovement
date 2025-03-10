using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Focus
{
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

            tempPressing ??= new();

            var userMacros = FocusEditor.config.Macros();

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
                tempPressing.Add(key, new Key() { code = (Keys)key });
                // }
            }
        }

        public static bool CheckIfAnyInputFieldIsFocused()
        {
            return EditorGUIUtility.editingTextField;
        }

        private static bool hasControlModifier = false;
        private static bool hasShiftModifier = false;

        private static void CheckKeys()
        {
            //todo: need to change this to support keyboard commands that start with control
            //want to use arrows when typing in search and stuff
            if (
                !UnityEditorInternal.InternalEditorUtility.isApplicationActive
                || CheckIfAnyInputFieldIsFocused()
            )
            {
                return;
            }

            for (int key = 2; key < 256; key++)
            {
                var tempKey = new Key();
                var pressingKey = (GetAsyncKeyState(key) & 0x8000) != 0;

                // if (pressingKey)
                // {
                //     Debug.Log($"pressing {key}");
                // }
                if (pressingKey && Key.IsControl(key) && hasControlModifier == false)
                {
                    hasControlModifier = true;
                }
                if (pressingKey && (Keys)key == Keys.LShift && hasShiftModifier == false)
                {
                    hasShiftModifier = true;
                }

                tempPressing.TryGetValue(key, out tempKey);

                if (pressingKey && hasShiftModifier && !Key.IsModifier(key))
                {
                    hasShiftModifier = false;
                }

                if (pressingKey && hasControlModifier && !Key.IsModifier(key))
                {
                    hasControlModifier = false;
                }

                Assert.IsNotNull(tempKey, "there is no key on temp keys");

                var wasReleased = tempKey.pressed && !pressingKey;

                tempKey.pressed = pressingKey;

                tempKey.lastChecked = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                tempPressing.Remove(key);
                tempPressing.Add(key, tempKey);

                if (wasReleased && !Key.IsModifier(tempKey.code))
                {
                    tempKey.pressed = true;

                    tempKey.Shift(hasShiftModifier);
                    tempKey.Control(hasControlModifier);

                    hasControlModifier = false;
                    hasShiftModifier = false;
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

            if (currentKeys.Count > 0)
            {
                var keys = string.Join(" ", currentKeys.Select(k => k.ToString()));
            }

            var macros = GetCommand(currentKeys, GlobalKeyListener.macros);

            if (macros is not null)
            {
                foreach (var macro in macros)
                {
                    macro();
                }

                hasControlModifier = false;
                currentKeys.Clear();
            }
        }

        static List<Action> GetCommand(List<Key> keys, Dictionary<List<Key>, List<Action>> macros)
        {
            if (keys.Count == 0)
                return null;
            foreach (var macro in macros)
            {
                if (macro.Key.Count != keys.Count)
                {
                    continue;
                }

                var exact = true;

                for (int i = 0; i < keys.Count; i++)
                {
                    Key current = keys[i];
                    var macroKey = macro.Key[i];

                    if (!current.Same(macroKey))
                    {
                        exact = false;
                        break;
                    }
                }
                if (exact == true)
                {
                    return macro.Value;
                }
            }
            return null;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
