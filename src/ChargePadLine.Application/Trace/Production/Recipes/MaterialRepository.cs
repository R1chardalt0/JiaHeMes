using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Application.Trace.Production.Recipes
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly AppDbContext _context;

        public MaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Material?> FindWithMaterialCodeAsync(MaterialCode matCode)
        {
            var dbSet = _context.Set<Material>();
            return await dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaterialCode.Value == matCode.Value && !x.IsDeleted);
        }

        public async Task<PaginatedList<Material>> PaginateAsync(int page, int size)
        {
            var dbSet = _context.Set<Material>();
            var query = dbSet.AsQueryable().Where(x => !x.IsDeleted);
            return await query.RetrievePagedListAsync(page, size);
        }
    }
}