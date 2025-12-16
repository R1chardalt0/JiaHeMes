using Microsoft.FSharp.Core;
using ChargePadLine.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service
{
    public static class ProdDbContextExtensions
    {
        public static async Task<FSharpResult<TOk, TErr>> ExecuteTransationsAsync<TOk, TErr>(this AppDbContext dbContext, Func<Task<FSharpResult<TOk, TErr>>> func)
        {
            using (var trans = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var r = await func();
                    if (r.IsError)
                    {
                        await trans.RollbackAsync();
                        return r;
                    }
                    else
                    {
                        await trans.CommitAsync();
                        return r;
                    }
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
        }
    }
}