using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using TagsReportGeneratorApp.Model;

namespace TagsReportGeneratorApp.Repo
{
    public class ConfigRepository : BaseRepository<Config>
    {
        public ConfigRepository() : base("config", ConnectionString.Value)
        {
        }

        public new bool Update(Config objDb, Config obj)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Config>(TableName);
                objDb.Value = obj.Value;
                return collection.Update(objDb);
            }
        }

        public new bool Delete(Config obj)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Config>(TableName);
                return collection.Delete(obj.Parameter);
            }
        }
    }
}
