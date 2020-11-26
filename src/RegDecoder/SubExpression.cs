using System;
namespace RegDecoder
{
    public class SubExpression : Element
    {
        public Expression Exp;

        public SubExpression()
        {
            this.Exp = new Expression();
            this.Image = ImageType.Expression;
        }

        public SubExpression(Expression expression)
        {
            this.Exp = expression;
            this.Image = ImageType.Expression;
        }

        public SubExpression(string literal, int offset, bool WS, bool IsECMA)
        {
            this.Exp = new Expression(literal, offset, WS, IsECMA);
            this.Literal = literal;
            this.Start = offset;
            this.End = offset + literal.Length;
            this.Image = ImageType.Expression;
        }

        public SubExpression(
          string literal,
          int offset,
          bool WS,
          bool IsECMA,
          bool SkipFirstCaptureNumber)
        {
            this.Exp = new Expression(literal, offset, WS, IsECMA, SkipFirstCaptureNumber);
            this.Literal = literal;
            this.Start = offset;
            this.End = offset + literal.Length;
            this.Image = ImageType.Expression;
        }

        public override TreeNode GetNode()
        {
            TreeNode[] nodes = this.Exp.GetNodes();
            TreeNode node;
            if (nodes.Length > 1)
            {
                node = new TreeNode(this.Exp.Literal);
                node.Nodes.AddRange(this.Exp.GetNodes());
                Element.SetNode(node, (Element)this);
            }
            else if (nodes.Length == 1)
            {
                node = nodes[0];
            }
            else
            {
                node = new TreeNode("NULL");
                Element.SetNode(node, (Element)this);
            }
            return node;
        }

        public override string ToString() => this.Exp.ToString();
    }
}
