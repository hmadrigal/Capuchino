using System;
namespace RegDecoder
{
    public class StringElement : Element
    {
        public StringElement(string text)
        {
            this.Image = ImageType.Conditional;
            this.Description = text;
            this.Literal = text;
        }
    }
}
