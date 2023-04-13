
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SharpCompress.Common;
using webapiTest;
using webapiTest.Models;


var path = "./videos";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer");  // схема аутентификации - с помощью jwt-токенов
builder.Services.AddAuthorization();

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBoundaryLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L;
});



builder.Services.AddCors(options =>
{     
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:8080",
                                "http://www.contoso.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

DbContext conn = new DbContext();

conn.ConnectDatabase();

 app.MapGet("/video",[Authorize](string path) =>
 {
     GC.Collect();
     //FileStream fileStream = new FileStream(path, FileMode.Open);
     var filestream = System.IO.File.OpenRead(path);
     
     return Results.File(filestream, contentType: "video/mp4",  enableRangeProcessing: true); 
 });

 app.MapGet("/getVideoCount", ()=>{
    return System.IO.Directory.GetFiles(path).Length;
 });

app.MapPost("/AddVideo", [Authorize]async (context) =>
{

    try
    {
        context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
        IFormFile fileData;
        IFormFile filePreview;
        var dict = context.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString()); //Имя файла

        if (context.Request.Form.Files.Count == 1)
        {
            fileData = context.Request.Form.Files[0]; // Файл
            
            var model = new VideoModel
            {
                Link = Path.Combine("./videos/", fileData.Name),
                Previev = "",
                Title = dict["name"]

            };
            conn.AddVideo(model);
        }
        else
        {
            filePreview = context.Request.Form.Files[0];
            fileData = context.Request.Form.Files[1]; // Файл
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
                conn.AddVideo(model);
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

    //var result = context.Request.Form.Keys;

});

app.MapGet("/getAllVideos",async () =>
{
    var videos = await conn.GetAllVideos();
    return videos;
});

app.Run();
