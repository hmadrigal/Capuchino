using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Forms
{
    public class TreeNodeCollection : List<TreeNode>, IList
    {
        public void Add(string text)
        {
            base.Add(new TreeNode(text));
        }
    }
}
