using System;
using System.Text.RegularExpressions;

namespace RegDecoder
{
    public class CharacterClass : Element
    {
        public static Regex ClassRegex = new Regex("^\\[(?<Negate>\\^?)(?<Contents>.*)]", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        private string Content;
        private bool Negate;

        public string Contents => this.Content;

        public CharacterClass(CharBuffer buffer)
        {
            this.Image = ImageType.CharacterClass;
            this.Start = buffer.IndexInOriginalBuffer;
            if (buffer.End)
            {
                Utility.ParseError("CharacterClass: Reached end of buffer looking for a character!", buffer);
                this.IsValid = false;
            }
            int currentIndex = buffer.CurrentIndex;
            CharBuffer.ParsedCharacterClass parsedCharacterClass = buffer.GetParsedCharacterClass();
            if (parsedCharacterClass.Count == 0)
            {
                this.Description = parsedCharacterClass.ErrorMessage;
                buffer.MoveTo(currentIndex + 1);
                this.End = buffer.IndexInOriginalBuffer;
                this.Literal = "[";
                this.IsValid = false;
            }
            else
            {
                int length = buffer.CurrentIndex - currentIndex;
                Match match = CharacterClass.ClassRegex.Match(buffer.Substring(currentIndex, length));
                if (match.Success)
                {
                    if (match.Groups[nameof(Negate)].Value == "^")
                    {
                        this.Negate = true;
                        this.MatchIfAbsent = true;
                    }
                    else
                    {
                        this.Negate = false;
                        this.MatchIfAbsent = false;
                    }
                    if (match.Groups[nameof(Contents)].Value.Length != 0)
                    {
                        this.Content = match.Groups[nameof(Contents)].Value;
                    }
                    else
                    {
                        this.Description = "Character class is empty";
                        this.IsValid = false;
                    }
                    this.Literal = match.Value;
                }
                else
                {
                    this.Description = "Invalid Character Class";
                    this.IsValid = false;
                    this.Literal = "[";
                }
                if (this.IsValid)
                {
                    if (this.Negate)
                        this.Description = "Any character that is NOT in this class: " + this.Literal.Remove(1, 1);
                    else
                        this.Description = "Any character in this class: " + this.Literal;
                }
                this.ParseRepetitions(buffer);
            }
        }
    }
}
