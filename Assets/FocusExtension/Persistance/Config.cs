using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Focus.Persistance
{
    [System.Serializable]
    public class FocusConfig
    {
        [SerializeField]
        public List<Macro> macros = new();

        public List<Macro> Macros()
        {
            return macros;
        }

        public FileConfig ToFile()
        {
            return new FileConfig() { macros = macros.Select(m => m.ToFile()).ToList() };
        }

        public FocusConfig AddCommand(IEnumerable<Macro> macros)
        {
            foreach (var m in macros)
            {
                AddCommand(m);
            }

            return this;
        }

        public FocusConfig AddCommand(Macro macro)
        {
            if (!macros.Contains(macro))
            {
                macros.Add(macro);
            }
            return this;
        }
    }
}
