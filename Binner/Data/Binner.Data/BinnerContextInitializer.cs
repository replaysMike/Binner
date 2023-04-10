using Binner.Model.Configuration;
using TypeSupport.Extensions;
using static Binner.Model.SystemDefaults;

namespace Binner.Data
{
    public static class BinnerContextInitializer
    {
        /// <summary>
        /// Initialize the Binner Context
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="environment">Development environment</param>
        /// <param name="passwordHasher">Password hasher for hasing passwords</param>
        public static void Initialize(BinnerContext context, Environments environment, Func<string, string> passwordHasher)
        {
            if (environment == Environments.Development)
                context.Database.EnsureCreated();
            if (context.PartTypes.Any())
            {
                return;
            }

            Console.WriteLine("================================");
            Console.WriteLine("Seeding default database data!");
            Console.WriteLine("================================");
            var transaction = context.Database.BeginTransaction();
            try
            {
                // seed data
                SeedSystemPartTypes(context);
                SeedTestUsers(context, passwordHasher);
                if (context.ChangeTracker.HasChanges())
                {
                    context.SaveChanges();
                    transaction.Commit();
                }
            }
            catch(Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void SeedTestUsers(BinnerContext context, Func<string, string> passwordHasher)
        {
            var record = new Model.User
            {
                Name = "Default Account",
                EmailAddress = "admin",
                PhoneNumber = "555-555-1212",
                PasswordHash = passwordHasher("admin"),
                DateCreatedUtc = DateTime.Now,
                DateModifiedUtc = DateTime.Now,
                IsEmailConfirmed = true,
                IsAdmin = true
            };
            context.Users.Add(record);
        }

        private static void SeedSystemPartTypes(BinnerContext context)
        {
            var defaultPartTypes = typeof(DefaultPartTypes).GetExtendedType();
            foreach (var partType in defaultPartTypes.EnumValues)
            {
                long? parentPartTypeId = null;
                var field = typeof(DefaultPartTypes).GetField(partType.Value);
                if (field?.IsDefined(typeof(Binner.Model.ParentPartTypeAttribute), false) == true)
                {
                    var customAttribute = Attribute.GetCustomAttribute(field, typeof(Binner.Model.ParentPartTypeAttribute)) as Binner.Model.ParentPartTypeAttribute;
                    parentPartTypeId = (long?)customAttribute?.Parent;
                }

                var record = new Model.PartType
                {
                    Name = partType.Value,
                    ParentPartTypeId = parentPartTypeId,
                    DateCreatedUtc = DateTime.UtcNow,
                };
                context.PartTypes.Add(record);
            }
        }
    }
}

