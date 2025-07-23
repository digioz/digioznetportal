using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class CommentConfigService : ICommentConfigService
    {
        private readonly digiozPortalContext _context;

        public CommentConfigService(digiozPortalContext context)
        {
            _context = context;
        }

        public CommentConfig Get(string id)
        {
            return _context.CommentConfigs.Find(id);
        }

        public List<CommentConfig> GetAll()
        {
            return _context.CommentConfigs.ToList();
        }

        public void Add(CommentConfig config)
        {
            _context.CommentConfigs.Add(config);
            _context.SaveChanges();
        }

        public void Update(CommentConfig config)
        {
            _context.CommentConfigs.Update(config);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var config = _context.CommentConfigs.Find(id);
            if (config != null)
            {
                _context.CommentConfigs.Remove(config);
                _context.SaveChanges();
            }
        }
    }
}
