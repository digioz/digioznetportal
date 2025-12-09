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
    }
}
