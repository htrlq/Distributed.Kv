namespace Services
{
    public interface IConsistencyRing
    {
        bool Contains(string serverIp);
        void Add(string serverIp);
        void Remove(string serverIp);
        string this[string key] { get; }
    }
}
