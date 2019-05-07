using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KvController : ControllerBase
    {
        private IAgentFactory AgentFactory { get; }
        private IHttpClientFactory HttpClientFactory { get; }

        public KvController(IAgentFactory agentFactory, IHttpClientFactory httpClientFactory)
        {
            AgentFactory = agentFactory;
            HttpClientFactory = httpClientFactory;
        }

        [HttpGet("AddAgent/{url}")]
        public bool AddAgent(string url)
        {
            AgentFactory.Add(WebUtility.UrlDecode(url));

            return true;
        }

        private string GetKey()
        {
            var remoteKey = $"{Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}";

            return remoteKey;
        }

        [HttpPost("Register")]
        public async Task<ResponseModel> RegisterAsync(RegisterModel model)
        {
            var remoteKey = GetKey();
            var baseUrl = AgentFactory.GetAgent(remoteKey);
            var remoteUrl = $"{baseUrl}/api/Kv/Register";

            using (var httpClient = HttpClientFactory.CreateClient())
            {
                var response = await httpClient.PostAsJsonAsync(remoteUrl, model);
                var responseText = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ResponseModel>(responseText);
            }
        }

        [HttpGet("Get/{key}")]
        public async Task<string> GetAsync(string key)
        {
            var remoteKey = GetKey();
            var baseUrl = AgentFactory.GetAgent(remoteKey);
            var remoteUrl = $"{baseUrl}/api/Kv/Get/{key}";

            using (var httpClient = HttpClientFactory.CreateClient())
            {
                var responseText = await httpClient.GetStringAsync(remoteUrl);

                return responseText;
            }
        }
    }

    public class RegisterModel
    {
        [Required]
        public string Key { get; set; }
        [MaxLength(1024 * 4)]
        public string Value { get; set; }
    }

    public class ResponseModel
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ResponseTime => DateTime.Now;

        public ResponseModel()
        {
            Success = true;
        }

        public ResponseModel(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
}
