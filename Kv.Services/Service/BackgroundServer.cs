using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KvServices.Service
{
    public class AgentServer : BackgroundService
    {
        private IAgentService AgentService { get; }
        private IInformat Informat { get; }

        public AgentServer(IAgentService agentService, IInformat informat)
        {
            AgentService = agentService;
            Informat = informat;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var url = Informat.ServiceUrl;

                try
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        await AgentService.AddAsync(url);
                        await Task.Delay(20);
                    }
                }
                catch
                {
                    await Task.Delay(50);
                }
            }
        }
    }
}
