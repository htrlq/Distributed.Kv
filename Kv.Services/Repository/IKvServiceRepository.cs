using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KvServices.Repository
{

    public interface IKvServiceRepository
    {
        Task<bool> ContainAsync(string key);
        Task<bool> ValidateAsync(string key, byte[] value);
        Task AddAsync(string key, byte[] value);
        Task UpdateAsync(string key, byte[] value);
        Task<byte[]> GetAsync(string key); 
    }
}
