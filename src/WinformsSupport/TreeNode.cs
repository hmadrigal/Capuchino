using System;
using System.Drawing;

namespace System.Windows.Forms
{
    public class TreeNode
    {
        public TreeNode(string text)
        {
            
        }

        public Color ForeColor { get; set; }
        public TreeNode Parent { get; set; }
        public int ImageIndex { get; set; }
        public int SelectedImageIndex { get; set; }
        public string Text { get; set; }
        public TreeNodeCollection Nodes { get; private set; } = new TreeNodeCollection();

        public object Tag { get; set; }

        public void EnsureVisible()
        {
            throw new NotImplementedException();
        }
    }
}
