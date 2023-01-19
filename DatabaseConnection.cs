using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using webapiTest.Models;

namespace webapiTest
{
    public class DbContext
    {
        MongoClient client { get; set; }
        IMongoDatabase db { get; set; }
        BsonElement videos { get; set; }
        public DbContext()
        {
            client = new MongoClient("mongodb://localhost:27017");
            db = client.GetDatabase("restube_test");
        }

        public async void ConnectDatabase()
        {
            try
            {
                Directory.CreateDirectory("./videos");
                if(db.GetCollection<BsonDocument>("video") == null)
                {
                    await db.CreateCollectionAsync("video");
                }
                var videos = db.GetCollection<BsonDocument>("video");  

            }
            catch (Exception)
            {
                throw;
            }
        }
        public void AddVideo(VideoModel model)
        {
            try
            {
                model.Insert(db);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Object>> GetAllVideos()
        {
            try
            {
                var videos = db.GetCollection<BsonDocument>("video");
                List<BsonDocument> videoList = await videos.Find(new BsonDocument()).ToListAsync();
                var dotNetObjList = videoList.ConvertAll(BsonTypeMapper.MapToDotNetValue);


                return dotNetObjList;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
