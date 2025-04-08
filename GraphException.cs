using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public static class GraphExceptions
    {
        public class NodeNotFoundException<T> : Exception
        {
            public NodeNotFoundException(T node)
                : base($"Node '{node}' not found in the graph.") { }
        }

        public class LinkNotFoundException<T> : Exception
        {
            public LinkNotFoundException(T from, T to)
                : base($"Link from '{from}' to '{to}' not found in the graph.") { }
        }

        public class LinkAlreadyExistsException<T> : Exception
        {
            public LinkAlreadyExistsException(T from, T to)
                : base($"Link from '{from}' to '{to}' already exists.") { }
        }

        public class NodeAlreadyExistsException<T> : Exception
        {
            public NodeAlreadyExistsException(T value)
                : base($"Node with value '{value}' already exists.") { }
        }

        public class CycleDetectedException : Exception
        {
            public CycleDetectedException()
                : base("Graph contains a cycle.") { }
        }

        public class ImmutableGraphModificationException : Exception
        {
            public ImmutableGraphModificationException()
                : base("Cannot modify an immutable graph.") { }
        }
    }
}
