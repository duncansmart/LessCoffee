using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotSmart
{
    static class StringUtil
    {
        public static string JsEncode(this string text)
        {
            if (text == null)
                return null;
            if (text.Length == 0)
                return text;

            StringBuilder safe = new StringBuilder();
            foreach (char ch in text)
            {
                // Newline etc?
                if (ch == '\r')
                    safe.Append("\\r");
                else if (ch == '\n')
                    safe.Append("\\n");
                else if (ch == '\t')
                    safe.Append("\\t");
                else if (ch == '\\')
                    safe.Append("\\\\");
                // is it a common, safe char?
                else if ((ch >= '0' && ch <= '9') ||
                    (ch >= 'A' && ch <= 'Z') ||
                    (ch >= 'a' && ch <= 'z') ||
                    ch == ' ' || ch == ',' || ch == '.' || ch == ':' ||
                    ch == ';' || ch == '!' || ch == '?' || ch == '(' ||
                    ch == ')' || ch == '[' || ch == ']' || ch == '/' ||
                    ch == ')' || ch == '[' || ch == ']' || ch == '_' ||
                    ch == '='
                    )
                    safe.Append(ch);
                // Hex encode "\xFF"
                else if (ch <= 127)
                    safe.Append("\\x" + ((int)ch).ToString("x2"));
                // Unicode hex encode "\uFFFF"
                else
                    safe.Append("\\u" + ((int)ch).ToString("x4"));
            }
            return safe.ToString();
        }

        public static string Join(this IEnumerable<string> values, string delimiter = "")
        {
            StringBuilder sb = new StringBuilder();
            foreach (string value in values)
            {
                sb.Append(value);
                sb.Append(delimiter);
            }
            return sb.ToString();
        }

    }
}
