using MongoDB.Driver;
using rn_tubeApi.Interfaces;
using webapiTest.Models;

namespace rn_tubeApi.Models
{
    public class UserModel : IInsertDb
    {
        public string Login { get; set; } 
        public string  Password { get; set; }
        public string Email { get; set; } 
        public string Name { get; set; }

        public async void Insert(IMongoDatabase database)
        {
            var collection = database.GetCollection<UserModel>("users");
            await collection.InsertOneAsync(this);
        }
    }
}
