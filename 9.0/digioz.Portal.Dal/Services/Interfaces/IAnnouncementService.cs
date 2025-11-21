using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Announcement Get(int id);
        List<Announcement> GetAll();
        
        /// <summary>
        /// Gets the most recent visible announcements ordered by timestamp descending.
        /// </summary>
        /// <param name="count">Number of announcements to retrieve</param>
        /// <returns>List of visible announcements</returns>
        List<Announcement> GetVisible(int count);
        
        /// <summary>
        /// Gets a paginated list of visible announcements ordered by timestamp descending.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="totalCount">Output parameter for total count of visible announcements</param>
        /// <returns>Paginated list of visible announcements</returns>
        List<Announcement> GetPagedVisible(int pageNumber, int pageSize, out int totalCount);
        
        void Add(Announcement announcement);
        void Update(Announcement announcement);
        void Delete(int id);
    }
}
