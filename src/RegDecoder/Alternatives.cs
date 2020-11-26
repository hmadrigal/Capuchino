using System;
using System.Windows.Forms;

namespace RegDecoder
{
    public class Alternatives : Element
    {
        private ExpressionList Expressions;

        public Alternatives()
        {
            this.Expressions = new ExpressionList();
            this.Image = ImageType.Alternative;
            this.Literal = "";
        }

        public int Count => this.Expressions.Count;

        public void Add(SubExpression expression)
        {
            if (this.Expressions.Count == 0)
            {
                this.Start = expression.Start;
                this.Literal = expression.Literal;
            }
            else
                this.Literal = this.Literal + "|" + expression.Literal;
            this.End = expression.End;
            this.Expressions.Add(expression);
        }

        public SubExpression this[int i] => this.Expressions[i];

        public ExpressionList GetList() => this.Expressions;

        public override TreeNode GetNode()
        {
            TreeNode node1 = new TreeNode(this.ToString());
            for (int i = 0; i < this.Expressions.Count; ++i)
            {
                SubExpression expression = this.Expressions[i];
                TreeNode[] nodes = expression.Exp.GetNodes();
                TreeNode node2;
                if (nodes.Length > 1)
                {
                    node2 = new TreeNode(expression.ToString());
                    Element.SetNode(node2, (Element)expression);
                    node2.Nodes.AddRange(nodes);
                }
                else if (nodes.Length == 1)
                {
                    node2 = nodes[0];
                }
                else
                {
                    node2 = new TreeNode("NULL");
                    Element.SetNode(node2, (Element)expression);
                }
                node1.Nodes.Add(node2);
            }
            Element.SetNode(node1, (Element)this);
            return node1;
        }

        public override string ToString() => "Select from " + this.Count.ToString() + " alternatives";
    }
}
