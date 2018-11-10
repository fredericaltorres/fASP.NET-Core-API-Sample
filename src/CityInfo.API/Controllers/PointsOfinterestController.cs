using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    /// <summary>
    /// PointsOfInterest resource is a child resource of cities resource
    /// </summary>
    [Route(CitiesController.ROUTE)]
    public class PointsOfInterestController : Controller
    {
        /// <summary>
        /// http://localhost:1028/api/cities/1/pointsofinterest
        /// </summary>
        /// <returns></returns>
        [HttpGet("{cityId}/pointsofinterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if(city == null)
                return NotFound();
            return Ok(city.PointsOfInterest);
        }

        /// <summary>
        /// http://localhost:1028/api/cities/1/pointsofinterest/1
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{cityId}/pointsofinterest/{id}")]
        public IActionResult GetPointsOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if(city == null)
                return NotFound();
            var poi = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if(poi == null)
                return NotFound();
            return Ok(poi);
        }
    }
}
