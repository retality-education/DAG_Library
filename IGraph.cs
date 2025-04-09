namespace DAG_Library
{
    // Базовый интерфейс для графов
    public interface IGraph<N, L> : IEnumerable<Vertex<N, L>>
        where N : IComparable
        where L : IComparable
    {
        void AddNode(N node);
        void AddEdge(N from, N to, L link);
        void Clear();
        bool Contains(N node);
        void RemoveNode(N node);
        void RemoveEdge(N from, N to);

        int Count { get; }
        bool IsEmpty { get; }
        IEnumerable<Vertex<N, L>> Nodes { get; }
        IEnumerable<Edge<N, L>> Edges { get; }
    }
}
