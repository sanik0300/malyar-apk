using System;
using System.Collections.Generic;
using System.Text;

namespace malyar_apk
{
    public class TPModelDeletedEventArgs : EventArgs
    {
        public readonly int OldIndex;
        public readonly byte Range;
        public TPModelDeletedEventArgs(int indx, byte rng=1)
        {
            OldIndex = indx;
            Range = rng;
        }
    }

    public class TPModelAddedEventArgs : EventArgs
    {
        public readonly int PositionIndex;
        public readonly AdditionMode howtoadd;
        public TPModelAddedEventArgs(int indx, AdditionMode a=AdditionMode.Insert)
        {
            PositionIndex = indx;
            howtoadd = a;
        }
    }
}
