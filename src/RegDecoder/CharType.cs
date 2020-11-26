using System;
namespace RegDecoder
{
    public enum CharType
    {
        Other,
        Octal,
        Unicode,
        Hex,
        Control,
        CharClass,
        Escaped,
        Regular,
        ZeroWidth,
        Invalid,
    }
}
