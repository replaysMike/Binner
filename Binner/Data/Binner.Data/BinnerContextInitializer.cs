using Binner.Global.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        public static void Initialize(ILogger logger, BinnerContext context, Func<string, string> passwordHasher)
        {
            // context.Database.EnsureCreated();
            if (!context.Users.Any() || !context.PartTypes.Any())
            {
                Console.WriteLine("================================");
                Console.WriteLine("Seeding default database data!");
                Console.WriteLine("================================");
            }

            var transaction = context.Database.BeginTransaction();
            try
            {
                // seed data
                SeedInitialUsers(logger, context, passwordHasher);
                if (context.ChangeTracker.HasChanges())
                    context.SaveChanges();

                AddOrUpdatePartTypes(logger, context);
                if (context.ChangeTracker.HasChanges())
                    context.SaveChanges();

                AddMissingShortIds(logger, context);
                if (context.ChangeTracker.HasChanges())
                    context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void SeedInitialUsers(ILogger logger, BinnerContext context, Func<string, string> passwordHasher)
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
            // if there are no users, add an admin user as admin/admin
            if (!context.Users.Any())
            {
                context.Users.Add(defaultUser);
                logger.LogInformation($"Added default admin user '{defaultUser.EmailAddress}'!");
            }
        }

        private static void AddOrUpdatePartTypes(ILogger logger, BinnerContext context)
        {
            var defaultPartTypes = typeof(DefaultPartTypes).GetExtendedType();
            foreach (var partType in defaultPartTypes.EnumValues)
            {
                long? parentPartTypeId = null;
                string? description = null;
                string? referenceDesignator = null;
                string? keywords = null;
                string? symbolId = null;
                var field = typeof(DefaultPartTypes).GetField(partType.Value);
                if (field?.IsDefined(typeof(Binner.Model.ParentPartTypeAttribute), false) == true)
                {
                    var customAttribute = Attribute.GetCustomAttribute(field, typeof(Binner.Model.ParentPartTypeAttribute)) as Binner.Model.ParentPartTypeAttribute;
                    parentPartTypeId = (long?)customAttribute?.Parent;
                }
                if (field?.IsDefined(typeof(Binner.Model.PartTypeInfoAttribute), false) == true)
                {
                    var info = Attribute.GetCustomAttribute(field, typeof(Binner.Model.PartTypeInfoAttribute)) as Binner.Model.PartTypeInfoAttribute;
                    if (info != null)
                    {
                        description = info.Description;
                        referenceDesignator = info.ReferenceDesignator;
                        keywords = info.Keywords;
                        symbolId = info.SymbolId;
                    }
                }

                var newPartType = new Model.PartType
                {
                    Name = partType.Value,
                    ParentPartTypeId = parentPartTypeId,
                    DateCreatedUtc = DateTime.UtcNow,
                    OrganizationId = 1,
                    UserId = 1,
                    Description = description,
                    ReferenceDesignator = referenceDesignator,
                    Keywords = keywords,
                    SymbolId = symbolId,
                };

                var existingPartType = context.PartTypes
                    .Where(x => x.Name == newPartType.Name && x.ParentPartTypeId == newPartType.ParentPartTypeId)
                    .FirstOrDefault();

                if (existingPartType == null)
                {
                    context.PartTypes.Add(newPartType);
                    logger.LogInformation($"Added new part type '{newPartType.Name}'!");
                }
                else
                {
                    // already exists, update it's info if its not set
                    if (string.IsNullOrEmpty(existingPartType.Description) && !string.IsNullOrEmpty(newPartType.Description))
                        existingPartType.Description = newPartType.Description;
                    if (string.IsNullOrEmpty(existingPartType.ReferenceDesignator) && !string.IsNullOrEmpty(newPartType.ReferenceDesignator))
                        existingPartType.ReferenceDesignator = newPartType.ReferenceDesignator;
                    if (string.IsNullOrEmpty(existingPartType.Keywords) && !string.IsNullOrEmpty(newPartType.Keywords))
                        existingPartType.Keywords = newPartType.Keywords;
                    if (string.IsNullOrEmpty(existingPartType.SymbolId) && !string.IsNullOrEmpty(newPartType.SymbolId))
                        existingPartType.SymbolId = newPartType.SymbolId;
                    if (context.ChangeTracker.HasChanges())
                    {
                        existingPartType.DateModifiedUtc = DateTime.UtcNow;
                        logger.LogInformation($"Updated part type '{existingPartType.Name}'!");
                    }
                }
            }
        }

        private static void AddMissingShortIds(ILogger logger, BinnerContext context)
        {
            var partsWithMissingShortIds = context.Parts.Where(x => x.ShortId == null).ToList();
            foreach(var part in partsWithMissingShortIds)
            {
                // ensure shortid is unique
                var exists = false;
                do
                {
                    part.ShortId = ShortIdGenerator.Generate();
                    exists = context.Parts.Any(x => x.ShortId == part.ShortId);
                } while (exists);
                part.DateModifiedUtc = DateTime.UtcNow;
                logger.LogInformation($"Generated shortId '{part.ShortId}' for part '{part.PartNumber}'!");
            }
        }
    }
}

