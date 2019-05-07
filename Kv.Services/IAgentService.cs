using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiClient;
using WebApiClient.Attributes;

namespace KvServices
{
    public interface IAgentService: IHttpApi
    {
        [HttpGet("/api/Kv/AddAgent/{url}")]
        Task<bool> AddAsync(string url);
    }
}
