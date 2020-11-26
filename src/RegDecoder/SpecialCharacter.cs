using System;
using System.Text.RegularExpressions;

namespace RegDecoder
{
    public class SpecialCharacter : Element
    {
        private string character;
        public bool Escaped;
        public CharType CharacterType;
        private static Regex EscapableCharacters = new Regex("[-=!@#$%^&*()_+~`{}|\\][:\"';<>?/.,\\\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        private static Regex RegNumeric = new Regex("^[0-7]{3} | ^x[0-9A-Fa-f]{2} | ^u[0-9A-Fa-f]{4} | ^c[A-Z\\[\\\\\\]\\^_]", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public string TheCharacter => this.character;

        private string S
        {
            set
            {
                this.character = value;
                string str1 = "";
                string str2;
                switch (value)
                {
                    case "\t":
                        str2 = str1 + "Tab";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "\n":
                        str2 = str1 + "New line";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "\r":
                        str2 = str1 + "Return";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case " ":
                        str2 = str1 + "Space";
                        this.CharacterType = CharType.Regular;
                        break;
                    case "$":
                        if (!this.Escaped)
                        {
                            str2 = str1 + "End of line or string";
                            this.CharacterType = CharType.ZeroWidth;
                            break;
                        }
                        str2 = str1 + "Literal " + this.character;
                        this.CharacterType = CharType.Escaped;
                        break;
                    case ".":
                        if (!this.Escaped)
                        {
                            str2 = str1 + "Any character";
                            this.CharacterType = CharType.CharClass;
                            break;
                        }
                        str2 = str1 + "Literal " + this.character;
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "A":
                        str2 = str1 + "Beginning of string";
                        this.CharacterType = CharType.ZeroWidth;
                        break;
                    case "B":
                        str2 = str1 + "Anything other than the first or last character in a word";
                        this.MatchIfAbsent = true;
                        this.CharacterType = CharType.ZeroWidth;
                        break;
                    case "D":
                        str2 = str1 + "Any character that is not a digit";
                        this.MatchIfAbsent = true;
                        this.CharacterType = CharType.CharClass;
                        break;
                    case "G":
                        str2 = str1 + "Beginning of current search";
                        this.CharacterType = CharType.ZeroWidth;
                        break;
                    case nameof(S):
                        str2 = str1 + "Anything other than whitespace";
                        this.MatchIfAbsent = true;
                        this.CharacterType = CharType.CharClass;
                        break;
                    case "W":
                        str2 = str1 + "Any character that is not alphanumeric";
                        this.MatchIfAbsent = true;
                        this.CharacterType = CharType.CharClass;
                        break;
                    case "Z":
                        str2 = str1 + "End of string or before new line at end of string";
                        this.CharacterType = CharType.ZeroWidth;
                        break;
                    case "^":
                        if (!this.Escaped)
                        {
                            str2 = str1 + "Beginning of line or string";
                            this.CharacterType = CharType.ZeroWidth;
                            break;
                        }
                        str2 = str1 + "Literal " + this.character;
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "a":
                        str2 = str1 + "Bell";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "b":
                        str2 = str1 + "First or last character in a word";
                        this.CharacterType = CharType.ZeroWidth;
                        break;
                    case "d":
                        str2 = str1 + "Any digit";
                        this.CharacterType = CharType.CharClass;
                        break;
                    case "e":
                        str2 = str1 + "Escape";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "f":
                        str2 = str1 + "Form feed";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "n":
                        str2 = str1 + "New line";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "r":
                        str2 = str1 + "Carriage return";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "s":
                        str2 = str1 + "Whitespace";
                        this.CharacterType = CharType.CharClass;
                        break;
                    case "t":
                        str2 = str1 + "Tab";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "v":
                        str2 = str1 + "Vertical tab";
                        this.CharacterType = CharType.Escaped;
                        break;
                    case "w":
                        str2 = str1 + "Alphanumeric";
                        this.CharacterType = CharType.CharClass;
                        break;
                    case "z":
                        str2 = str1 + "End of string";
                        this.CharacterType = CharType.ZeroWidth;
                        break;
                    default:
                        if (SpecialCharacter.EscapableCharacters.Match(value.ToString()).Success)
                        {
                            str2 = str1 + "Literal " + value.ToString();
                            this.CharacterType = CharType.Escaped;
                            break;
                        }
                        str2 = str1 + "Illegal escape character: " + value.ToString();
                        this.IsValid = false;
                        this.CharacterType = CharType.Invalid;
                        break;
                }
                this.Description = str2;
            }
            get => this.character;
        }

        public SpecialCharacter(Backreference back)
        {
            if (!back.IsOctal)
            {
                Utility.ExpressoError("Error trying to convert a Backreference item to a Special Character");
            }
            else
            {
                this.Description = "Octal " + back.Contents;
                this.CharacterType = CharType.Octal;
                this.character = back.Contents;
                this.Literal = "\\" + back.Contents;
                this.Escaped = false;
                this.End = back.End;
                this.AsFewAsPossible = back.AsFewAsPossible;
                this.m = back.m;
                this.n = back.n;
                this.MatchIfAbsent = back.MatchIfAbsent;
                this.RepeatType = back.RepeatType;
                this.Start = back.Start;
            }
        }

        public SpecialCharacter(CharBuffer buffer)
        {
            this.Image = ImageType.SpecialCharacter;
            this.Escaped = false;
            this.Start = buffer.IndexInOriginalBuffer;
            if (buffer.End)
            {
                this.CharacterType = CharType.Invalid;
                this.character = "\\";
                this.Literal = "\\";
                this.Description = "Illegal \\ at end of pattern";
                this.IsValid = false;
                --this.Start;
                this.End = this.Start + 1;
            }
            else if (buffer.Current == '[')
            {
                buffer.MoveNext();
                if (buffer.Current == '^')
                {
                    this.MatchIfAbsent = true;
                    buffer.Move(2);
                    this.Literal = "[^\\" + buffer.Current.ToString();
                }
                else
                {
                    buffer.MoveNext();
                    this.Literal = "[\\" + buffer.Current.ToString();
                }
                char current = buffer.Current;
                this.S = current.ToString();
                if (this.MatchIfAbsent)
                    this.Description = "Any character other than " + this.Description;
                buffer.MoveNext();
                string literal = this.Literal;
                current = buffer.Current;
                string str = current.ToString();
                this.Literal = literal + str;
                buffer.MoveNext();
            }
            else if (buffer.Current == '\\')
            {
                buffer.MoveNext();
                if (buffer.End)
                {
                    Utility.ParseError("Illegal \\ at end of pattern", buffer);
                }
                else
                {
                    Match match = SpecialCharacter.RegNumeric.Match(buffer.GetEnd());
                    if (match.Success)
                    {
                        string str = match.Value.Substring(0, 1);
                        if (!(str == "x"))
                        {
                            if (!(str == "u"))
                            {
                                if (str == "c")
                                {
                                    this.Description = "Control " + match.Value.Substring(1, 1);
                                    this.CharacterType = CharType.Control;
                                    this.character = match.Value.Substring(1);
                                }
                                else
                                {
                                    this.Description = "Octal " + match.Value;
                                    this.CharacterType = CharType.Octal;
                                    this.character = match.Value.Substring(2);
                                }
                            }
                            else
                            {
                                this.Description = "Unicode " + match.Value.Substring(1);
                                this.CharacterType = CharType.Unicode;
                                this.character = match.Value.Substring(1);
                            }
                        }
                        else
                        {
                            this.Description = "Hex " + match.Value.Substring(1);
                            this.CharacterType = CharType.Hex;
                            this.character = match.Value.Substring(1);
                        }
                        this.Literal = "\\" + match.Value;
                        buffer.Move(match.Length);
                    }
                    else
                    {
                        this.Escaped = true;
                        this.S = buffer.Current.ToString();
                        this.Literal = "\\" + this.S;
                        buffer.MoveNext();
                    }
                }
            }
            else
            {
                this.S = buffer.Current.ToString();
                this.Literal = this.S;
                buffer.MoveNext();
            }
            this.ParseRepetitions(buffer);
        }

        public static bool NextIsWhitespace(CharBuffer buffer)
        {
            bool flag = false;
            switch (buffer.Next)
            {
                case '\t':
                case '\n':
                case '\r':
                case ' ':
                    flag = true;
                    break;
            }
            return flag;
        }
    }
}
