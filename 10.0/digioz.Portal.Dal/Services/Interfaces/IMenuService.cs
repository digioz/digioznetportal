using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMenuService
    {
        Menu Get(int id);
        Menu GetNoTracking(int id);
        List<Menu> GetAll();
        void Add(Menu menu);
        void Update(Menu menu);
        void Delete(int id);
    }
}
