using System.Collections.Generic;
using System.Linq;
using Cubes.Core.DataAccess;

namespace Cubes.Core.Web.UIHelpers.Lookups.Providers
{
    public class DatabaseProvidersLookupProvider : ILookupProvider
    {
        private static Dictionary<string, string> knownProviderNames = new Dictionary<string, string>
        {
            { "oracle", "Oracle" },
            { "mssql",  "SQL Server" },
            { "mysql",  "mySQL" },
        };

        public string Name => LookupProviders.DatabaseProviders;

        public Lookup GetLookup()
        {
            var knownProviders = ConnectionManager.KnownProviders;
            return new Lookup
            {
                Name      = this.Name,
                Cacheable = true,
                Items     = knownProviders
                    .Select(pv => new LookupItem
                    {
                        Value   = pv.Key,
                        Display = knownProviderNames[pv.Key]
                    })
                    .OrderBy(i => i.Display)
                    .ToList()
            };
        }
    }
}