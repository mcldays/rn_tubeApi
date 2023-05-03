
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using rn_tubeApi;
using rn_tubeApi.Models;
using SharpCompress.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using webapiTest;
using webapiTest.Models;


var path = "./videos";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

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
    app.UseDeveloperExceptionPage();
    
    
}
app.UseSwagger();
app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                c.RoutePrefix = string.Empty;
            });
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

DbContext conn = new DbContext();

conn.ConnectDatabase();

 app.MapGet("/video", (string path) =>
 {
     GC.Collect();
     var filestream = System.IO.File.OpenRead(path);
     return Results.File(filestream, contentType: "video/mp4",  enableRangeProcessing: true); 
 });

 app.MapGet("/getVideoCount", [Authorize] ()=>{
    return System.IO.Directory.GetFiles(path).Length;
 });

app.Map("/login", (UserModel user) =>
{
    var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Login) };
    // создаем JWT-токен
    var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

    return new JwtSecurityTokenHandler().WriteToken(jwt);
});

app.Map("/reg", (UserModel user) =>
{
   return conn.Registration(user);
});


app.MapPost("/AddVideo", conn.AddVideo).RequireAuthorization();

app.MapGet("/getAllVideos", [Authorize] async () =>
{
    var videos = await conn.GetAllVideos();
    return videos;
});

app.Map("/aut", (LoginModel model) =>
{
    return conn.Authorize(model);
});

app.Run();
