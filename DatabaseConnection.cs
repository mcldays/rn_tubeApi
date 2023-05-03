using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using rn_tubeApi;
using rn_tubeApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using webapiTest.Models;
using System;

namespace webapiTest
{
    public class DbContext
    {
        MongoClient client { get; set; }
        IMongoDatabase db { get; set; }
        public DbContext()
        {
            if (Environment.GetEnvironmentVariable("RN_TUBE_MODE") == "PRODUCTION") {
                client = new MongoClient("mongodb://rn_tube_db:27017");     // for docker
            }
            else {
                client = new MongoClient("mongodb://localhost:27017");    // for localhost
            }
            db = client.GetDatabase("restube_test");
        }

        public async void ConnectDatabase()
        {
            try
            {
                Directory.CreateDirectory("./videos");
                if(db.GetCollection<BsonDocument>("video") == null)
                {
                    await db.CreateCollectionAsync("users");
                    await db.CreateCollectionAsync("video");
                }
                var videos = db.GetCollection<BsonDocument>("video");  
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task AddVideo(HttpContext context)
        {
            try
            {
                context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
                IFormFile fileData;
                IFormFile filePreview;
                var dict = context.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

                if (context.Request.Form.Files.Count == 1)
                {
                    fileData = context.Request.Form.Files[0];

                    var model = new VideoModel
                    {
                        Link = Path.Combine("./videos/", fileData.Name),
                        Previev = "",
                        Title = dict["name"]

                    };
                    model.Insert(db);
                }
                else
                {
                    filePreview = context.Request.Form.Files[0];
                    fileData = context.Request.Form.Files[1];
                    using (var ms = new MemoryStream())
                    {
                        filePreview.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        string s = Convert.ToBase64String(fileBytes);

                        var model = new VideoModel
                        {
                            Link = Path.Combine("./videos/", fileData.Name),
                            Previev = s,
                            Title = dict["name"]

                        };
                        model.Insert(db);
                    }

                }
                string filePath = Path.Combine("./videos/", fileData.Name);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await fileData.CopyToAsync(fileStream);
                }
                Console.WriteLine(fileData);
            }
            catch (Exception ex)
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

        public Task Registration(UserModel model)
        {
            try
            {
                model.Insert(db);
                return Task.CompletedTask;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> Authorize (LoginModel model) 
        {
            var users = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("Login", model.Login),
            Builders<BsonDocument>.Filter.Eq("Password", model.Password));
            var user = await users.Find(filter).FirstOrDefaultAsync();

            if (user != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.Login) };
                // создаем JWT-токен
                var jwt = new JwtSecurityToken(
                        issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        claims: claims,
                        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                return new JwtSecurityTokenHandler().WriteToken(jwt);
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
