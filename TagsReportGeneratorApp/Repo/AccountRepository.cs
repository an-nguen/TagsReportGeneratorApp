using LiteDB;
using TagsReportGeneratorApp.Model;

namespace TagsReportGeneratorApp.Repo
{
    public class AccountRepository: BaseRepository<Account>
    {
        public AccountRepository() : base("account", ConnectionString.Value)
        {

        }

        public new bool Update(Account objDb, Account obj)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Account>(TableName);
                objDb.Email = obj.Email;
                return collection.Update(objDb);
            }
        }

        public new bool Delete(Account tag)
        {
            using (var db = new LiteDatabase(ConnString))
            {
                var collection = db.GetCollection<Account>(TableName);
                return collection.Delete(tag.Id);
            }
        }
    }
}
