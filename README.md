1- dotnet pack
2- dotnet nuget push bin/Release/Sengkhim.ECP.Infrastructure.1.1.1.nupkg --api-key oy2cuoymnpm52jotkpjaceptvqntgo2enpjxquqamimgeq --source https://api.nuget.org/v3/index.json

Usage: Database connection
builder.Services
   .AddCoreEcpLibrary<InventoryDbContext>(builder.Configuration, "InventoryDb");