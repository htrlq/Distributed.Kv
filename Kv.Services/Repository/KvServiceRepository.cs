using KvServices.Context;
using KvServices.Context.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace KvServices.Repository
{
    internal class KvServiceRepository : IKvServiceRepository
    {
        private KvServiceContent Content { get; }
        private ICalcHashcode CalcHashcode { get; }

        public KvServiceRepository(KvServiceContent content, ICalcHashcode calcHashcode)
        {
            Content = content;
            CalcHashcode = calcHashcode;
        }

        public async Task<bool> ContainAsync(string key)
        {
            return await Content.KvValidate.AnyAsync(_kvValidate => _kvValidate.Key.Equals(key));
        }

        public async Task<bool> ValidateAsync(string key, byte[] value)
        {
            var hashcode = CalcHashcode.CacleBytes(value);

            return !(await Content.KvValidate.AnyAsync(_kvValidate => _kvValidate.Key.Equals(key) || _kvValidate.Hashcode.Equals(hashcode)));
        }

        public async Task AddAsync(string key, byte[] value)
        {
            using (var transaction = Content.Database.CurrentTransaction ?? await Content.Database.BeginTransactionAsync())
            {
                try
                {
                    var kvModel = new Kv
                    {
                        Id = Guid.NewGuid(),
                        Key = key,
                        Value = value
                    };

                    await Content.Kv.AddAsync(kvModel);

                    var hashcode = CalcHashcode.CacleBytes(value);

                    var kvValidate = new KvValidate
                    {
                        Id = Guid.NewGuid(),
                        KvId = kvModel.Id,
                        Key = key,
                        Hashcode = hashcode
                    };

                    await Content.KvValidate.AddAsync(kvValidate);
                    await Content.SaveChangesAsync();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }

        public async Task UpdateAsync(string key, byte[] value)
        {
            using (var transaction = Content.Database.CurrentTransaction ?? await Content.Database.BeginTransactionAsync())
            {
                try
                {
                    var hashcode = CalcHashcode.CacleBytes(value);

                    var kvValidate = await Content.KvValidate
                        .Include(_kvValidate => _kvValidate.Kv)
                        .FirstOrDefaultAsync(_kvValidate => _kvValidate.Key.Equals(key));

                    kvValidate.Hashcode = hashcode;
                    kvValidate.Kv.Value = value;

                    await Content.SaveChangesAsync();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }

        public async Task<byte[]> GetAsync(string key)
        {
            return (await Content.Kv.AsNoTracking().FirstOrDefaultAsync(_kv=>_kv.Key.Equals(key)))?.Value;
        }
    }
}
