using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ILinkService
    {
        Link Get(int id);
        List<Link> GetAll();
        List<Link> GetAllVisible();
        void Add(Link link);
        void Update(Link link);
        void Delete(int id);
        void IncrementViews(int id);
        
        /// <summary>
        /// Searches links by term in name, url, and description fields.
        /// </summary>
        /// <param name="term">Search term</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="totalCount">Output parameter for total matching count</param>
        /// <returns>List of matching links</returns>
        List<Link> Search(string term, int skip, int take, out int totalCount);
        
        /// <summary>
        /// Searches links for admin area with filters for visibility, category, and search term.
        /// </summary>
        /// <param name="searchQuery">Search term for name and URL</param>
        /// <param name="visibilityFilter">Visibility filter: "all", "visible", or "notvisible"</param>
        /// <param name="categoryFilter">Optional category ID filter</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="totalCount">Output parameter for total matching count</param>
        /// <returns>List of matching links</returns>
        List<Link> AdminSearch(string searchQuery, string visibilityFilter, int? categoryFilter, int skip, int take, out int totalCount);
    }
}
