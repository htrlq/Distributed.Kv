using System;

namespace KvServices.Context.Model
{
    public class KvValidate
    {
        public Guid Id { get; set; }
        public Guid KvId { get; set; }
        public string Key { get; set; }
        public string Hashcode { get; set; }
        public virtual Kv Kv { get; set; }
    }
}
