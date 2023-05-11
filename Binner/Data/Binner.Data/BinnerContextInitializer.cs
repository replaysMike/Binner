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
        /// <param name="passwordHasher">Password hasher for hashing passwords</param>
        public static void Initialize(BinnerContext context, Func<string, string> passwordHasher)
        {
            // context.Database.EnsureCreated();
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
                SeedInitialUsers(context, passwordHasher);
                if (context.ChangeTracker.HasChanges())
                {
                    context.SaveChanges();
                }
                SeedSystemPartTypes(context);
                if (context.ChangeTracker.HasChanges())
                {
                    context.SaveChanges();
                }
                transaction.Commit();
            }
            catch(Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void SeedInitialUsers(BinnerContext context, Func<string, string> passwordHasher)
        {
            var defaultUser = new Data.Model.User
            {
                Name = "Admin",
                EmailAddress = "admin",
                PasswordHash = passwordHasher("admin"),
                DateCreatedUtc = DateTime.UtcNow,
                DateModifiedUtc = DateTime.UtcNow,
                IsEmailConfirmed = true,
                IsAdmin = true,
                OrganizationId = 1
            };
            context.Users.Add(defaultUser);
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
                    OrganizationId = 1,
                    UserId = 1
                };
                context.PartTypes.Add(record);
            }
        }
    }
}

