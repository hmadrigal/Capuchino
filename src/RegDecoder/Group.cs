using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RegDecoder
{
    public class Group : Element
    {
        public GroupType Type;
        public string Name;
        public string Name2;
        public SubExpression Content;
        private string Options;
        public CheckState SetX;
        public CheckState SetI;
        public CheckState SetM;
        public CheckState SetS;
        public CheckState SetN;
        private static Regex RegGroup = new Regex("^\\(\\?['<](?<Name>[a-zA-Z]?[\\w\\d]*)(?<GroupType>-)(?<Name2>[a-zA-Z]?[\\w\\d]*)['>](?<Contents>.*)\\)|\r\n^\\(\\?(?<Options>[imnsx-]{1,15}:)(?<Contents>.*)\\)|\r\n^\\(\\?(?<Options>[imnsx-]{1,15})\\)|\r\n^\\(\\?(?<GroupType>\\(|\\<\\!|\\!|\\>|\\#|\\:|\\=|\\<\\=|[<'](?<Name>[a-zA-Z][\\w\\d]*)[>']|[<'](?<Number>\\d*)[>'])(?<Contents>.*)\\)|\r\n^\\((?<Contents>.*)\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);

        public Group(CharBuffer buffer)
          : this(buffer, false)
        {
        }

        public Group(CharBuffer buffer, bool SkipCaptureNumber)
        {
            this.Image = ImageType.Group;
            this.Start = buffer.IndexInOriginalBuffer;
            bool flag = true;
            this.Literal = buffer.GetStringToMatchingParenthesis();
            if (this.Literal == "")
                return;
            Match match = Group.RegGroup.Match(this.Literal);
            if (match.Success)
            {
                string str = match.Groups["GroupType"].Value;
                string name = match.Groups[nameof(Name)].Value;
                string s = match.Groups["Number"].Value;
                string options = match.Groups[nameof(Options)].Value;
                string literal = match.Groups["Contents"].Value;
                int offset = this.Start + match.Groups["Contents"].Index;
                this.Name2 = match.Groups[nameof(Name2)].Value;
                if (options != "")
                {
                    this.DecodeOptions(options);
                    if (this.Type == GroupType.OptionsOutside)
                        flag = false;
                }
                else
                {
                    switch (str)
                    {
                        case "":
                            if (literal.Length > 0 && literal.Substring(0, 1) == "?")
                            {
                                this.Type = GroupType.Invalid;
                                literal = literal.Substring(1);
                                this.Content = new SubExpression(literal, offset + 1, buffer.IgnoreWhiteSpace, buffer.IsECMA);
                                this.Description = "Illegal group syntax";
                                this.IsValid = false;
                                break;
                            }
                            this.Type = GroupType.Numbered;
                            this.Name = SkipCaptureNumber ? "" : Backreference.AddNumber().ToString();
                            break;
                        case "!":
                            this.Type = GroupType.SuffixAbsent;
                            break;
                        case "#":
                            this.Type = GroupType.Comment;
                            flag = false;
                            break;
                        case "(":
                            this.Type = GroupType.Invalid;
                            this.Content = new SubExpression("", 0, false, false);
                            this.IsValid = false;
                            this.Description = "Syntax error in group definition";
                            flag = false;
                            buffer.Move(1 - this.Literal.Length);
                            break;
                        case "-":
                            int index1 = match.Groups[nameof(Name)].Index - 1;
                            int index2 = match.Groups[nameof(Name2)].Index + match.Groups[nameof(Name2)].Length;
                            char ch1 = this.Literal[index1];
                            char ch2 = this.Literal[index2];
                            this.IsValid = ch1 == '<' && ch2 == '>' || ch1 == '\'' && ch2 == '\'';
                            if (!this.IsValid)
                                this.Description = "Invalid syntax for balancing group";
                            this.Type = GroupType.Balancing;
                            this.Name = name;
                            int result;
                            if (this.Name != "")
                            {
                                if (int.TryParse(this.Name, out result))
                                    Backreference.AddNumber(result);
                                else
                                    Backreference.AddName(this.Name);
                            }
                            if (int.TryParse(this.Name2, out result))
                            {
                                if (!Backreference.ContainsNumber(this.Name2))
                                {
                                    this.Description = "Invalid group number in a balancing group: " + this.Name2;
                                    this.IsValid = false;
                                    break;
                                }
                                break;
                            }
                            if (!Backreference.ContainsName(this.Name2))
                            {
                                this.Description = "Invalid group name in a balancing group: " + this.Name2;
                                this.IsValid = false;
                                break;
                            }
                            break;
                        case ":":
                            this.Type = GroupType.Noncapturing;
                            break;
                        case "<!":
                            this.Type = GroupType.PrefixAbsent;
                            break;
                        case "<=":
                            this.Type = GroupType.PrefixPresent;
                            break;
                        case "=":
                            this.Type = GroupType.SuffixPresent;
                            break;
                        case ">":
                            this.Type = GroupType.Greedy;
                            break;
                        default:
                            char ch3 = str[0];
                            char ch4 = str[str.Length - 1];
                            this.IsValid = ch3 == '<' && ch4 == '>' || ch3 == '\'' && ch4 == '\'';
                            if (name.Length > 0)
                            {
                                this.Type = GroupType.Named;
                                this.Name = name;
                                Backreference.AddName(name);
                                if (!this.IsValid)
                                {
                                    this.Description = "[" + name + "] Invalid syntax for named group";
                                    break;
                                }
                                break;
                            }
                            if (s.Length > 0)
                            {
                                this.Type = GroupType.Numbered;
                                this.Name = s;
                                Backreference.AddNumber(int.Parse(s));
                                if (!this.IsValid)
                                {
                                    this.Description = "[" + s + "] Invalid syntax for numbered group";
                                    break;
                                }
                                break;
                            }
                            this.Type = GroupType.Named;
                            this.Name = "";
                            this.IsValid = false;
                            this.Description = "Missing name for a named group";
                            break;
                    }
                }
                bool WS = buffer.IgnoreWhiteSpace;
                if (this.Type == GroupType.OptionsInside || this.Type == GroupType.OptionsOutside)
                {
                    if (this.SetX == CheckState.Checked)
                        WS = true;
                    else if (this.SetX == CheckState.Unchecked)
                        WS = false;
                }
                if (this.IsValid || this.Type == GroupType.Named || (this.Type == GroupType.Numbered || this.Type == GroupType.Balancing))
                    this.Content = new SubExpression(literal, offset, WS, buffer.IsECMA);
            }
            else
            {
                this.Type = GroupType.Invalid;
                this.Content = new SubExpression("", 0, false, false);
                this.IsValid = false;
                flag = false;
                this.Description = "Syntax error in group definition";
                buffer.Move(1 - this.Literal.Length);
            }
            buffer.MoveNext();
            if (flag)
            {
                this.ParseRepetitions(buffer);
            }
            else
            {
                this.End = buffer.IndexInOriginalBuffer;
                this.RepeatType = Repeat.Once;
            }
            this.SetDescription();
        }

        private void SetDescription()
        {
            string str = "";
            switch (this.Type)
            {
                case GroupType.Balancing:
                    if (this.Name != "" && this.Name2 != "")
                    {
                        str = "Balancing group. Restore previous match for [" + this.Name + "]. Store the interval from that match to current match in [" + this.Name2 + "]";
                        break;
                    }
                    if (this.Name == "" && this.Name2 != "")
                    {
                        str = "Balancing group. Remove the most recent [" + this.Name2 + "] capture from the stack";
                        break;
                    }
                    if (this.Name == "")
                    {
                        this.Description = "Balancing group. Missing both group names.";
                        this.IsValid = false;
                        break;
                    }
                    this.Description = "Balancing group. Missing the second group name.";
                    this.IsValid = false;
                    break;
                case GroupType.Named:
                    str = "[" + this.Name + "]: A named capture group";
                    break;
                case GroupType.Numbered:
                    str = "[" + this.Name + "]: A numbered capture group";
                    break;
                case GroupType.Noncapturing:
                    str = "Match expression but don't capture it";
                    break;
                case GroupType.SuffixPresent:
                    str = "Match a suffix but exclude it from the capture";
                    break;
                case GroupType.PrefixPresent:
                    str = "Match a prefix but exclude it from the capture";
                    break;
                case GroupType.SuffixAbsent:
                    str = "Match if suffix is absent";
                    break;
                case GroupType.PrefixAbsent:
                    str = "Match if prefix is absent";
                    break;
                case GroupType.Greedy:
                    str = "Greedy subexpression";
                    break;
                case GroupType.Comment:
                    str = "Comment";
                    break;
                case GroupType.OptionsInside:
                case GroupType.OptionsOutside:
                    str = this.Options;
                    break;
                case GroupType.Invalid:
                    str = "";
                    break;
                default:
                    str = "";
                    break;
            }
            if (!this.IsValid)
                return;
            if (this.Type == GroupType.OptionsOutside)
                this.Description = str;
            else if (this.Content != null)
                this.Description = this.Description + str + ". [" + this.Content.Literal + "]";
            else
                this.Description += str;
        }

        public override TreeNode GetNode()
        {
            TreeNode node1 = new TreeNode(this.ToString());
            if (this.Type == GroupType.OptionsInside || this.Type == GroupType.OptionsOutside)
            {
                if (this.SetI == CheckState.Checked)
                    node1.Nodes.Add("Turn ON Ignore Case option");
                else if (this.SetI == CheckState.Unchecked)
                    node1.Nodes.Add("Turn OFF Ignore Case option");
                if (this.SetM == CheckState.Checked)
                    node1.Nodes.Add("Turn ON Multiline option");
                else if (this.SetM == CheckState.Unchecked)
                    node1.Nodes.Add("Turn OFF Multiline option");
                if (this.SetS == CheckState.Checked)
                    node1.Nodes.Add("Turn ON Single Line option");
                else if (this.SetS == CheckState.Unchecked)
                    node1.Nodes.Add("Turn OFF Single Line option");
                if (this.SetN == CheckState.Checked)
                    node1.Nodes.Add("Turn ON Explicit Capture option");
                else if (this.SetN == CheckState.Unchecked)
                    node1.Nodes.Add("Turn OFF Explicit Capture option");
                if (this.SetX == CheckState.Checked)
                    node1.Nodes.Add("Turn ON Ignore Pattern Whitespace option");
                else if (this.SetX == CheckState.Unchecked)
                    node1.Nodes.Add("Turn OFF Ignore Pattern Whitespace option");
            }
            if (this.Type != GroupType.Comment && this.Type != GroupType.OptionsOutside)
            {
                TreeNode node2 = this.Content != null ? this.Content.GetNode() : new TreeNode("NULL");
                Element.SetNode(node2, (Element)this.Content);
                node1.Nodes.Add(node2);
            }
            Element.SetNode(node1, (Element)this);
            return node1;
        }

        private void DecodeOptions(string options)
        {
            CharBuffer buffer = new CharBuffer(options);
            this.SetI = CheckState.Indeterminate;
            this.SetM = CheckState.Indeterminate;
            this.SetS = CheckState.Indeterminate;
            this.SetN = CheckState.Indeterminate;
            this.SetX = CheckState.Indeterminate;
            if (options.Substring(options.Length - 1, 1) == ":")
            {
                this.Type = GroupType.OptionsInside;
                this.Options = "Change options within a new noncapturing group [" + options + "]";
            }
            else
            {
                this.Type = GroupType.OptionsOutside;
                this.Options = "Change options within the enclosing group [" + options + "]";
            }
            bool flag = true;
            while (!buffer.End)
            {
                switch (buffer.Current)
                {
                    case '-':
                        flag = false;
                        goto case ':';
                    case ':':
                        buffer.MoveNext();
                        continue;
                    case 'i':
                        this.SetI = !flag ? CheckState.Unchecked : CheckState.Checked;
                        goto case ':';
                    case 'm':
                        this.SetM = !flag ? CheckState.Unchecked : CheckState.Checked;
                        goto case ':';
                    case 'n':
                        this.SetN = !flag ? CheckState.Unchecked : CheckState.Checked;
                        goto case ':';
                    case 's':
                        this.SetS = !flag ? CheckState.Unchecked : CheckState.Checked;
                        goto case ':';
                    case 'x':
                        this.SetX = !flag ? CheckState.Unchecked : CheckState.Checked;
                        goto case ':';
                    default:
                        Utility.ParseError("Error in options construct!", buffer);
                        this.IsValid = false;
                        goto case ':';
                }
            }
        }
    }
}
