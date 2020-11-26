using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RegDecoder
{
    public class CharBuffer
    {
        private string Data;
        private int Length;
        private int Index;
        public int Offset;
        public bool IgnoreWhiteSpace = true;
        public bool IsECMA;

        public CharBuffer(string data)
        {
            this.Data = data;
            this.Length = this.Data.Length;
            this.Index = 0;
        }

        public char Current => this.Index == this.Length ? char.MinValue : this.Data.ToCharArray(this.Index, 1)[0];

        public char Next => this.Index >= this.Length - 1 ? char.MinValue : this.Data.ToCharArray(this.Index + 1, 1)[0];

        public char Previous => this.Index < 1 ? char.MinValue : this.Data.ToCharArray(this.Index - 1, 1)[0];

        public int CurrentIndex => this.Index;

        public int IndexInOriginalBuffer => this.Index + this.Offset;

        public string Snapshot()
        {
            if (this.End)
                return "";
            int num1 = 10;
            string str1 = "";
            string str2 = "";
            int startIndex = this.Index - num1;
            if (startIndex < 0)
            {
                startIndex = 0;
                int index = this.Index;
            }
            string str3 = this.Data.Substring(startIndex, this.Index - startIndex);
            int num2 = this.Index + num1;
            if (num2 > this.Length - 1)
                num2 = this.Length - 1;
            string str4 = str1 + this.Data.Substring(this.Index, 1) + str2;
            string str5 = this.Index != this.Length - 1 ? this.Data.Substring(this.Index + 1, num2 - this.Index - 1) : "";
            string str6 = str4;
            string str7 = str5;
            return str3 + str6 + str7;
        }

        public bool MoveNext() => this.Index != this.Length && this.Index++ != this.Length;

        public bool Move(int N)
        {
            if (N >= 0)
            {
                this.Index += N;
                if (this.Index < this.Length)
                    return true;
                this.Index = this.Length;
                return false;
            }
            this.Index += N;
            if (this.Index > 0)
                return true;
            this.Index = 0;
            return false;
        }

        public bool MoveTo(int index) => this.Move(index - this.Index);

        public bool MovePrevious()
        {
            if (this.Index == 0)
                return false;
            --this.Index;
            return true;
        }

        public void MoveEnd() => this.Index = this.Length;

        public void MoveBegin() => this.Index = 0;

        public void SkipWhiteSpace()
        {
            if (!this.IgnoreWhiteSpace)
                return;
            this.GetWhiteSpace();
        }

        public string GetWhiteSpace()
        {
            Match match = !this.IsECMA ? WhiteSpace.FindWhiteSpace.Match(this.Data, this.Index) : WhiteSpace.FindECMAWhiteSpace.Match(this.Data, this.Index);
            if (!match.Success)
                return "";
            this.Move(match.Value.Length);
            return match.Value;
        }

        public string GetEnd() => this.End ? "" : this.Data.Substring(this.Index);

        public string Substring(int index, int length) => this.Data.Substring(index, length);

        public bool End => this.Index == this.Length;

        public bool Begin => this.Index == 0;

        public string GetStringToMatchingParenthesis()
        {
            if (this.Current != '(')
                return "";
            int currentIndex1 = this.CurrentIndex;
            int num = 1;
            bool flag = false;
            while (!this.End && num != 0)
            {
                this.MoveNext();
                if (!flag && this.Current == '[')
                {
                    int currentIndex2 = this.CurrentIndex;
                    if (this.GetParsedCharacterClass().Count == 0)
                        this.MoveTo(currentIndex2 + 1);
                }
                if (!flag && this.Current == '(')
                    ++num;
                else if (!flag && this.Current == ')')
                    --num;
                flag = this.Current == '\\' && !flag;
            }
            if (this.End)
            {
                this.MovePrevious();
                int currentIndex2 = this.CurrentIndex;
                return this.Substring(currentIndex1, currentIndex2 - currentIndex1 + 1);
            }
            int currentIndex3 = this.CurrentIndex;
            return this.Substring(currentIndex1, currentIndex3 - currentIndex1 + 1);
        }

        public CharBuffer.ParsedCharacterClass GetParsedCharacterClass()
        {
            CharBuffer.ParsedCharacterClass parsedCharacterClass = new CharBuffer.ParsedCharacterClass();
            if (this.Current != '[')
            {
                parsedCharacterClass.ErrorMessage = "Parsing Error - Character Class did not start with [";
                return parsedCharacterClass;
            }
            parsedCharacterClass.LeftBracketIndex[0] = this.CurrentIndex;
            parsedCharacterClass.Count = 1;
            int currentIndex = this.CurrentIndex;
            bool flag1 = false;
            bool flag2 = false;
            while (!this.End)
            {
                this.MoveNext();
                if (flag2 && this.Current == '[')
                {
                    currentIndex = this.CurrentIndex;
                    parsedCharacterClass.LeftBracketIndex[parsedCharacterClass.Count] = currentIndex;
                    ++parsedCharacterClass.Count;
                    if (parsedCharacterClass.Count > 30)
                    {
                        parsedCharacterClass.ErrorMessage = "The Analyzer cannot handle character class subtraction with depth >30";
                        parsedCharacterClass.Count = 0;
                        return parsedCharacterClass;
                    }
                }
                flag2 = !flag1 && this.Current == '-';
                if (!flag1 && this.Current == ']')
                {
                    if (this.CurrentIndex != currentIndex + 1 && (this.CurrentIndex != currentIndex + 2 || this.Previous != '^'))
                        break;
                }
                else
                    flag1 = this.Current == '\\' && !flag1;
            }
            if (this.End)
            {
                parsedCharacterClass.Count = 0;
                parsedCharacterClass.ErrorMessage = "Unmatched bracket in character class";
                return parsedCharacterClass;
            }
            for (int index = 0; index < parsedCharacterClass.Count - 1; ++index)
            {
                this.MoveNext();
                if (this.End)
                {
                    parsedCharacterClass.ErrorMessage = "Unmatched bracket in character class";
                    parsedCharacterClass.Count = 0;
                    return parsedCharacterClass;
                }
                if (this.Current != ']')
                {
                    parsedCharacterClass.ErrorMessage = "Syntax error in character class subtraction";
                    parsedCharacterClass.Count = 0;
                    return parsedCharacterClass;
                }
            }
            this.MoveNext();
            return parsedCharacterClass;
        }

        public List<int> FindNakedPipes()
        {
            int num = 0;
            bool flag = false;
            List<int> intList = new List<int>();
            while (!this.End)
            {
                if (!flag && this.Current == '(')
                    ++num;
                else if (!flag && this.Current == ')')
                    --num;
                if (num == 0 && !flag && this.Current == '|')
                    intList.Add(this.CurrentIndex);
                flag = this.Current == '\\' && !flag;
                this.MoveNext();
            }
            return intList;
        }

        public class ParsedCharacterClass
        {
            public int Count;
            public string ErrorMessage = "";
            public int[] LeftBracketIndex = new int[31];
        }
    }
}
