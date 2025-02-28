using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Focus
{
    [System.Serializable]
    public struct Key
    {
        public Keys code;
        public long lastChecked;
        public bool pressed;
        public bool control;
        public bool shift;
        public bool alt;

        private static readonly string CTRL = "Ctrl";
        private static readonly string SHIFT = "Shift";
        private static readonly string ALT = "Alt";
        private static readonly char DELIMITER = '+';

        public static Key ToKey(string str)
        {
            var k = new Key();

            var words = str.Trim().Split(DELIMITER);

            k.control = words.Contains(CTRL);
            k.shift = words.Contains(SHIFT);
            k.alt = words.Contains(ALT);

            k.code = GetKeyCode(words.Last());

            return k;
        }

        private static JsonSerializerSettings encodingSettings = new()
        {
            Converters = { new StringEnumConverter() },
        };

        private static Keys GetKeyCode(string key)
        {
            string json = $"\"{key}\""; // Wrap key in quotes to mimic JSON string format

            return JsonConvert.DeserializeObject<Keys>(json, encodingSettings);
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            if (control)
            {
                str.Append(CTRL).Append(DELIMITER);
            }
            if (shift)
            {
                str.Append(SHIFT).Append(DELIMITER);
            }
            if (alt)
            {
                str.Append(ALT).Append(DELIMITER);
            }

            str.Append(GetKeyName(code));

            return str.ToString();
        }

        public static string GetKeyName(Keys value)
        {
            // Get the enum field
            var field = value.GetType().GetField(value.ToString());

            // Get the EnumMember attribute, if it exists
            var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();

            return attribute?.Value ?? value.ToString(); // Return EnumMember value or default to name
        }

        public bool Same(Key other)
        {
            return this.code == other.code
                && this.control == other.control
                && this.shift == other.shift
                && this.alt == other.alt;
        }
    }
}
