using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KvServices.Repository;
using Microsoft.AspNetCore.Mvc;

namespace KvServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KvController : ControllerBase, IKvService
    {
        private IKvServiceRepository KvServiceRepository { get; }

        public KvController(IKvServiceRepository kvServiceRepository)
        {
            KvServiceRepository = kvServiceRepository;
        }

        [HttpPost("Register")]
        public async Task<ResponseModel> RegisterAsync(RegisterModel model)
        {
            try
            {
                var value = Encoding.UTF8.GetBytes(model.Value);

                if (!(await KvServiceRepository.ContainAsync(model.Key)))
                {
                    await KvServiceRepository.AddAsync(model.Key, value);
                }
                else
                {
                    if (!(await KvServiceRepository.ValidateAsync(model.Key, value)))
                        return new ResponseModel("Validate Fail");

                    await KvServiceRepository.UpdateAsync(model.Key, value);
                }

                return new ResponseModel();
            }
            catch(Exception ex)
            {
                return new ResponseModel(ex.Message);
            }
        }

        [HttpGet("Get/{key}")]
        public async Task<string> GetAsync(string key)
        {
            var value = await KvServiceRepository.GetAsync(key);

            return value == null ? "" : Encoding.UTF8.GetString(value);
        }
    }

    public interface IKvService
    {
        Task<ResponseModel> RegisterAsync(RegisterModel model);
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
