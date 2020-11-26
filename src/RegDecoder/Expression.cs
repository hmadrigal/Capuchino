using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RegDecoder
{
    public class Expression : CollectionBase
    {
        public string Literal;
        public bool IgnoreWhitespace = true;
        private Alternatives alternatives;
        public bool IsECMA;
        public static bool IsWhitespaceVisible;
        public bool Cancel;

        public Expression(string literal, int offset, bool WS, bool isECMA)
        {
            this.alternatives = new Alternatives();
            this.IgnoreWhitespace = WS;
            this.IsECMA = isECMA;
            this.Literal = literal;
            this.Cancel = false;
            this.Parse(offset);
        }

        public Expression(
          string literal,
          int offset,
          bool WS,
          bool isECMA,
          bool SkipFirstCaptureNumber)
        {
            this.alternatives = new Alternatives();
            this.IgnoreWhitespace = WS;
            this.IsECMA = isECMA;
            this.Literal = literal;
            this.Cancel = false;
            this.Parse(offset, (BackgroundWorker)null, SkipFirstCaptureNumber);
        }

        public Expression(string literal, int offset, bool WS, bool isECMA, BackgroundWorker worker)
        {
            this.alternatives = new Alternatives();
            this.IgnoreWhitespace = WS;
            this.IsECMA = isECMA;
            this.Literal = literal;
            this.Cancel = false;
            this.Parse(offset, worker, false);
        }

        public Expression()
        {
            this.alternatives = new Alternatives();
            this.Literal = "";
        }

        public Expression(CharBuffer buffer)
        {
            this.Literal = buffer.GetEnd();
            this.Parse();
        }

        public Expression Clone()
        {
            Expression expression = new Expression();
            foreach (Element element in (IEnumerable)this.List)
                expression.List.Add((object)element);
            expression.Literal = this.Literal;
            expression.IgnoreWhitespace = this.IgnoreWhitespace;
            expression.IsECMA = this.IsECMA;
            return expression;
        }

        public bool HasAlternatives => this.alternatives.Count > 0;

        public Alternatives Alternates => this.alternatives;

        public new void Clear()
        {
            this.List.Clear();
            this.Literal = "";
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("");
            if (this.alternatives.Count > 0)
            {
                for (int i = 0; i < this.alternatives.Count; ++i)
                {
                    SubExpression alternative = this.alternatives[i];
                    if (i != 0)
                        stringBuilder.Append("|");
                    stringBuilder.Append(alternative.ToString());
                }
            }
            else
            {
                foreach (Element element in (IEnumerable)this.List)
                {
                    if (element.GetType().Name == "SubExpression")
                        stringBuilder.Append(((SubExpression)element).Exp.ToString());
                    else
                        stringBuilder.Append(element.Literal);
                }
            }
            return stringBuilder.ToString();
        }

        public virtual void AddRange(Element[] elements)
        {
            foreach (object element in elements)
                this.List.Add(element);
        }

        public virtual void AddRange(Expression expression)
        {
            foreach (Element element in expression)
                this.List.Add((object)element);
        }

        public virtual void Add(Element element) => this.List.Add((object)element);

        public virtual bool Contains(Element element) => this.List.Contains((object)element);

        public virtual int IndexOf(Element element) => this.List.IndexOf((object)element);

        public virtual void Insert(int index, Element element) => this.List.Insert(index, (object)element);

        public virtual Element this[int index]
        {
            get => (Element)this.List[index];
            set => this.List[index] = (object)value;
        }

        public virtual void Remove(Element element) => this.List.Remove((object)element);

        public virtual Expression.Enumerator GetEnumerator() => new Expression.Enumerator(this);

        public TreeNode[] GetNodes()
        {
            ArrayList arrayList = new ArrayList();
            if (this.alternatives.Count != 0)
            {
                arrayList.Add((object)this.alternatives.GetNode());
            }
            else
            {
                foreach (Element element in this)
                {
                    if (Expression.IsWhitespaceVisible || element.GetType().Name != "WhiteSpace")
                        arrayList.Add((object)element.GetNode());
                }
            }
            TreeNode[] treeNodeArray = new TreeNode[arrayList.Count];
            for (int index = 0; index < arrayList.Count; ++index)
            {
                TreeNode node = (TreeNode)arrayList[index];
                this.Recolor(node);
                treeNodeArray[index] = node;
            }
            return treeNodeArray;
        }

        private void Recolor(TreeNode node)
        {
            if (node.ForeColor == Color.Red)
            {
                node.EnsureVisible();
                for (TreeNode parent = node.Parent; parent != null; parent = parent.Parent)
                {
                    parent.ImageIndex = 0;
                    parent.SelectedImageIndex = 0;
                }
            }
            if (node.Nodes.Count <= 0)
                return;
            foreach (TreeNode node1 in node.Nodes)
                this.Recolor(node1);
        }

        public Expression Stringify()
        {
            if (this.alternatives.Count != 0)
                return this;
            Expression expression = new Expression();
            SubExpression subExpression = (SubExpression)null;
            bool flag = true;
            foreach (Element element in this)
            {
                string name = element.GetType().Name;
                if ((name == "Character" || name == "SpecialCharacter" || name == "Backreference" ? 1 : (name == "NamedClass" ? 1 : 0)) != 0)
                {
                    if (flag)
                    {
                        subExpression = new SubExpression();
                        subExpression.Exp.Add(element);
                        subExpression.Start = element.Start;
                        subExpression.End = element.End;
                        subExpression.Exp.Literal = element.Literal;
                        subExpression.Literal = element.Literal;
                        flag = false;
                    }
                    else
                    {
                        subExpression.Exp.Add(element);
                        subExpression.End = element.End;
                        subExpression.Exp.Literal += element.Literal;
                        subExpression.Literal += element.Literal;
                    }
                }
                else
                {
                    if (!flag)
                    {
                        expression.Add((Element)subExpression);
                        flag = true;
                    }
                    expression.Add(element);
                }
            }
            if (!flag)
                expression.Add((Element)subExpression);
            return expression;
        }

        public void Parse() => this.Parse(0);

        public void Parse(int offset) => this.Parse(offset, (BackgroundWorker)null, false);

        public void Parse(int offset, BackgroundWorker worker, bool SkipFirstCaptureNumber)
        {
            CharBuffer buffer = new CharBuffer(this.Literal);
            buffer.Offset = offset;
            buffer.IgnoreWhiteSpace = this.IgnoreWhitespace;
            buffer.IsECMA = this.IsECMA;
            while (!buffer.End && (worker == null || !worker.CancellationPending))
            {
                int inOriginalBuffer1 = buffer.IndexInOriginalBuffer;
                if (this.IgnoreWhitespace)
                {
                    string whiteSpace = buffer.GetWhiteSpace();
                    if (whiteSpace.Length > 0)
                        this.Add((Element)new WhiteSpace(inOriginalBuffer1, whiteSpace));
                }
                if (!buffer.End)
                {
                    switch (buffer.Current)
                    {
                        case '\t':
                        case '\n':
                        case '\r':
                        case ' ':
                        case '$':
                        case '.':
                        case '^':
                            this.Add((Element)new SpecialCharacter(buffer));
                            continue;
                        case '#':
                            if (this.IgnoreWhitespace)
                            {
                                this.Add((Element)new Comment(buffer));
                                continue;
                            }
                            this.Add((Element)new Character(buffer));
                            continue;
                        case '(':
                            Conditional conditional = new Conditional();
                            if (conditional.Parse(buffer))
                            {
                                this.Add((Element)conditional);
                                Backreference.NeedsSecondPass = true;
                                continue;
                            }
                            Group group = new Group(buffer, SkipFirstCaptureNumber);
                            if (group.Type == GroupType.OptionsOutside)
                            {
                                if (group.SetX == CheckState.Checked)
                                    this.IgnoreWhitespace = true;
                                else if (group.SetX == CheckState.Unchecked)
                                    this.IgnoreWhitespace = false;
                                buffer.IgnoreWhiteSpace = this.IgnoreWhitespace;
                            }
                            this.Add((Element)group);
                            continue;
                        case ')':
                            Character character1 = new Character(buffer);
                            character1.IsValid = false;
                            character1.Description = "Unbalanced parenthesis";
                            this.Add((Element)character1);
                            continue;
                        case '*':
                        case '+':
                        case '?':
                            Character character2 = new Character(buffer, true);
                            character2.Description = character2.Literal + " Misplaced quantifier";
                            character2.IsValid = false;
                            this.Add((Element)character2);
                            continue;
                        case '[':
                            this.Add((Element)new CharacterClass(buffer));
                            continue;
                        case '\\':
                            if (SpecialCharacter.NextIsWhitespace(buffer))
                            {
                                this.Add((Element)new SpecialCharacter(buffer));
                                continue;
                            }
                            Backreference back = new Backreference();
                            if (back.Parse(buffer))
                            {
                                Backreference.NeedsSecondPass = true;
                                if (back.IsOctal)
                                {
                                    this.Add((Element)new SpecialCharacter(back));
                                    continue;
                                }
                                this.Add((Element)back);
                                continue;
                            }
                            NamedClass namedClass = new NamedClass();
                            if (namedClass.Parse(buffer))
                            {
                                this.Add((Element)namedClass);
                                continue;
                            }
                            this.Add((Element)new SpecialCharacter(buffer));
                            continue;
                        case '{':
                            Character character3 = new Character(buffer, true);
                            character3.Description = character3.Literal + " Misplaced quantifier";
                            character3.IsValid = false;
                            if (character3.RepeatType == Repeat.Once)
                            {
                                this.Add((Element)new Character(buffer));
                                continue;
                            }
                            this.Add((Element)character3);
                            continue;
                        case '|':
                            SubExpression expression = new SubExpression(this.Clone());
                            expression.Literal = buffer.Substring(0, buffer.CurrentIndex);
                            expression.Start = buffer.Offset;
                            expression.End = buffer.IndexInOriginalBuffer;
                            this.alternatives.Add(expression);
                            buffer.MoveNext();
                            int inOriginalBuffer2 = buffer.IndexInOriginalBuffer;
                            buffer = new CharBuffer(buffer.GetEnd());
                            buffer.Offset = inOriginalBuffer2;
                            buffer.IgnoreWhiteSpace = this.IgnoreWhitespace;
                            buffer.IsECMA = this.IsECMA;
                            this.Clear();
                            continue;
                        default:
                            this.Add((Element)new Character(buffer));
                            continue;
                    }
                }
                else
                    break;
            }
            if (worker != null && worker.CancellationPending)
            {
                this.Cancel = true;
            }
            else
            {
                if (this.alternatives.Count == 0)
                    return;
                SubExpression expression = new SubExpression(this.Clone());
                expression.Exp.alternatives = new Alternatives();
                expression.Start = buffer.Offset;
                expression.End = buffer.IndexInOriginalBuffer;
                expression.Literal = buffer.Substring(0, buffer.CurrentIndex);
                this.alternatives.Add(expression);
                this.alternatives.Start = 0;
                this.alternatives.End = buffer.IndexInOriginalBuffer;
            }
        }

        public class Enumerator : IEnumerator
        {
            private IEnumerator wrapped;

            public Enumerator(Expression expression) => this.wrapped = ((CollectionBase)expression).GetEnumerator();

            public object Current => (object)(Element)this.wrapped.Current;

            object IEnumerator.Current => (object)(Element)this.wrapped.Current;

            public bool MoveNext() => this.wrapped.MoveNext();

            public void Reset() => this.wrapped.Reset();
        }
    }
}
