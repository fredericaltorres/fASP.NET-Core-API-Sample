using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Entities
{
    /// <summary>
    /// NuGet package: 
    /// Microsoft.entityFrameworkCore.SqlServer
    /// Microsoft.entityFrameworkCore.Tools
    /// 
    /// How to create a db snapshot version
    ///     C\>Add-Migration CityInfoDbInitialCreation
    ///     C\>Update-Database or from code Database.Migrate(); from CityInfoContext()
    ///     Add-Migration CityInfoDb_AddPointOfInterestDescription2
    /// </summary>
    public class CityInfoContext : DbContext
    {
        public CityInfoContext(DbContextOptions<CityInfoContext> options) : base(options)
        {
            // Database.EnsureCreated();
            Database.Migrate();
        }

        /// <summary>
        /// Access to the entities cities
        /// </summary>
        public DbSet<City> Cities { get; set; }

        /// <summary>
        /// Access to all the points of interest
        /// </summary>
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("connectionstring");
        //    base.OnConfiguring(optionsBuilder);
        //}
    }
}
