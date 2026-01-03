using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService
{
    public interface ITagService
    {
        Task<PaginatedList<Tag>> GetAllTagsAsync(TagSearchDto dto);
        Task<Tag?> GetTagByIdAsync(int id);
        Task<Tag> AddTagAsync(Tag tag);
        Task<Tag> UpdateTagAsync(Tag tag);
        Task DeleteTagAsync(int id);
    }
}

