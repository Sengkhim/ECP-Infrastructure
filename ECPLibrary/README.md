1- dotnet pack
2- dotnet nuget push bin/Release/SK.ECP.Infrastructure.1.0.6.nupkg --api-key oy2esfogp2bbhh2ako4avdr2hepqrnfenjtx3dhxivdh3a --source https://api.nuget.org/v3/index.json

Usage: Database connection
builder.Services
   .AddCoreEcpLibrary<InventoryDbContext>(builder.Configuration, "InventoryDb");