using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt => opt.AddPolicy("CorsPolicy",
            policy =>
            {
                policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000");
            }
        ));

builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

// this section of code will create/update database if when there is change
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (System.Exception ex)
{

    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}


app.Run();


//To update dotnet-ef tool, search dotnet-ef online, get the nuget installation command
// and run it in terminal window (change "install" to "update" in command text)

//dotnet-ef commands - view commands: dotnet-ef
/*
Commands:
  database    Commands to manage the database.
  dbcontext   Commands to manage DbContext types.
  migrations  Commands to manage migrations.
*/

/*
To initialize migration
    run command dotnet ef migrations add InitialCreate -s API -p Persistence
    Tool name: dotnet ef
    Command name: migration
    Action name: Add
    Name: InitialCreate
    Switches: -s: Starting project name -p: Persistence project name
*/

/*Before you can run the command, need to install Microsoft.EntityFrameworkCore.Design via Nuget, otherwise error will occurs.
*/
