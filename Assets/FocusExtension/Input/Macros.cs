using System.Collections.Generic;
using System.Linq;
using Focus.Persistance;

namespace Focus
{
    [System.Serializable]
    public class Macro
    {
        public List<string> commands = new();
        public List<Key> keys = new();

        public FileMacro ToFile()
        {
            return new FileMacro()
            {
                commands = commands,
                keys = keys.Select(key => key.ToString()).ToList(),
            };
        }

        public static Macro New()
        {
            return new Macro();
        }

        public Macro Command(string commandName)
        {
            if (!this.commands.Contains(commandName))
            {
                this.commands.Add(commandName);
            }
            return this;
        }

        public Macro Commands(List<string> val)
        {
            this.commands = val;
            return this;
        }

        public Macro Key(Key key)
        {
            this.keys.Add(key);
            return this;
        }

        public Macro Keys(List<Key> keys)
        {
            this.keys = keys;
            return this;
        }
    }
}
