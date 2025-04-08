using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public class GraphException : Exception
    {
        public GraphException(string message) : base(message) { }
    }

    public class CyclicGraphException : GraphException
    {
        public CyclicGraphException() : base("Adding this link would create a cycle in the graph") { }
    }

    public class NodeNotFoundException<N> : GraphException
    {
        public N Node { get; }

        public NodeNotFoundException(N node)
            : base($"Node {node} not found in graph")
        {
            Node = node;
        }
    }

    public class LinkAlreadyExistsException<N> : GraphException
    {
        public N From { get; }
        public N To { get; }

        public LinkAlreadyExistsException(N from, N to)
            : base($"Link from {from} to {to} already exists")
        {
            From = from;
            To = to;
        }
    }

    public class LinkNotFoundException<N> : GraphException
    {
        public N From { get; }
        public N To { get; }

        public LinkNotFoundException(N from, N to)
            : base($"Link from {from} to {to} not found")
        {
            From = from;
            To = to;
        }
    }
}
