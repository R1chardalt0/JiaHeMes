using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManage.Services.TagMagService.Impl
{
    /// <summary>
    /// 点位组（Tag）管理：增删改查 dm_tag
    /// TagDetailDataArray（JSON）由上层页面维护。
    /// </summary>
    public class TagService : ITagService
    {
        private readonly IRepository<Tag> _repo;
        private readonly AppDbContext _db;
        private readonly ILogger<TagService> _logger;

        public TagService(IRepository<Tag> repo, AppDbContext db, ILogger<TagService> logger)
        {
            _repo = repo;
            _db = db;
            _logger = logger;
        }

        public async Task<List<Tag>> GetAllTagsAsync() => await _repo.GetListAsync();

        public async Task<Tag?> GetTagByIdAsync(int id) => await _repo.GetAsync(t => t.Id == id);

        public async Task<Tag> AddTagAsync(Tag tag)
        {
            // 确保导航属性为 null，避免 EF 尝试插入关联实体
            tag.PlcDevice = null;
            tag.RecipeItems = null;
            
            await _repo.InsertAsync(tag);
            await _db.SaveChangesAsync();
            return tag;
        }

        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            var exist = await _repo.GetAsync(t => t.Id == tag.Id);
            if (exist == null) return tag;

            exist.PlcDeviceId = tag.PlcDeviceId;
            exist.Remarks = tag.Remarks;
            exist.TagDetailDataArray = tag.TagDetailDataArray;

            _repo.Update(exist);
            await _db.SaveChangesAsync();
            return exist;
        }

        public async Task DeleteTagAsync(int id)
        {
            var exist = await _repo.GetAsync(t => t.Id == id);
            if (exist != null)
            {
                _repo.Delete(exist);
                await _db.SaveChangesAsync();
            }
        }
    }
}
