using EntityFramework.Configuration;
using EntityFramework.Context;
using EntityFramework.Handler;
using EntityFramework.JWT;
using EntityFramework.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



//var mongoClient = new MongoClient("mongodb://localhost:27017");
//var dbContextOptions =
//    new DbContextOptionsBuilder<MongoDbContext>().UseMongoDB(mongoClient, "MoviesDB");
//var db = new MongoDbContext(dbContextOptions.Options);

//builder.Services.AddSingleton(db);
//builder.Services.AddSingleton<MongoDbContext>(db);


// Add services to the container.

builder.Services.AddControllers();


var jwtkey = builder.Configuration.GetValue<string>("Jwt:Key");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            //ValidIssuer = "yourdomain.com",
            //ValidAudience = "yourdomain.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtkey))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();


MongoDbSettings mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
builder.Services.AddSingleton(mongoDbSettings);

builder.Services.AddDbContext<MongoDbContext>(options => options.UseMongoDB(mongoDbSettings.ConnectionString, mongoDbSettings.DatabaseName));
builder.Services.AddScoped<IMoviesHandler, MoviesHandler>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    //c.OperationFilter<HeadersFilter>();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CRUD API"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    c.CustomSchemaIds(i => i.FullName);
});




builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue<int>("Port"));

});


var app = builder.Build();


/*
//Create Endpoint
app.MapPost("movies", async (MongoDbContext dbContext, Movies movie) =>
{
    await dbContext.Movies.AddAsync(movie);
    await dbContext.SaveChangesAsync();
});


//Read Endpoint
app.MapGet("movies", async (MongoDbContext dbContext) =>
{
    var movies = await dbContext.Movies.ToListAsync();
    var modifiedMovies = movies.Select(movie => new
    {
        Id = movie.Id.ToString(),
        movie.Title,
        movie.Genre,
        movie.Rating
    });
    return Results.Ok(modifiedMovies);
});
*/



// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
