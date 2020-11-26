using System;
using System.Text.RegularExpressions;

namespace RegDecoder
{
    public class NamedClass : Element
    {
        private static Regex NamedClassRegex = new Regex("^\\\\(?<Type>[pP])\\{(?<Name>.*?)\\}", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        private string ClassName;
        private string FriendlyName;

        public string Contents => this.ClassName;

        public NamedClass() => this.Image = ImageType.CharacterClass;

        public bool Parse(CharBuffer buffer)
        {
            this.Start = buffer.IndexInOriginalBuffer;
            if (buffer.End || buffer.Next != 'p' && buffer.Next != 'P')
                return false;
            Match match = NamedClass.NamedClassRegex.Match(buffer.GetEnd());
            if (match.Success && match.Groups["Type"].Length > 0)
            {
                if (match.Groups["Type"].Value == "P")
                    this.MatchIfAbsent = true;
                else
                    this.MatchIfAbsent = false;
                this.ClassName = match.Groups["Name"].Value;
                int length = UnicodeCategories.UnicodeAbbrev.Length;
                this.FriendlyName = "";
                for (int index = 0; index < length; ++index)
                {
                    if (UnicodeCategories.UnicodeAbbrev[index] == this.ClassName)
                    {
                        this.FriendlyName = UnicodeCategories.UnicodeName[index];
                        break;
                    }
                }
                string str;
                if (this.ClassName == "")
                {
                    str = "Empty Unicode character class";
                    this.IsValid = false;
                }
                else if (this.FriendlyName == "")
                {
                    str = "Possibly unrecognized Unicode character class: [" + this.ClassName + "]";
                    this.IsValid = false;
                }
                else
                    str = "a Unicode character class: \"" + this.FriendlyName + "\"";
                this.Literal = match.Value;
                if (!this.IsValid)
                    this.Description = str;
                else if (this.MatchIfAbsent)
                    this.Description = "Any character NOT from " + str;
                else
                    this.Description = "Any character from " + str;
                buffer.Move(match.Length);
                this.ParseRepetitions(buffer);
                return true;
            }
            this.Description = "Syntax error in Unicode character class";
            this.Literal = "\\p";
            buffer.Move(2);
            this.End = buffer.IndexInOriginalBuffer;
            this.IsValid = false;
            return true;
        }
    }
}
