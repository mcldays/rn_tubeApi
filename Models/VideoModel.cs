using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace webapiTest.Models
{
    public class VideoModel
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string VideoData {  get; set; }
        public string Previev { get; set; }

        public async void Insert(IMongoDatabase database)
        {
            var collection = database.GetCollection<VideoModel>("video");
            await collection.InsertOneAsync(this);
        }
    }

}
