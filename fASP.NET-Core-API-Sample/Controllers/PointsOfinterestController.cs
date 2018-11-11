using CityInfo.API.Models;
using Microsoft.AspNetCore.JsonPatch;
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

        public const string ROUTE_GET_ALL = "{cityId}/pointsofinterest";
        public const string ROUTE_GET_ONE = "{cityId}/pointsofinterest/{id}";
        public const string ROUTE_POST    = ROUTE_GET_ALL;
        public const string ROUTE_PUT     = ROUTE_GET_ONE;
        public const string ROUTE_PATCH   = ROUTE_GET_ONE;

        /// <summary>
        /// http://localhost:1028/api/cities/1/pointsofinterest
        /// </summary>
        /// <returns></returns>
        [HttpGet(ROUTE_GET_ALL)]
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
        [HttpGet(ROUTE_GET_ONE, Name = GetPointsOfInterestEndPointName)]
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
        /// Validate a pointOfInterest instance for an POST/Create and PUT/Update.
        /// </summary>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        private bool ValidatePointOfInterest(PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest == null) return false;

            this.CheckPointOfinterestNameAndDescriptionAreNotTheSame(pointOfInterest);

            // Apply the System.ComponentModel.DataAnnotations validation
            // attribute from the type PointOfInterestForCreationDto passed as the body
            if (!this.ModelState.IsValid)
                return false;

            return true;
        }

        private void CheckPointOfinterestNameAndDescriptionAreNotTheSame(PointOfInterestForUpdateDto pointOfInterest)
        {
            // Custom validation rule that will update the ModelState
            // For more validation see FluentValidation library by Jemery Skinner
            if (pointOfInterest.Name == pointOfInterest.Description)
                ModelState.AddModelError("description", "The description field cannot be equal to the name");
        }

        /// <summary>
        /// Create a resource of type PointOfInterest
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        [HttpPost(ROUTE_POST)]
        public IActionResult CreatePointsOfInterest(
            int cityId,
            [FromBody] PointOfInterestForUpdateDto pointOfInterest
            )
        {
            if(!ValidatePointOfInterest(pointOfInterest))
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

        /// <summary>
        /// Update a resource of type PointOfInterest
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="id"></param>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        [HttpPut(ROUTE_PUT)]
        public IActionResult UpdatePointsOfInterest(
            int cityId, 
            int id,
            [FromBody] PointOfInterestForUpdateDto pointOfInterest
            )
        {
            if(!ValidatePointOfInterest(pointOfInterest))
                return BadRequest(this.ModelState); // Passing the ModelState to return the list of error to the client


            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if(city == null)
                return NotFound();
            
            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if(pointOfInterestFromStore == null)
                 return NotFound();

            // All the fields of the resource should be in the pointOfInterest
            // object passed in the body according the http standard (REST).
            // This is a full update of the resource. 
            // For a partial update see HTTP method `PATCH` (JSON PATCH (RFC 6902 https://tools.ietf.org/html/rfc6902)).
            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore.Description = pointOfInterest.Description;
            // Http put must return http code 204 - Ok with No Content to return
            return NoContent();
        }

        /// <summary>
        /// Update a resource partially. The HTTP PATCH method
        /// (RFC 6902 https://tools.ietf.org/html/rfc6902).
        /// Allow to update only a few property of a resource
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="id"></param>
        /// <param name="patchDoc"></param>
        /// <returns></returns>
        [HttpPatch(ROUTE_PATCH)]
        public IActionResult PatchPointsOfInterest(
            int cityId, 
            int id,
            [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc
            )
        {
            if(patchDoc == null) return BadRequest();

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if(city == null)
                return NotFound();
            
            // Load the resource to update from the store
            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if(pointOfInterestFromStore == null)
                 return NotFound();

            // Make a full copy of the resource just loaded from the store
            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description
            };


            // Patch the loaded resource from the store with the resource passed in the body.
            // All properties missing from the body have the default value read from the store
            patchDoc.ApplyTo(pointOfInterestToPatch, this.ModelState); // Pass ModelState so data validation is perform on instance contained in patchDoc
            if(!this.ModelState.IsValid)
                return BadRequest(this.ModelState);


            this.CheckPointOfinterestNameAndDescriptionAreNotTheSame(pointOfInterestToPatch);
            // Force data validation on pointOfInterestToPatch
            this.TryValidateModel(pointOfInterestToPatch);
            if(!this.ModelState.IsValid)
                return BadRequest(this.ModelState);


            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;
            // Http put must return http code 204 - Ok with No Content to return
            return NoContent();
        }
    }
}

