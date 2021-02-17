using System.Collections.Generic;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Bll
{
    public class ConfigLogic : AbstractLogic<Config>, IConfigLogic
    {
        public ConfigLogic(IRepo<Config> repo) : base(repo) { }

        public Dictionary<string, string> GetConfig() {
            var models = base.GetAll();
            var configs = new Dictionary<string, string>();

            foreach (var model in models) {
                configs.Add(model.ConfigKey, model.ConfigValue);
            }

            return configs;
        }

        public string GetEncryptionKey()
		{
            var dict = GetConfig();

            var encryptionKey = dict["SiteEncryptionKey"];
            return encryptionKey;
        }

        public new Config Get(object id)
		{
            var config = base.Get(id);

            if (config.IsEncrypted)
            {
                var encryptionKey = GetEncryptionKey();
                var encryptString = new EncryptString();

                try
                {
                    config.ConfigValue = encryptString.Decrypt(encryptionKey, config.ConfigValue);
                }
                catch { }
            }

            return config;
		}

        public new List<Config> GetAll()
        {
            var configs = base.GetAll();
            var configsDecrypted = new List<Config>();

            foreach(var config in configs)
			{
                if (config.IsEncrypted)
                {
                    var encryptionKey = GetEncryptionKey();
                    var encryptString = new EncryptString();

                    try
                    {
                        config.ConfigValue = encryptString.Decrypt(encryptionKey, config.ConfigValue);
                    }
                    catch { }
                }

                configsDecrypted.Add(config);
			}

            return configsDecrypted;
        }

        public new void Add(Config config)
        {
            if (config.IsEncrypted)
			{
                var encryptionKey = GetEncryptionKey();
                var encryptString = new EncryptString();
                 
                try
                {
                    config.ConfigValue = encryptString.Encrypt(encryptionKey, config.ConfigValue);
                }
                catch { }
            }

            base.Add(config);
        }

        public new void Edit(Config config)
        {
            if (config.IsEncrypted)
            {
                var encryptionKey = GetEncryptionKey();
                var encryptString = new EncryptString();

                try
                {
                    config.ConfigValue = encryptString.Encrypt(encryptionKey, config.ConfigValue);
                }
                catch { }
            }

            base.Edit(config);
        }
    }
}
