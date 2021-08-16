using LiteDB;
using TagsReportGeneratorApp.Model;

namespace TagsReportGeneratorApp.Repo
{
    public class TagRepository: BaseRepository<Tag>
    {

        public TagRepository() : base("tag", ConnectionString.Value)
        {

        }

        public new bool Update(Tag tagDb, Tag tag)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Tag>(TableName);
                tagDb.Name = tag.Name;
                return collection.Update(tag);
            }
        }

        public new bool Delete(Tag tag)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Tag>(TableName);
                return collection.Delete(tag.Uuid);
            }
        }


    }
}
