using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPictureService
    {
        Picture Get(int id);
        List<Picture> GetAll();
        List<Picture> GetByIds(List<int> ids);
        List<Picture> GetFiltered(string userId = null, int? albumId = null, bool? visible = null, bool? approved = null, bool isAdmin = false);
        int CountByUserId(string userId);
        void Add(Picture picture);
        void Update(Picture picture);
        void Delete(int id);
        void IncrementViews(int id);
        
        // Bulk operations for performance
        int DeleteByUserId(string userId);
        int ReassignByUserId(string fromUserId, string toUserId);
        
        /// <summary>
        /// Searches pictures by term in filename and description fields.
        /// </summary>
        /// <param name="term">Search term</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="totalCount">Output parameter for total matching count</param>
        /// <returns>List of matching pictures</returns>
        List<Picture> Search(string term, int skip, int take, out int totalCount);
    }
}
