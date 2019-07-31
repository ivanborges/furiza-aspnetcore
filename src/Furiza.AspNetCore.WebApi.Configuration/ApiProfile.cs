using System.Collections.Generic;
using System.Reflection;

namespace Furiza.AspNetCore.WebApi.Configuration
{
    public class ApiProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultVersion { get; set; }
        public IDictionary<string, string> VersioningDescriptions { get; set; }
        public string DefaultCultureInfo { get; set; }
        public ICollection<Assembly> AutomapperAssemblies { get; } = new List<Assembly>();
    }
}