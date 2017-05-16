using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ProtonAnalytics.Repositories
{
    public class FeatureTogglesRepository : GenericRepository
    {
        // Used to avoid having to add the list of all toggles to every single controller view into ViewBag
        public static FeatureTogglesRepository Instance { get; private set; }

        public FeatureTogglesRepository(ConnectionStringSettings connectionString) : base(connectionString)
        {
            FeatureTogglesRepository.Instance = this;
        }

        // Throws a DB exception of the toggle doesn't exist.
        public bool IsToggleEnabled(string toggleName)
        {
            return (bool)this.ExecuteScalar<object>("SELECT IsEnabled FROM FeatureToggles WHERE ToggleName = @toggleName", new { toggleName = toggleName });
        }
    }
}