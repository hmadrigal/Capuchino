using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace RegDecoder
{
    public class Backreference : Element
    {
        private static ArrayList Numbers = new ArrayList();
        private static ArrayList FirstPassNumbers = new ArrayList();
        private static ArrayList Names = new ArrayList();
        private string ASCII = "ASCII Octal ";
        private string Numbered = "Backreference to capture number: ";
        private string Named = "Backreference to capture named: ";
        private string MissingNumber = "Backreference to missing capture number: ";
        private string MissingName = "Backreference to missing capture name: ";
        private static Regex BackrefRegex = new Regex("^k[<'](?<Named>\\w+)[>']|^(?<Octal>0[0-7]{0,2})|^(?<Backreference>[1-9](?=\\D|$))|^(?<Decimal>[1-9]\\d+)\r\n\r\n", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        private static Regex OctalBackParseRegex = new Regex("^(?<Octal>[1-3][0-7]{0,2})|^(?<Octal>[4-7][0-7]?)\r\n\r\n", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        public static bool IsFirstPass = true;
        public static bool NeedsSecondPass = false;
        private bool isOctal;
        private bool isNamed;
        private string contents;

        public Backreference()
        {
            this.Image = ImageType.Group;
            this.isOctal = false;
            this.isNamed = false;
            this.contents = "";
        }

        public bool Parse(CharBuffer buffer)
        {
            this.Start = buffer.IndexInOriginalBuffer;
            if (buffer.Current != '\\')
                return false;
            buffer.MoveNext();
            if (buffer.End)
                return false;
            char current = buffer.Current;
            if (!char.IsDigit(current) && current != 'k')
            {
                buffer.Move(-1);
                return false;
            }
            Match match1 = Backreference.BackrefRegex.Match(buffer.GetEnd());
            if (match1.Success)
            {
                string s1 = match1.Groups[nameof(Backreference)].Value;
                string s2 = match1.Groups["Decimal"].Value;
                string str = match1.Groups["Octal"].Value;
                string s3 = match1.Groups["Named"].Value;
                this.Literal = "\\" + match1.Value;
                if (s3 != "")
                {
                    if (Backreference.Names.Contains((object)s3))
                    {
                        this.Description = this.Named + s3;
                    }
                    else
                    {
                        int result;
                        if (int.TryParse(s3, out result) && Backreference.NumbersContains(result))
                        {
                            this.Description = this.Numbered + s3;
                        }
                        else
                        {
                            this.Description = this.MissingName + s3;
                            this.IsValid = false;
                        }
                    }
                    buffer.Move(match1.Length);
                    this.ParseRepetitions(buffer);
                    this.isNamed = true;
                    this.contents = s3;
                    return true;
                }
                if (str != "")
                {
                    this.Description = this.ASCII + str;
                    buffer.Move(match1.Length);
                    this.ParseRepetitions(buffer);
                    this.Image = ImageType.Character;
                    this.isOctal = true;
                    this.contents = str;
                    return true;
                }
                if (!buffer.IsECMA)
                {
                    if (s1 != "")
                    {
                        if (Backreference.NumbersContains(int.Parse(s1)))
                        {
                            this.Description = this.Numbered + s1;
                        }
                        else
                        {
                            this.Description = this.MissingNumber + s1;
                            this.IsValid = false;
                        }
                        buffer.Move(match1.Length);
                        this.ParseRepetitions(buffer);
                        this.contents = s1;
                        return true;
                    }
                    if (Backreference.NumbersContains(int.Parse(s2)))
                    {
                        this.Description = this.Numbered + s2;
                        this.contents = s2;
                        buffer.Move(match1.Length);
                        this.ParseRepetitions(buffer);
                        return true;
                    }
                    Match match2 = Backreference.OctalBackParseRegex.Match(buffer.GetEnd());
                    if (!match2.Success)
                        return false;
                    this.Literal = "\\" + match2.Value;
                    this.Description = this.ASCII + match2.Groups["Octal"].Value;
                    buffer.Move(match2.Length);
                    this.ParseRepetitions(buffer);
                    this.Image = ImageType.Character;
                    this.isOctal = true;
                    this.contents = match2.Groups["Octal"].Value;
                    return true;
                }
                if (s1 != "")
                {
                    if (Backreference.NumbersContains(int.Parse(s1)))
                    {
                        this.Description = this.Numbered + s1;
                        buffer.Move(match1.Length);
                        this.contents = s1;
                        this.ParseRepetitions(buffer);
                        return true;
                    }
                    Match match2 = Backreference.OctalBackParseRegex.Match(buffer.GetEnd());
                    if (!match2.Success)
                        return false;
                    this.Literal = "\\" + match2.Value;
                    this.Description = this.ASCII + match2.Groups["Octal"].Value;
                    buffer.Move(match2.Length);
                    this.ParseRepetitions(buffer);
                    this.Image = ImageType.Character;
                    this.isOctal = true;
                    this.contents = match2.Groups["Octal"].Value;
                    return true;
                }
                if (!(s2 != ""))
                    return false;
                for (int length = s2.Length; length > 0; --length)
                {
                    string s4 = s2.Substring(0, length);
                    if (Backreference.NumbersContains(int.Parse(s4)))
                    {
                        this.Description = this.Numbered + s4;
                        this.Literal = "\\" + s4;
                        this.contents = s4;
                        buffer.Move(length);
                        this.ParseRepetitions(buffer);
                        return true;
                    }
                }
                Match match3 = Backreference.OctalBackParseRegex.Match(buffer.GetEnd());
                if (!match3.Success)
                    return false;
                this.Literal = "\\" + match3.Value;
                this.Description = this.ASCII + match3.Groups["Octal"].Value;
                buffer.Move(match3.Length);
                this.ParseRepetitions(buffer);
                this.Image = ImageType.Character;
                this.isOctal = true;
                this.contents = match3.Groups["Octal"].Value;
                return true;
            }
            if (current != 'k')
                return false;
            this.IsValid = false;
            this.Literal = "\\k";
            this.Description = "Invalid backreference";
            this.contents = "";
            buffer.MoveNext();
            this.End = buffer.IndexInOriginalBuffer;
            return true;
        }

        public static void InitializeSecondPass()
        {
            Backreference.AddNamedCaptureNumbers();
            Backreference.FirstPassNumbers = (ArrayList)Backreference.Numbers.Clone();
            Backreference.Numbers.Clear();
            Backreference.IsFirstPass = false;
            Backreference.NeedsSecondPass = false;
        }

        public static void AddNamedCaptureNumbers()
        {
            for (int index = 0; index < Backreference.Names.Count; ++index)
                Backreference.AddNumber();
        }

        public static bool NumbersContains(int n) => Backreference.IsFirstPass ? Backreference.Numbers.Contains((object)n) : Backreference.FirstPassNumbers.Contains((object)n);

        public static void AddNumber(int n)
        {
            if (Backreference.Numbers.Contains((object)n))
                return;
            Backreference.Numbers.Add((object)n);
        }

        public static int AddNumber()
        {
            if (Backreference.Numbers == null)
            {
                Backreference.AddNumber(1);
                return 1;
            }
            for (int n = 1; n <= Backreference.Numbers.Count + 1; ++n)
            {
                if (!Backreference.Numbers.Contains((object)n))
                {
                    Backreference.AddNumber(n);
                    return n;
                }
            }
            return 0;
        }

        public static void RemoveNumber(int n) => Backreference.Numbers.Remove((object)n);

        public static void RemoveNumber(string n)
        {
            int result;
            if (!int.TryParse(n, out result))
                return;
            Backreference.Numbers.Remove((object)result);
        }

        public static void ClearNumbers()
        {
            if (Backreference.Numbers == null)
                return;
            Backreference.Numbers.Clear();
        }

        public static bool ContainsNumber(string name)
        {
            if (!Regex.Match(name, "^\\d+$").Success)
                return false;
            return Backreference.IsFirstPass ? Backreference.Numbers.Contains((object)int.Parse(name)) : Backreference.FirstPassNumbers.Contains((object)int.Parse(name));
        }

        public static void AddName(string name)
        {
            if (Backreference.Names.Contains((object)name))
                return;
            Backreference.Names.Add((object)name);
        }

        public static bool ContainsName(string name) => Backreference.Names.Contains((object)name);

        public static void ClearNames()
        {
            if (Backreference.Names == null)
                return;
            Backreference.Names.Clear();
        }

        public bool IsOctal => this.isOctal;

        public bool IsNamed => this.isNamed;

        public string Contents => this.contents;
    }
}
