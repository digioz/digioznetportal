using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ProfileLogic
    {
        public Profile Get(int id) {
            var ProfileRepo = new ProfileRepo();
            Profile Profile = ProfileRepo.Get(id);

            return Profile;
        }

        public Profile GetProfileByEmail(string email) {
            var profileRepo = new ProfileRepo();
            Profile profile = profileRepo.GetProfileByEmail(email);

            return profile;
        }

        public Profile GetProfileByUserId(string id) {
            var profileRepo = new ProfileRepo();
            Profile profile = profileRepo.GetProfileByUserId(id);

            return profile;
        }

        public List<Profile> GetAll() {
            var ProfileRepo = new ProfileRepo();
            var Profiles = ProfileRepo.GetAll();

            return Profiles;
        }

        public void Add(Profile Profile) {
            var ProfileRepo = new ProfileRepo();
            ProfileRepo.Add(Profile);
        }

        public void Edit(Profile Profile) {
            var ProfileRepo = new ProfileRepo();

            ProfileRepo.Edit(Profile);
        }

        public void Delete(int id) {
            var ProfileRepo = new ProfileRepo();
            var Profile = ProfileRepo.Get(id); // Db.Profiles.Find(id);

            if (Profile != null) {
                ProfileRepo.Delete(Profile);
            }
        }
    }

}
