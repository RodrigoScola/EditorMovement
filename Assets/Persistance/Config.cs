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

        public void AddCommand(Macro macro)
        {
            if (macros.Contains(macro))
            {
                return;
            }

            macros.Add(macro);
        }
    }
}
