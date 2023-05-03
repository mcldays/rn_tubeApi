using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using rn_tubeApi.Interfaces;

namespace webapiTest.Models
{
    public class VideoModel : IInsertDb
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
