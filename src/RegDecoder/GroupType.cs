﻿using System;
namespace RegDecoder
{
    public enum GroupType
    {
        Balancing,
        Named,
        Numbered,
        Noncapturing,
        SuffixPresent,
        PrefixPresent,
        SuffixAbsent,
        PrefixAbsent,
        Greedy,
        Comment,
        OptionsInside,
        OptionsOutside,
        Alternatives,
        Conditional,
        NoGroup,
        Invalid,
    }
}
