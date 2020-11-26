using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RegDecoder
{
    public class Conditional : Element
    {
        public Regex ConditionalRegex = new Regex("^\\(\\?(?<Expression>\\((?>(\\\\\\(|\\\\\\)|[^()])+|\\((?<depth>)|\\)(?<-depth>))*(?(depth)(?!))\\))(?<Contents>.*)\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        public Regex AlphanumericRegex = new Regex("^\\w+$", RegexOptions.Compiled);
        private Conditional.ExpType expType;
        public SubExpression Exp;
        public SubExpression Yes;
        public SubExpression No;
        public string Name;

        public Conditional() => this.Image = ImageType.Conditional;

        public bool Parse(CharBuffer buffer)
        {
            this.Start = buffer.IndexInOriginalBuffer;
            if (buffer.End)
            {
                Utility.ParseError("Reached end of buffer looking for a conditional!", buffer);
                this.IsValid = false;
                return false;
            }
            this.Literal = buffer.GetStringToMatchingParenthesis();
            Match match = this.ConditionalRegex.Match(this.Literal);
            if (match.Success)
            {
                System.Text.RegularExpressions.Group group1 = match.Groups["Expression"];
                int offset1 = this.Start + group1.Index;
                string str1 = group1.Value.Substring(1, group1.Value.Length - 2);
                this.Name = str1;
                this.expType = Conditional.ExpType.Expression;
                this.Exp = new SubExpression(group1.Value, offset1, buffer.IgnoreWhiteSpace, buffer.IsECMA, true);
                this.Exp.Start = offset1;
                this.Exp.End = this.Exp.Start + group1.Length;
                if (!(this.Exp.Exp[0] is Group))
                {
                    this.expType = Conditional.ExpType.NotAGroup;
                }
                else
                {
                    Group group2 = (Group)this.Exp.Exp[0];
                    switch (group2.Type)
                    {
                        case GroupType.Balancing:
                            group2.Description = "Test condition cannot be a balancing group";
                            group2.IsValid = false;
                            this.IsValid = false;
                            break;
                        case GroupType.Named:
                            group2.Description = "Test condition cannot be a named group";
                            group2.IsValid = false;
                            this.IsValid = false;
                            break;
                        case GroupType.Numbered:
                            if (Backreference.ContainsName(str1))
                            {
                                this.expType = Conditional.ExpType.NamedCapture;
                                break;
                            }
                            if (int.TryParse(str1, out int _))
                            {
                                this.expType = Conditional.ExpType.NumberedCapture;
                                if (!Backreference.ContainsNumber(str1))
                                {
                                    this.expType = Conditional.ExpType.NonExistentNumber;
                                    group2.IsValid = false;
                                    break;
                                }
                                break;
                            }
                            if (this.AlphanumericRegex.Match(str1).Success)
                            {
                                if (char.IsDigit(str1[0]))
                                {
                                    this.expType = Conditional.ExpType.InvalidName;
                                    group2.IsValid = false;
                                    break;
                                }
                                this.expType = Conditional.ExpType.NonExistentName;
                                break;
                            }
                            group2.Description = "Match if prefix is present";
                            break;
                        case GroupType.SuffixPresent:
                            group2.Description = "Match if suffix is present";
                            break;
                        case GroupType.PrefixPresent:
                            group2.Description = "Match if prefix is present";
                            break;
                        case GroupType.SuffixAbsent:
                            group2.Description = "Match if suffix is absent";
                            break;
                        case GroupType.PrefixAbsent:
                            group2.Description = "Match if prefix is absent";
                            break;
                        case GroupType.Comment:
                            group2.Description = "Test condition cannot be a comment!";
                            group2.IsValid = false;
                            this.IsValid = false;
                            break;
                        default:
                            group2.Description = "Test condition is";
                            break;
                    }
                    group2.Description = group2.Description + " " + group2.Literal;
                }
                System.Text.RegularExpressions.Group group3 = match.Groups["Contents"];
                string str2 = group3.Value;
                List<int> nakedPipes = new CharBuffer(str2).FindNakedPipes();
                int offset2 = this.Start + group3.Index;
                switch (nakedPipes.Count)
                {
                    case 0:
                        this.Yes = new SubExpression(str2, offset2, buffer.IgnoreWhiteSpace, buffer.IsECMA);
                        this.Yes.Start = offset2;
                        this.Yes.End = this.Yes.Start + group3.Length;
                        this.Description = "Conditional Expression with \"Yes\" clause only";
                        break;
                    case 1:
                        int startIndex = nakedPipes[0] + 1;
                        this.Yes = new SubExpression(str2.Substring(0, startIndex - 1), offset2, buffer.IgnoreWhiteSpace, buffer.IsECMA);
                        this.Yes.Start = offset2;
                        this.Yes.End = this.Yes.Start + startIndex - 1;
                        int offset3 = this.Yes.Start + startIndex;
                        this.No = new SubExpression(str2.Substring(startIndex), offset3, buffer.IgnoreWhiteSpace, buffer.IsECMA);
                        this.No.Start = offset3;
                        this.No.End = this.Yes.Start + group3.Length;
                        this.Description = "Conditional Expression with \"Yes\" and \"No\" clause";
                        break;
                    default:
                        this.Yes = new SubExpression(str2, offset2, buffer.IgnoreWhiteSpace, buffer.IsECMA);
                        this.Yes.Start = offset2;
                        this.Yes.End = this.Yes.Start + group3.Length;
                        this.Description = "Too many | symbols in a conditional expression";
                        this.IsValid = false;
                        break;
                }
                buffer.MoveNext();
                this.ParseRepetitions(buffer);
                return true;
            }
            buffer.Move(1 - this.Literal.Length);
            return false;
        }

        public override TreeNode GetNode()
        {
            StringElement stringElement = new StringElement(this.Name);
            stringElement.Start = this.Exp.Start + 1;
            stringElement.End = this.Exp.Start + 1 + this.Name.Length;
            string text1 = "";
            string text2 = "";
            TreeNode node1 = new TreeNode(this.ToString());
            if (this.Yes != null)
                text1 = "If yes, search for [" + this.Yes.Literal + "]";
            if (this.No != null)
                text2 = "If no, search for [" + this.No.Literal + "]";
            TreeNode node2;
            switch (this.expType)
            {
                case Conditional.ExpType.Expression:
                    node2 = this.Exp.Exp.GetNodes()[0];
                    Element.SetNode(node2, (Element)this.Exp);
                    break;
                case Conditional.ExpType.NotAGroup:
                    node2 = new TreeNode("Test condition is " + this.Exp.Literal);
                    Element.SetNode(node2, (Element)this.Exp);
                    break;
                case Conditional.ExpType.NamedCapture:
                    node2 = new TreeNode("Did the capture named [" + this.Name + "] match?");
                    Element.SetNode(node2, (Element)stringElement);
                    break;
                case Conditional.ExpType.NumberedCapture:
                    node2 = new TreeNode("Did capture number " + this.Name + " match?");
                    Element.SetNode(node2, (Element)stringElement);
                    break;
                case Conditional.ExpType.NonExistentName:
                    node2 = new TreeNode("Since the capture named [" + this.Name + "] does not exist, this test will always fail.");
                    Element.SetNode(node2, (Element)stringElement);
                    break;
                case Conditional.ExpType.NonExistentNumber:
                    node2 = new TreeNode("Capture number [" + this.Name + "] does not exist!");
                    stringElement.IsValid = false;
                    this.IsValid = false;
                    Element.SetNode(node2, (Element)stringElement);
                    break;
                case Conditional.ExpType.InvalidName:
                    node2 = new TreeNode("Invalid capture name [" + this.Name + "]");
                    stringElement.IsValid = false;
                    this.IsValid = false;
                    Element.SetNode(node2, (Element)stringElement);
                    break;
                default:
                    node2 = new TreeNode("There was an error in parsing a conditional!");
                    node2.Nodes.AddRange(this.Exp.Exp.GetNodes());
                    Element.SetNode(node2, (Element)this.Exp);
                    break;
            }
            TreeNode node3 = new TreeNode(text1);
            node3.Nodes.AddRange(this.Yes.Exp.GetNodes());
            Element.SetNode(node3, (Element)this.Yes);
            node1.Nodes.Add(node2);
            node1.Nodes.Add(node3);
            if (this.No != null)
            {
                TreeNode node4 = new TreeNode(text2);
                Element.SetNode(node4, (Element)this.No);
                node4.Nodes.AddRange(this.No.Exp.GetNodes());
                node1.Nodes.Add(node4);
            }
            Element.SetNode(node1, (Element)this);
            return node1;
        }

        private enum ExpType
        {
            Expression,
            NotAGroup,
            NamedCapture,
            NumberedCapture,
            NonExistentName,
            NonExistentNumber,
            InvalidName,
        }
    }
}
