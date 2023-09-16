using CommBank.Models;
using CommBank.Services;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data"); //for seeeding the JSON Data 

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("Secrets.json");

var mongoClient = new MongoClient(builder.Configuration.GetConnectionString("CommBank"));
var mongoDatabase = mongoClient.GetDatabase("CommBank");

IAccountsService accountsService = new AccountsService(mongoDatabase);
IAuthService authService = new AuthService(mongoDatabase);
IGoalsService goalsService = new GoalsService(mongoDatabase);
ITagsService tagsService = new TagsService(mongoDatabase);
ITransactionsService transactionsService = new TransactionsService(mongoDatabase);
IUsersService usersService = new UsersService(mongoDatabase);

builder.Services.AddSingleton(accountsService);
builder.Services.AddSingleton(authService);
builder.Services.AddSingleton(goalsService);
builder.Services.AddSingleton(tagsService);
builder.Services.AddSingleton(transactionsService);
builder.Services.AddSingleton(usersService);

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder
   .AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


// Seed each collection
SeedCollection<Account>("Accounts", "Accounts.json");
SeedCollection<Goal>("Goals", "Goals.json");
SeedCollection<CommBank.Models.Tag>("Tags", "Tags.json"); // Use your Tag model, not MongoDB.Driver.Tag
SeedCollection<Transaction>("Transactions", "Transactions.json");
SeedCollection<User>("Users", "Users.json");


void SeedCollection<T>(string collectionName, string jsonFileName)
{
    var collection = mongoDatabase.GetCollection<T>(collectionName);
    var jsonFilePath = Path.Combine(dataDirectory, jsonFileName);

    if (File.Exists(jsonFilePath))
    {
        var jsonData = File.ReadAllText(jsonFilePath);
        var documents = JsonConvert.DeserializeObject<List<T>>(jsonData);

        // Insert the data into the collection
        collection.InsertMany(documents);
        Console.WriteLine($"Seeded {collectionName} with {documents.Count} documents.");
    }
    else
    {
        Console.WriteLine($"JSON file not found: {jsonFileName}");
    }
}

app.MapControllers();

app.Run();

