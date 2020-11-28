using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expresso.Ultrapico.Expresso
{
    public class RegLibrary
    {
        public void SetDefaults(ICollection<RegexLibraryItem> list)
        {
            RegexOptions options1 = RegexOptions.Multiline | RegexOptions.Compiled;
            RegexOptions options2 = RegexOptions.Compiled;
            list.Add(new RegexLibraryItem("Dates", "(?<Month>\\d{1,2})/(?<Day>\\d{1,2})/(?<Year>(?:\\d{4}|\\d{2}))"));
            list.Add(new RegexLibraryItem("Zip Codes", "(?<Zip>\\d{5})-(?<Sub>\\d{4})"));
            list.Add(new RegexLibraryItem("Phone Numbers", "\\((?<AreaCode>\\d{3})\\)\\s*(?<Number>\\d{3}(?:-|\\s*)\\d{4})"));
            list.Add(new RegexLibraryItem("All Words", "\\w+"));
            list.Add(new RegexLibraryItem("Five and Six Letter Words", "(?<=(?:\\s|\\G|\\A))\\w{5,6}(?=(?:\\s|\\Z|\\.|\\?|\\!))"));
            list.Add(new RegexLibraryItem("Sentences", "(?sx-m)[^\\r\\n].*?(?:(?:\\.|\\?|!)\\s)"));
            list.Add(new RegexLibraryItem("IP Addresses", "(?<First>2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.(?<Second>2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.(?<Third>2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.(?<Fourth>2[0-4]\\d|25[0-5]|[01]?\\d\\d?)"));
            list.Add(new RegexLibraryItem("URLs", "(?<Protocol>\\w+):\\/\\/(?<Domain>[\\w@][\\w.:@]+)\\/?[\\w\\.?=%&=\\-@/$,]*"));
            list.Add(new RegexLibraryItem("Show multiple captures within a group, when used with sample project", "(a(b)c)+(?x)   # Shows multiple captures in each group, also show use of comment"));
            list.Add(new RegexLibraryItem("Email Address", "([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})"));
            list.Add(new RegexLibraryItem("Doubled Words", "\\b(\\w+)\\s+\\1\\b"));
            list.Add(new RegexLibraryItem("Decimal Number", "^[+-]?(?:\\d+\\.?\\d*|\\d*\\.?\\d+)[\\r\\n]*$", options1));
            list.Add(new RegexLibraryItem("Hexadecimal Number", "^[0-9a-fA-F]+[\\r\\n]*$", options1));
            list.Add(new RegexLibraryItem("Scientific Notation", "^[+-]?(?<Mantissa>\\d+\\.?\\d*|\\d*\\.?\\d+)(?<Exponent>E[+\\-\\x20]?\\d+)?[\\r\\n]*$", options1));
            list.Add(new RegexLibraryItem("Comma Separated Values", "^(?:(?<Item>[^,\\n]+),)+(?<LastItem>[^,\\n]+)[\\r\\n]*$", options1));
            list.Add(new RegexLibraryItem("Key=Value Pairs", "(?<Keyword>\\w+)\\s*=\\s*(?<Value>.*)((?=\\W$)|\\z)", options1));
            list.Add(new RegexLibraryItem("Find Hyperlinks", "href=\"(?<Link>.*?)\""));
            list.Add(new RegexLibraryItem("Opening or Closing Punctuation", "[\\p{Pe}\\p{Ps}]"));
            list.Add(new RegexLibraryItem("Repetitions of More Than One Vowel", "[aeiou]{2,}"));
            list.Add(new RegexLibraryItem("Repetitions of More Than Three Consonants", "[bcdfghj-np-tv-z]{4,}"));
            list.Add(new RegexLibraryItem("Each Line of Text", "^.*$", options1));
            list.Add(new RegexLibraryItem("First Line of Text", "\\A.*"));
            list.Add(new RegexLibraryItem("Last Line of Text", ".+\\Z$"));
            list.Add(new RegexLibraryItem("String with the same beginning and end", "(?<First>\\w{2,})(?<Second>\\w+)(?(Second)\\k<First>)"));
            list.Add(new RegexLibraryItem("Find the end of words that begin with \"re\"", "(?<=\\bre)\\w+\\b"));
            list.Add(new RegexLibraryItem("Find the beginning of words that end with \"ing\"", "\\b\\w+(?=ing\\b)"));
            list.Add(new RegexLibraryItem("Text between HTML Tags", "<(?<tag>\\w*)>(?<text>.*)</\\k<tag>>"));
            list.Add(new RegexLibraryItem("Social Security Number", "\\d{3}-\\d{2}-\\d{4}"));
            list.Add(new RegexLibraryItem("Search for Matching Parentheses", "\\(\r\n    (?>\r\n        [^()]+ \r\n        |    \\( (?<number>)\r\n        |    \\) (?<-number>)\r\n    )*\r\n    (?(number)(?!))\r\n\\)\r\n"));
            list.Add(new RegexLibraryItem("Canadian Postal Code", "\\b[A-Z-[DFIOQUWZ]]\\d[A-Z-[DFIOQU]]\\ +\\d[A-Z-[DFIOQU]]\\d\\b", options2));
            list.Add(new RegexLibraryItem("UK Postal Code", "\\b([A-Z]{1,2}\\d[A-Z]|[A-Z]{1,2}\\d{1,2})\\ +\\d[A-Z-[CIKMOV]]{2}\\b", options2));
            list.Add(new RegexLibraryItem("Netherlands Postal Code", "\\b[1-9]\\d{3}\\ +[A-Z]{2}\\b", options2));
            list.Add(new RegexLibraryItem("Credit Card Validator", "^\r\n(?:(?<Mastercard>5[1-5]\\d{14})|\r\n(?<Visa>4(?:\\d{15}|\\d{12}))|\r\n(?<Amex>3[47]\\d{13})|\r\n(?<DinersClub>3(?:0[0-5]|6[0-9]|8[0-9])\\d{11})|\r\n(?<Discover>6011\\d{12}))\r\n\\s*$", options1 | RegexOptions.IgnorePatternWhitespace));
        }
    }
}
