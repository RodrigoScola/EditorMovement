using System.Collections.Generic;

namespace Focus.Persistance
{
    public class FileMacro
    {
        public List<string> commands = new();
        public List<string> keys = new();

        public Macro ToUserMacro()
        {
            var userMacro = new Macro();

            userMacro.commands = commands;

            foreach (var key in keys)
            {
                userMacro.keys.Add(Key.ToKey(key));
            }

            return userMacro;
        }

        // public static FileMacro ToFile(Macro macro)
        // {
        //     var m = new FileMacro();

        //     m.commands = macro.commands;

        //     foreach (var key in macro.keys)
        //     {
        //         m.keys.Add(key.ToString());
        //     }

        //     return m;
        // }
    }
}
