using MongoDB.Driver;

namespace rn_tubeApi.Interfaces
{
    public interface IInsertDb
    {
        public void Insert(IMongoDatabase database);
    }
}
