using LiteDB;
using TagsReportGeneratorApp.Model;

namespace TagsReportGeneratorApp.Repo
{
    public class ZoneRepository: BaseRepository<Zone>
    {

        public ZoneRepository() : base("zone", ConnectionString.Value)
        {

        }

        
        public new bool Update(Zone zoneDb, Zone zone)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Zone>(TableName);
                zoneDb.Name = zone.Name;
                return collection.Update(zoneDb);
            }
        }

        public new bool Delete(Zone zone)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Zone>(TableName);
                return collection.Delete(zone.Uuid);
            }
        }
    }
}
