using System;
namespace RegDecoder
{
    public class Comment : Element
    {
        private string contents;

        public Comment(CharBuffer buffer)
        {
            this.Image = ImageType.Comment;
            this.Start = buffer.IndexInOriginalBuffer;
            while (!buffer.End)
            {
                buffer.MoveNext();
                if (buffer.Current == '\n')
                    break;
            }
            this.End = buffer.IndexInOriginalBuffer;
            this.Literal = buffer.Substring(this.Start - buffer.Offset, this.End - this.Start);
            this.contents = this.Literal.Remove(0, 1).Trim();
            this.Description = "Comment: " + this.contents;
            buffer.MoveNext();
        }

        public string Contents => this.contents;
    }
}
