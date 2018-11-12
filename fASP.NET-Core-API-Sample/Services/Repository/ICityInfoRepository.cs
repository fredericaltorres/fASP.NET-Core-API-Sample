using CityInfo.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        bool CityExists(int cityId);

        /// <summary>
        /// here we can use IEnumerable or IQueryable
        /// </summary>
        /// <returns></returns>
        IEnumerable<City> GetCities();
        // IQueryable<City> GetCities();

        City GetCity(int cityId, bool includePointsOfInterest);
        IEnumerable<PointOfInterest> GetPointsOfInterestForCity(int cityId);
        PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId);
        //void AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest);
        //void DeletePointOfInterest(PointOfInterest pointOfInterest);
        //bool Save();
    }
}
