using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Service;

namespace ChargePadLine.Application.Trace.Production.Recipes
{
    public interface IMaterialRepository 
    {
        public Task<Material?> FindWithMaterialCodeAsync(MaterialCode matCode);
        public Task<PaginatedList<Material>> PaginateAsync(int page, int size);
    }
}
