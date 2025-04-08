using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public class LinkedGraph<N, L> : IGraph<N, L>
        where N : IComparable<N>
        where L : IComparable<L>
    {
        #region Node and Edge structures
        private class GraphNode
        {
            public N Value;
            public EdgeNode FirstOutgoingEdge;
            public EdgeNode FirstIncomingEdge;
            public int Index;
        }

        private class EdgeNode
        {
            public GraphNode From;
            public GraphNode To;
            public L Weight;
            public EdgeNode NextOutgoing;
            public EdgeNode NextIncoming;
        }
        #endregion 

        private GraphNode[] nodes;
        private int count;
        private int capacity;
        private const int DefaultCapacity = 4;

        public LinkedGraph()
        {
            capacity = DefaultCapacity;
            nodes = new GraphNode[capacity];
        }
        public int Count => count;
        public bool IsEmpty => count == 0;

        public IEnumerable<N> Nodes
        {
            get
            {
                for (int i = 0; i < count; i++)
                    yield return nodes[i].Value;
            }
        }

        public IEnumerable<(N from, N to, L link)> Links
        {
            get
            {
                for (int i = 0; i < count; i++)
                {
                    var current = nodes[i].FirstOutgoingEdge;
                    while (current != null)
                    {
                        yield return (current.From.Value, current.To.Value, current.Weight);
                        current = current.NextOutgoing;
                    }
                }
            }
        }

        public void AddNode(N node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (Contains(node))
                return;

            if (count == capacity)
                Resize(capacity * 2);

            nodes[count++] = new GraphNode { Value = node };
        }

        public void AddLink(N from, N to, L weight)
        {
            if (from == null || to == null)
                throw new ArgumentNullException();

            var fromNode = FindNode(from);
            var toNode = FindNode(to);

            if (fromNode == null)
                throw new NodeNotFoundException<N>(from);
            if (toNode == null)
                throw new NodeNotFoundException<N>(to);
            if (HasLink(fromNode, toNode))
                throw new LinkAlreadyExistsException<N>(from, to);

            if (WouldCreateCycle(fromNode, toNode))
                throw new CyclicGraphException();

            var newEdge = new EdgeNode
            {
                From = fromNode,
                To = toNode,
                Weight = weight,
                NextOutgoing = fromNode.FirstOutgoingEdge,
                NextIncoming = toNode.FirstIncomingEdge
            };

            fromNode.FirstOutgoingEdge = newEdge;
            toNode.FirstIncomingEdge = newEdge;
        }

        private bool WouldCreateCycle(GraphNode from, GraphNode to)
        {
            if (from == to)
                return true;

            var visited = new bool[count];
            var queue = new CustomQueue<GraphNode>();
            queue.Enqueue(to);
            visited[to.Index] = true;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var edge = current.FirstOutgoingEdge;

                while (edge != null)
                {
                    if (edge.To == from)
                        return true;

                    if (!visited[edge.To.Index])
                    {
                        visited[edge.To.Index] = true;
                        queue.Enqueue(edge.To);
                    }

                    edge = edge.NextOutgoing;
                }
            }

            return false;
        }
      

        public bool Contains(N node)
        {
            return FindNode(node) != null;
        }

        private GraphNode FindNode(N node)
        {
            for (int i = 0; i < count; i++)
            {
                if (nodes[i].Value.CompareTo(node) == 0)
                    return nodes[i];
            }
            return null;
        }

        private bool HasLink(GraphNode from, GraphNode to)
        {
            var current = from.FirstOutgoingEdge;
            while (current != null)
            {
                if (current.To == to)
                    return true;
                current = current.NextOutgoing;
            }
            return false;
        }

        public bool HasLink(N from, N to)
        {
            var fromNode = FindNode(from);
            var toNode = FindNode(to);
            return fromNode != null && toNode != null && HasLink(fromNode, toNode);
        }

        public L GetLink(N from, N to)
        {
            var fromNode = FindNode(from);
            var toNode = FindNode(to);

            if (fromNode == null)
                throw new NodeNotFoundException<N>(from);
            if (toNode == null)
                throw new NodeNotFoundException<N>(to);

            var current = fromNode.FirstOutgoingEdge;
            while (current != null)
            {
                if (current.To == toNode)
                    return current.Weight;
                current = current.NextOutgoing;
            }

            throw new LinkNotFoundException<N>(from, to);
        }

        public IEnumerable<N> GetNeighbors(N node)
        {
            var nodeObj = FindNode(node);
            if (nodeObj == null)
                throw new NodeNotFoundException<N>(node);

            var current = nodeObj.FirstOutgoingEdge;
            while (current != null)
            {
                yield return current.To.Value;
                current = current.NextOutgoing;
            }
        }

        public IEnumerable<(N to, L link)> GetOutgoingLinks(N node)
        {
            var nodeObj = FindNode(node);
            if (nodeObj == null)
                throw new NodeNotFoundException<N>(node);

            var current = nodeObj.FirstOutgoingEdge;
            while (current != null)
            {
                yield return (current.To.Value, current.Weight);
                current = current.NextOutgoing;
            }
        }

        public IEnumerable<(N from, L link)> GetIncomingLinks(N node)
        {
            var nodeObj = FindNode(node);
            if (nodeObj == null)
                throw new NodeNotFoundException<N>(node);

            var current = nodeObj.FirstIncomingEdge;
            while (current != null)
            {
                yield return (current.From.Value, current.Weight);
                current = current.NextIncoming;
            }
        }

        public void RemoveNode(N node)
        {
            var nodeToRemove = FindNode(node);
            if (nodeToRemove == null)
                throw new NodeNotFoundException<N>(node);

            // Remove all outgoing edges
            while (nodeToRemove.FirstOutgoingEdge != null)
            {
                RemoveEdge(nodeToRemove.FirstOutgoingEdge);
            }

            // Remove all incoming edges
            while (nodeToRemove.FirstIncomingEdge != null)
            {
                RemoveEdge(nodeToRemove.FirstIncomingEdge);
            }

            // Shift array
            int index = Array.FindIndex(nodes, n => n == nodeToRemove);
            for (int i = index; i < count - 1; i++)
            {
                nodes[i] = nodes[i + 1];
            }

            nodes[count - 1] = null;
            count--;
        }

        public void RemoveLink(N from, N to)
        {
            var fromNode = FindNode(from);
            var toNode = FindNode(to);

            if (fromNode == null)
                throw new NodeNotFoundException<N>(from);
            if (toNode == null)
                throw new NodeNotFoundException<N>(to);

            EdgeNode edgeToRemove = null;
            EdgeNode prevOutgoing = null;

            // Find outgoing edge
            var current = fromNode.FirstOutgoingEdge;
            while (current != null)
            {
                if (current.To == toNode)
                {
                    edgeToRemove = current;
                    break;
                }
                prevOutgoing = current;
                current = current.NextOutgoing;
            }

            if (edgeToRemove == null)
                throw new LinkNotFoundException<N>(from, to);

            RemoveEdge(edgeToRemove, fromNode, prevOutgoing);
        }

        private void RemoveEdge(EdgeNode edge, GraphNode fromNode = null, EdgeNode prevOutgoing = null)
        {
            // Remove from outgoing list
            if (fromNode == null)
                fromNode = edge.From;

            if (prevOutgoing == null)
                fromNode.FirstOutgoingEdge = edge.NextOutgoing;
            else
                prevOutgoing.NextOutgoing = edge.NextOutgoing;

            // Remove from incoming list
            var toNode = edge.To;
            EdgeNode prevIncoming = null;
            var currentIncoming = toNode.FirstIncomingEdge;

            while (currentIncoming != null)
            {
                if (currentIncoming == edge)
                {
                    if (prevIncoming == null)
                        toNode.FirstIncomingEdge = edge.NextIncoming;
                    else
                        prevIncoming.NextIncoming = edge.NextIncoming;
                    break;
                }
                prevIncoming = currentIncoming;
                currentIncoming = currentIncoming.NextIncoming;
            }
        }

        private void Resize(int newCapacity)
        {
            var newNodes = new GraphNode[newCapacity];
            Array.Copy(nodes, newNodes, count);
            nodes = newNodes;
            capacity = newCapacity;
        }

        public void Clear()
        {
            nodes = new GraphNode[DefaultCapacity];
            count = 0;
            capacity = DefaultCapacity;
        }

        public IEnumerator<N> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
                yield return nodes[i].Value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
