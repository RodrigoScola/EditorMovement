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
    }
}
