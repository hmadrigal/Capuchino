using System;
using System.Text.RegularExpressions;

namespace RegDecoder
{
    public class Element
    {
        public string Literal;
        public string Description;
        public int n;
        public int m;
        public bool AsFewAsPossible;
        public bool MatchIfAbsent;
        public Repeat RepeatType;
        public int Start;
        public int End;
        public ImageType Image;
        public bool IsValid = true;
        private Regex RepeatRegex = new Regex("^\\{(?<N>\\d+),(?<M>\\d*)\\}|^\\{(?<Exact>\\d+)\\}", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        private static int[] SelectedImages = new int[9]
        {
      0,
      4,
      4,
      1,
      2,
      3,
      0,
      4,
      5
        };
        private static int[] UnselectedImages = new int[9]
        {
      0,
      4,
      4,
      1,
      2,
      3,
      0,
      4,
      5
        };

        public Element() => this.RepeatType = Repeat.Once;

        public Element GetBaseElement()
        {
            if (this is SubExpression)
            {
                SubExpression subExpression = (SubExpression)this;
                if (subExpression.Exp.HasAlternatives)
                    return (Element)subExpression.Exp.Alternates;
                if (subExpression.Exp.Count == 1)
                    return subExpression.Exp[0];
            }
            return this;
        }

        public string RepeatString()
        {
            string str = "";
            switch (this.RepeatType)
            {
                case Repeat.Any:
                    str += "any number of repetitions";
                    goto case Repeat.Once;
                case Repeat.AtLeast:
                    str = str + "at least " + this.n.ToString() + " repetitions";
                    goto case Repeat.Once;
                case Repeat.Between:
                    str = str + "between " + this.n.ToString() + " and " + this.m.ToString() + " repetitions";
                    goto case Repeat.Once;
                case Repeat.Exact:
                    str = str + "exactly " + this.n.ToString() + " repetitions";
                    goto case Repeat.Once;
                case Repeat.Once:
                    if (this.AsFewAsPossible)
                        str += ", as few as possible";
                    return str;
                case Repeat.OneOrMore:
                    str += "one or more repetitions";
                    goto case Repeat.Once;
                case Repeat.ZeroOrOne:
                    str += "zero or one repetitions";
                    goto case Repeat.Once;
                default:
                    str += "(Error parsing repetition)";
                    goto case Repeat.Once;
            }
        }

        public void ParseRepetitions(CharBuffer buffer)
        {
            int currentIndex1 = buffer.CurrentIndex;
            buffer.SkipWhiteSpace();
            if (buffer.End)
            {
                this.RepeatType = Repeat.Once;
                buffer.MoveTo(currentIndex1);
                this.End = buffer.IndexInOriginalBuffer;
            }
            else
            {
                switch (buffer.Current)
                {
                    case '*':
                        this.RepeatType = Repeat.Any;
                        this.Literal += buffer.Current.ToString();
                        buffer.MoveNext();
                        break;
                    case '+':
                        this.RepeatType = Repeat.OneOrMore;
                        this.Literal += buffer.Current.ToString();
                        buffer.MoveNext();
                        break;
                    case '?':
                        this.RepeatType = Repeat.ZeroOrOne;
                        this.Literal += buffer.Current.ToString();
                        buffer.MoveNext();
                        break;
                    case '{':
                        this.ParseBrackets(buffer);
                        break;
                    default:
                        this.RepeatType = Repeat.Once;
                        buffer.MoveTo(currentIndex1);
                        this.End = buffer.IndexInOriginalBuffer;
                        return;
                }
                int currentIndex2 = buffer.CurrentIndex;
                buffer.SkipWhiteSpace();
                if (!buffer.End && buffer.Current == '?')
                {
                    this.AsFewAsPossible = true;
                    this.Literal += buffer.Current.ToString();
                    buffer.MoveNext();
                }
                else
                    buffer.MoveTo(currentIndex2);
                this.End = buffer.IndexInOriginalBuffer;
            }
        }

        private void ParseBrackets(CharBuffer buffer)
        {
            try
            {
                Match match = this.RepeatRegex.Match(buffer.GetEnd());
                if (!match.Success)
                    return;
                this.Literal += match.Value;
                buffer.Move(match.Length);
                string s1 = match.Groups["N"].Value;
                string s2 = match.Groups["M"].Value;
                string s3 = match.Groups["Exact"].Value;
                if (s1 == "" && s3 == "")
                {
                    Utility.ParseError("Error parsing the quantifier!", buffer);
                    this.IsValid = false;
                }
                else if (s3 != "")
                {
                    this.n = int.Parse(s3);
                    this.RepeatType = Repeat.Exact;
                }
                else if (s1 != "" && s2 != "")
                {
                    this.n = int.Parse(s1);
                    this.m = int.Parse(s2);
                    this.RepeatType = Repeat.Between;
                    if (this.n <= this.m)
                        return;
                    this.IsValid = false;
                    this.Description = "N is greater than M in quantifier!";
                }
                else if (s1 != "" && s2 == "")
                {
                    this.n = int.Parse(s1);
                    this.RepeatType = Repeat.AtLeast;
                }
                else
                {
                    Utility.ParseError("Error parsing the quantifier!", buffer);
                    this.IsValid = false;
                }
            }
            catch
            {
                Utility.ParseError("Error parsing the quantifier", buffer);
                this.IsValid = false;
            }
        }

        public override string ToString()
        {
            string str = this.RepeatString();
            return !(str != "") ? this.Description : this.Description + ", " + str;
        }

        public virtual TreeNode GetNode()
        {
            TreeNode node = new TreeNode(this.ToString());
            Element.SetNode(node, this);
            return node;
        }

        public static void SetNode(TreeNode node, Element element)
        {
            node.Tag = (object)element;
            if (!element.IsValid)
            {
                node.ForeColor = Color.Red;
                node.ImageIndex = 1;
                node.SelectedImageIndex = 1;
            }
            else
            {
                node.ImageIndex = 2;
                node.SelectedImageIndex = 2;
            }
        }
    }
}
