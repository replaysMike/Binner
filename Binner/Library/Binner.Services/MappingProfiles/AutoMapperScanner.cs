using AutoMapper;
using AutoMapper.Internal;
using System.Reflection;

namespace Binner.Services.MappingProfiles
{
    public class AutoMapperScanner
    {
        public AutoMapperScanner()
        {

        }

        public void AddMaps(IMapperConfigurationExpression config) => AddMaps(config, typeof(AutoMapperScanner).Assembly);

        public void AddMaps(IMapperConfigurationExpression config, Assembly assembly)
        {
            var profiles = assembly.GetTypes().Where(x => typeof(Profile).IsAssignableFrom(x));
            foreach (var profile in profiles)
            {
                if (config.Internal().Profiles.Any(x => x.ProfileName == profile.Name))
                {
                    // already mapped, don't add it
                    System.Diagnostics.Debug.WriteLine($"Ignoring profile '{profile.Name}' as another profile with the same name was already added.");
                }
                else
                {
                    config.AddProfile(profile);
                }
            }
        }
    }
}
