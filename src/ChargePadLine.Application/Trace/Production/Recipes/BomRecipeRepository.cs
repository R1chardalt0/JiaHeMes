using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
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
    public class BomRecipeRepository : IBomRecipeRepository
    {
        private readonly IRepository<BomRecipe> _repository;
        private readonly AppDbContext _context;

        public BomRecipeRepository(IRepository<BomRecipe> repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<BomRecipeItem?> FindBomItemByCodeWithNoTrackAsync(string bomItemCode)
        {
            var dbSet = _context.Set<BomRecipeItem>();
            return await dbSet
                .AsNoTracking()
                .Include(x => x.Bom)
                .Where(x => x.BomItemCode.Value == bomItemCode)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedList<BomRecipe>> PaginateAsync(int page, int size)
        {
            var dbSet = _context.Set<BomRecipe>();
            var query = dbSet.AsQueryable();
            return await query.RetrievePagedListAsync(page, size);
        }
    }
}
