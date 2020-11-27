using System;
namespace Ultrapico.Undo
{
    public class UndoRedo
    {
        private string[] Buffer;
        private string[] action;
        private int pIndex;
        private int Origin;
        private int count;
        private int MaxCount;

        private int RealIndex(int pindex)
        {
            int num = this.MaxCount - this.Origin;
            return pindex < num ? this.Origin + pindex : pindex - num;
        }

        private void pIncrement()
        {
            ++this.pIndex;
            if (this.pIndex != this.MaxCount)
                return;
            this.pIndex = this.MaxCount - 1;
            ++this.Origin;
            if (this.Origin != this.MaxCount)
                return;
            this.Origin = 0;
        }

        private void pDecrement()
        {
            --this.pIndex;
            if (this.pIndex != -1)
                return;
            this.pIndex = this.MaxCount - 1;
        }

        public bool UndoPossible => this.count > 0 && this.pIndex > 0;

        public bool RedoPossible => this.pIndex < this.count - 1;

        public int Count => this.count;

        public int Index => this.pIndex;

        public string this[int pindex] => this.Buffer[this.RealIndex(pindex)];

        public string UndoAction => this.pIndex > 0 ? this.action[this.RealIndex(this.pIndex)] : "";

        public string RedoAction => this.pIndex + 1 < this.count ? this.action[this.RealIndex(this.pIndex + 1)] : "";

        public string Action(int pindex) => this.action[this.RealIndex(pindex)];

        public UndoRedo(int maxCount)
        {
            this.Buffer = new string[maxCount];
            this.action = new string[maxCount];
            this.MaxCount = maxCount;
            this.pIndex = -1;
            this.count = 0;
            this.Origin = 0;
        }

        public UndoRedo()
          : this(20)
        {
        }

        public void Save(string state) => this.Save(state, nameof(Save));

        public void Save(string state, string verb)
        {
            if (this.pIndex != -1 && !(state != this.Buffer[this.RealIndex(this.pIndex)]))
                return;
            this.pIncrement();
            this.count = this.pIndex + 1;
            this.Buffer[this.RealIndex(this.pIndex)] = state;
            this.action[this.RealIndex(this.pIndex)] = verb;
        }

        public string Undo()
        {
            if (!this.UndoPossible)
                return (string)null;
            this.pDecrement();
            return this.Buffer[this.RealIndex(this.pIndex)];
        }

        public string Undo(string state)
        {
            this.Save(state);
            return this.Undo();
        }

        public string Redo()
        {
            if (!this.RedoPossible)
                return (string)null;
            this.pIncrement();
            return this.Buffer[this.RealIndex(this.pIndex)];
        }
    }
}
