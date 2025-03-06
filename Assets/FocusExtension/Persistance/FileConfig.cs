using System.Collections.Generic;
using System.Linq;
using Focus.Persistance;

namespace Focus
{
    public class FileConfig
    {
        public List<FileMacro> macros = new();

        public FocusConfig ToUserData()
        {
            return new FocusConfig
            {
                macros = macros.Select(macro => macro.ToUserMacro()).ToList(),
            };
        }
    }
}
