using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RegDecoder
{
    public class Utility
    {
        private static Regex regex = new Regex("^[0-7]+\\r?$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        public static string[] ASCIILabels = new string[33]
        {
      "NUL",
      "SOH",
      "STX",
      "ETX",
      "EOT",
      "ENQ",
      "ACK",
      "BEL",
      "BS",
      "HT",
      "LF",
      "VT",
      "FF",
      "CR",
      "SO",
      "SI",
      "DLE",
      "DC1",
      "DC2",
      "DC3",
      "DC4",
      "NAK",
      "SYN",
      "ETB",
      "CAN",
      "EM",
      "SUB",
      "ESC",
      "FS",
      "GS",
      "RS",
      "US",
      "SP"
        };

        public static void ExpressoError(string msg)
        {
            int num = (int)MessageBox.Show(msg, "Expresso Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        public static void ParseError(string message, CharBuffer buffer) => Utility.ExpressoError("Cannot parse the regular expression\n\n" + message + "\n\n" + buffer.Snapshot());

        public static int ParseOctal(string octal)
        {
            if (!Utility.regex.Match(octal).Success)
                return -1;
            int length = octal.Length;
            if (length > 10)
                return -1;
            int num1 = 1;
            int num2 = 0;
            for (int index = 0; index < length; ++index)
            {
                num2 += int.Parse(octal.Substring(length - 1 - index, 1)) * num1;
                num1 *= 8;
            }
            return num2;
        }

        public static string NodesToText(TreeNodeCollection nodes, int level)
        {
            StringBuilder stringBuilder1 = new StringBuilder("");
            string str1 = new string(' ', level * 4);
            string str2 = "";
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null && node.Tag.GetType().Name == "Character")
                {
                    str2 += node.Text;
                }
                else
                {
                    if (str2.Length > 0)
                    {
                        stringBuilder1.Append(str1 + str2 + "\r\n");
                        str2 = "";
                    }
                    StringBuilder stringBuilder2 = new StringBuilder(node.Text);
                    stringBuilder2.Replace("\n", "");
                    stringBuilder2.Replace("\r", "");
                    stringBuilder1.Append(str1 + (object)stringBuilder2 + "\r\n");
                    if (node.Nodes.Count > 0)
                        stringBuilder1.Append(Utility.NodesToText(node.Nodes, level + 1));
                }
            }
            if (str2.Length > 0)
                stringBuilder1.Append(str1 + str2 + "\r\n");
            return stringBuilder1.ToString();
        }

        public static int ParseHex(string hex)
        {
            try
            {
                return int.Parse(hex, NumberStyles.HexNumber);
            }
            catch
            {
                return -1;
            }
        }

        public static string DisplayAllASCIICharacters(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < text.Length; ++index)
            {
                int utf32 = char.ConvertToUtf32(text, index);
                if (utf32 < 32)
                    stringBuilder.Append("[" + Utility.ASCIILabels[utf32] + "]");
                else if (utf32 == (int)sbyte.MaxValue)
                    stringBuilder.Append("[DEL]");
                else
                    stringBuilder.Append(text[index]);
            }
            return stringBuilder.ToString();
        }

        public static bool IsNonPrinting(string text)
        {
            for (int index = 0; index < text.Length; ++index)
            {
                switch (char.ConvertToUtf32(text, index))
                {
                    case 9:
                    case 10:
                    case 13:
                    case 28:
                    case 29:
                    case 30:
                    case 31:
                        continue;
                    default:
                        return false;
                }
            }
            return true;
        }

        public static bool IsNonPrinting(char c)
        {
            switch (char.ConvertToUtf32(c.ToString(), 0))
            {
                case 9:
                case 10:
                case 13:
                case 28:
                case 29:
                case 30:
                case 31:
                    return true;
                default:
                    return false;
            }
        }
    }
}
