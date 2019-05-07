using System;
using System.ComponentModel.DataAnnotations;

namespace KvServices.Context.Model
{
    public class Kv
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        [MaxLength(1024*4)]
        public byte[] Value { get; set; }
        public virtual KvValidate KvValidate { get; set; }
    }
}
