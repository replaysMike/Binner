# Nexar (Octopart) Client

This client is generated dynamically from the graphQL query definitions. It uses StrawberryShake to accomplish this.

## Requirements

Install StrawberryShake Tools
```ps
dotnet tool install --global StrawberryShake.Tools
```

Update the tools
```ps
cd Binner\Binner\Library\Nexar.Client
dotnet tool restore
```

Update the GraphQL schema to get the latest Nexar schema
```ps
dotnet graphql update
```

Notes: I had some difficulty with StrawberryShake v12 and created an [issue here](https://github.com/NexarDeveloper/nexar-templates/issues/2) describing how I fixed it.

## Resources

* [Voyager UI visualizes the Nexar data model](https://api.nexar.com/ui/voyager)
* [Nexar Api](https://support.nexar.com/support/home)
