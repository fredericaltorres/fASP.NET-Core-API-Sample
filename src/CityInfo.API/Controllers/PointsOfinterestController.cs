using CityInfo.API.Models;
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

        public const string GetPointsOfInterestEndPointName = "GetPointsOfInterest";

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
        [HttpGet("{cityId}/pointsofinterest/{id}", Name = GetPointsOfInterestEndPointName)]
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

        /// <summary>
        /// http://localhost:1028/api/cities/1/pointsofinterest/1
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        [HttpPost("{cityId}/pointsofinterest")]
        public IActionResult CreatePointsOfInterest(
            int cityId,
            [FromBody] PointOfInterestForCreationDto pointOfInterest
            )
        {
            if(pointOfInterest == null)
                return BadRequest();

            // Custom validation rule that will update the ModelState
            if(pointOfInterest.Name == pointOfInterest.Description)
                ModelState.AddModelError("description", "The description field cannot be equal to the name");

            // Apply the System.ComponentModel.DataAnnotations validation
            // attribute from the type PointOfInterestForCreationDto passed as the body
            if(!this.ModelState.IsValid)
                return BadRequest(this.ModelState); // Passing the ModelState to return the list of error to the client

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if(city == null) return NotFound();

            var maxPointOfInterestId = CitiesDataStore.Current.Cities
                .SelectMany(c => c.PointsOfInterest)
                    .Max(p => p.Id);

            var newPointOfinterest = new PointOfInterestDto()
            {
                Id = maxPointOfInterestId+1,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(newPointOfinterest);

            // Need to return a http status code 201 with the the location header containing
            // the url to access the created resource
            // Location: http://localhost:1028/api/cities/3/pointsofinterest/34
            var r = CreatedAtRoute(
                GetPointsOfInterestEndPointName, // Name of the route to generate the location link
                new { cityId = cityId, id = newPointOfinterest.Id}, // Parameter to populate the route
                newPointOfinterest // The data to be returned, can be optional
            );
            return r;
        }
    }
}
