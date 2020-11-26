using System;
namespace RegDecoder
{
    public class Character : Element
    {
        private string character;

        public string TheCharacter => this.character;

        public Character(CharBuffer buffer, bool RepetitionsOnly)
        {
            this.Image = ImageType.Character;
            this.Start = buffer.IndexInOriginalBuffer;
            if (buffer.End)
            {
                Utility.ParseError("Reached end of buffer in Character constructor!", buffer);
                this.IsValid = false;
            }
            else if (RepetitionsOnly)
            {
                this.character = buffer.Current.ToString();
                this.ParseRepetitions(buffer);
                return;
            }
            this.character = buffer.Current.ToString();
            this.Literal = this.character;
            this.Description = this.Literal;
            buffer.MoveNext();
            this.ParseRepetitions(buffer);
        }

        public Character(CharBuffer buffer)
          : this(buffer, false)
        {
        }
    }
}
