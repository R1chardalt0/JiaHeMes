using DeviceManage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManage.Services.TagMagService
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(int id);
        Task<Tag> AddTagAsync(Tag tag);
        Task<Tag> UpdateTagAsync(Tag tag);
        Task DeleteTagAsync(int id);
    }
}

