# Binner Data Stub

This application is used only for generating migrations. It contains connection strings for each database (default values), and is not used for user runtime.

Migrations will be applied against the database the Binner application is configured for, using it's `StorageProviderConfiguration`.

## Generating Migrations

`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.Sqlite --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider Sqlite`
`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.SqlServer --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider SqlServer`
`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.Postgresql --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider Postgresql`
`dotnet ef migrations add InitialCreate --project Data\Binner.Data.Migrations.MySql --startup-project Data\Binner.Data.Stub --context BinnerContext -v -- --provider MySql`


## Resources

* [Using one context with multiple providers](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli)
* [Two & Three project setups for multiple migration projects](https://github.com/dotnet/EntityFramework.Docs/tree/main/samples/core/Schemas)
