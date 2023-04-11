# Binner Data Model

## Configuration

Each entity model has a configuration defined in Binner.Data\Configurations. These are automatically scanned in the assembly. They 
specify relationships, indexes and other special properties for each entity. 

## Adding Migrations

`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.Sqlite --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider Sqlite`
`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.SqlServer --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider SqlServer`
`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.Postgresql --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider Postgresql`
`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.MySql --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider MySql`

then, update the database:
`dotnet ef database update --project Data\Binner.Data`

## Removing Migrations

To remove the last migration that was created (oops!):
`dotnet ef migrations remove --project Data\Binner.Data --startup-project Binner.Web`

## Deploying Databases

Install latest dotnet tools:

`dotnet tool update --global dotnet-ef`
or `dotnet tool install --global dotnet-ef` if not installed.

Generate a migrations bundle, from the folder which migrations exist in:

```ps
cd .\Data\Binner.Data
dotnet ef --startup-project ..\..\Binner.Web\Binner.Web.csproj migrations bundle --verbose
```

*Run on the server*
Run the `efbundle.exe --connection "(connection string here)" --verbose --force` package on the server. Ensure that a configured `appsettings.json` is provided so it knows the connection string to use.

[EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli)
[EF Core Migration Bundles](https://devblogs.microsoft.com/dotnet/introducing-devops-friendly-ef-core-migration-bundles/)
