using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public interface IAgentFactory
    {
        void Add(string url);
        string GetAgent(string key);
    }

    public class AgentFactory: IAgentFactory
    {
        private volatile object _locked = new object();
        private List<AgentModel> Agents = new List<AgentModel>();
        private TimeSpan CheckTimeSpan = new TimeSpan(0, 0, 0, 300);
        private IConsistencyRing ConsistencyRing { get; }

        public AgentFactory(IConsistencyRing consistencyRing)
        {
            ConsistencyRing = consistencyRing;
        }

        public void Add(string url)
        {
            if (Agents.Any(_agent=>_agent.Url.Equals(url)))
            {
                Refresh(url);
            }
            else
            {
                Agents.Add(new AgentModel
                {
                    Url = url,
                    LastTime = DateTime.Now
                });
            }
        }

        private void Refresh(string url)
        {
            var agent = Agents.FirstOrDefault(_agent => _agent.Url.Equals(url));

            agent.LastTime = DateTime.Now;
        }

        public string GetAgent(string key)
        {
            var queryArray = Agents.ToArray();

            foreach(var server in queryArray)
            {
                if (DateTime.Now.Subtract(server.LastTime) < CheckTimeSpan)
                {
                    if (!ConsistencyRing.Contains(server.Url)) ;
                        ConsistencyRing.Add(server.Url);
                }
                else
                {
                    ConsistencyRing.Remove(server.Url);
                }
            }

            return ConsistencyRing[key];
        }
    }

    internal class ConsistencyRing: IConsistencyRing
    {
        private const int defaultSize = 1024;
        private List<string> serverArray = new List<string>();
        private Dictionary<string, Tuple<int, int>> serverIndexMap = new Dictionary<string, Tuple<int, int>>();
        private List<string> hashArray = new List<string>(defaultSize);

        public ConsistencyRing()
        {
            for (int i = 0; i < hashArray.Capacity; i++)
                hashArray.Add(string.Empty);
        }

        public bool Contains(string serverIp)
        {
            return serverArray.Any(_server=>_server.Equals(serverIp));
        }

        public void Add(string serverIp)
        {
            serverArray.Add(serverIp);
            serverArray.Sort();
            int insertedIndex = serverArray.IndexOf(serverIp);
            if (insertedIndex == 0)
            {
                serverIndexMap[serverIp] = new Tuple<int, int>(0, defaultSize - 1);

                Tuple<int, int> item = serverIndexMap[serverIp];
                for (int itemIndex = item.Item1; itemIndex <= item.Item2; itemIndex++)
                    hashArray[itemIndex] = serverIp;

                return;
            }
            //切分所在前后的index，一半
            int preIndex = 0;
            int nextIndex = 0;
            if (insertedIndex > 0)
                preIndex = insertedIndex - 1;
            else
                preIndex = serverArray.Count - 1;

            if (insertedIndex == serverArray.Count - 1)
                nextIndex = 0;
            else
                nextIndex = insertedIndex + 1;

            Tuple<int, int> preNode = serverIndexMap[serverArray[preIndex]];
            Tuple<int, int> nextNode = serverIndexMap[serverArray[nextIndex]];


            if (preNode.Item1 == nextNode.Item1)
            {
                int splitIndex = (preNode.Item2 - preNode.Item1) / 2 + preNode.Item1;
                serverIndexMap[serverArray[insertedIndex]] = new Tuple<int, int>(splitIndex, hashArray.Count - 1);
                serverIndexMap[serverArray[preIndex]] = new Tuple<int, int>(0, splitIndex - 1);
            }
            else if (preNode.Item1 < nextNode.Item1)
            {
                int splitIndex = (preNode.Item2 - preNode.Item1) / 2 + preNode.Item1;
                serverIndexMap[serverArray[insertedIndex]] = new Tuple<int, int>(splitIndex, preNode.Item2);
                serverIndexMap[serverArray[preIndex]] = new Tuple<int, int>(preNode.Item1, splitIndex - 1);
            }
            else if (preNode.Item1 > nextNode.Item1)
            {
                int splitIndex;

                bool splitInLeft = true;
                int leftSize = defaultSize - preNode.Item1;
                int rightSize = nextNode.Item1;
                if (leftSize > rightSize)
                    splitInLeft = true;

                if (splitInLeft)
                    splitIndex = (defaultSize - preNode.Item1 + nextNode.Item1) / 2 + preNode.Item1;
                else
                    splitIndex = (defaultSize - preNode.Item1 + nextNode.Item1) / 2 + preNode.Item1 - defaultSize;

                if (nextNode.Item1 == 0)
                    serverIndexMap[serverArray[insertedIndex]] = new Tuple<int, int>(splitIndex, defaultSize - 1);
                else
                    serverIndexMap[serverArray[insertedIndex]] = new Tuple<int, int>(splitIndex, nextNode.Item1 - 1);
                serverIndexMap[serverArray[preIndex]] = new Tuple<int, int>(preNode.Item1, splitIndex - 1);
            }

            Tuple<int, int> newNode = serverIndexMap[serverArray[insertedIndex]];
            FillServerIpByNode(newNode, serverIp);
        }

        public void Remove(string serverIp)
        {
            int insertedIndex = serverArray.IndexOf(serverIp);
            int preIndex = 0;
            if (insertedIndex > 0)
                preIndex = insertedIndex - 1;
            else
                preIndex = serverArray.Count - 1;

            Tuple<int, int> preNode = serverIndexMap[serverArray[preIndex]];
            Tuple<int, int> curNode = serverIndexMap[serverArray[insertedIndex]];
            serverIndexMap[serverArray[preIndex]] = new Tuple<int, int>(preNode.Item1, curNode.Item2);

            string newServerIp = serverArray[preIndex];
            preNode = serverIndexMap[newServerIp];

            FillServerIpByNode(preNode, newServerIp);

            serverArray.Remove(serverIp);
        }

        private void FillServerIpByNode(Tuple<int, int> node, string serverIp)
        {
            if (node.Item2 > node.Item1)
            {
                for (int i = node.Item1; i <= node.Item2; i++)
                    hashArray[i] = serverIp;
            }
            else
            {
                for (int i = node.Item1; i < defaultSize; i++)
                    hashArray[i] = serverIp;
                for (int i = 0; i <= node.Item2; i++)
                    hashArray[i] = serverIp;
            }
        }

        public string this[string key]
        {
            get
            {
                EnsureNotNull(key);

                int hashIndex = MapString2Int(key);
                string obj = hashArray[hashIndex];
                if (obj == null)
                    throw new Exception("Key不存在");

                return obj;
            }
        }

        private int MapString2Int(string key)
        {
            int hashIndex = 0;
            char[] keyAry = key.ToCharArray();
            foreach (var c in keyAry)
                hashIndex += (int)c;

            hashIndex = (31 * hashIndex + 2) % hashArray.Capacity;

            return hashIndex;
        }

        private static void EnsureNotNull(string key)
        {
            if (key == null || key.Trim().Length == 0)
                throw new Exception("Key不能为空");
        }
    }

    public class AgentModel
    {
        private string _url;

        public string Url { get; set; }

        public DateTime LastTime { get; set; }
    }
}
