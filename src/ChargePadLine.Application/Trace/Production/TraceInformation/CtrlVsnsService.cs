using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Trace.TraceInformation;
using Microsoft.EntityFrameworkCore;

namespace ChargePadLine.Application.Trace.Production.TraceInformation
{
    public class CtrlVsnsService : ICtrlVsnsService
    {
        private readonly AppDbContext _context;

        public CtrlVsnsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CtrlVsn> GetVsnAsync(string productCode)
        {
            var vsn = await _context.Set<CtrlVsn>()
                .FirstOrDefaultAsync(v => v.ProductCode == productCode);

            if (vsn == null)
            {
                throw new InvalidOperationException($"未找到产品代码 '{productCode}' 对应的VSN记录");
            }

            return vsn;
        }

        public async Task<CtrlVsn?> TryGetVsnAsync(string productCode)
        {
            return await _context.Set<CtrlVsn>()
                .FirstOrDefaultAsync(v => v.ProductCode == productCode);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}