namespace DAG_Library
{
    // Базовый интерфейс для графов
    public interface IGraph<N, L> : IEnumerable<N> where L : IComparable<L>
    {
        void AddNode(N node);
        void AddLink(N from, N to, L link);
        void Clear();
        bool Contains(N node);
        void RemoveNode(N node);
        void RemoveLink(N from, N to);

        int Count { get; }
        bool IsEmpty { get; }
        IEnumerable<N> Nodes { get; }
        IEnumerable<(N from, N to, L link)> Links { get; }

        bool HasLink(N from, N to);
        L GetLink(N from, N to);
        IEnumerable<N> GetNeighbors(N node);
        IEnumerable<(N to, L link)> GetOutgoingLinks(N node);
        IEnumerable<(N from, L link)> GetIncomingLinks(N node);
    }
}
