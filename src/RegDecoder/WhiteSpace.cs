using System;
using System.Text.RegularExpressions;

namespace RegDecoder
{
    public class WhiteSpace : Element
    {
        public static Regex FindWhiteSpace = new Regex("\\G\\s+", RegexOptions.Compiled);
        public static Regex FindECMAWhiteSpace = new Regex("\\G\\s+", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public WhiteSpace(int originalIndex, string text)
        {
            this.Image = ImageType.WhiteSpace;
            this.Start = originalIndex;
            this.End = originalIndex + text.Length;
            this.Literal = text;
            this.Description = "WHITESPACE - ignored";
        }
    }
}
