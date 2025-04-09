
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public struct Edge<N, L> where N : IComparable where L : IComparable
    {
        public N From { get;}
        public L LinkValue { get; set; }
        public N To { get; }

        public Edge(N from, L linkValue, N to)
        {
            From = from;
            LinkValue = linkValue;
            To = to;
        }
        public override string ToString() => $"{From} → [{LinkValue}] → {To}";
    }
}
