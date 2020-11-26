using System;
using System.Collections;

namespace RegDecoder
{
    public class ExpressionList
    {
        private ArrayList Expressions;

        public ExpressionList() => this.Expressions = new ArrayList();

        public ExpressionList(SubExpression expression)
        {
            this.Expressions = new ArrayList();
            this.Expressions.Add((object)expression);
        }

        public void Add(SubExpression expression) => this.Expressions.Add((object)expression);

        public int Count => this.Expressions.Count;

        public SubExpression this[int i] => (SubExpression)this.Expressions[i];
    }
}
