using System;
using System.Collections.Generic;
using CityInfo.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Services;

namespace CityInfo.API.Controllers
{
    /// <summary>
    /// PointsOfInterest resource is a child resource of cities resource
    /// </summary>
    [Route(CitiesController.ROUTE)]
    public class PointsOfInterestController : Controller
    {
        public const string GetPointsOfInterestEndPointName = "GetPointsOfInterest";

        public const int HTTP_STATUS_CODE_INTERNAL_ERROR = 500;

        public const string ROUTE_GET_ALL = "{cityId}/pointsofinterest";
        public const string ROUTE_GET_ONE = "{cityId}/pointsofinterest/{id}";
        public const string ROUTE_POST    = ROUTE_GET_ALL;
        public const string ROUTE_PUT     = ROUTE_GET_ONE;
        public const string ROUTE_PATCH   = ROUTE_GET_ONE;
        public const string ROUTE_DELETE  = ROUTE_GET_ONE;

        private ILogger<PointsOfInterestController> _logger;
        private Services.IMailService _mailService;
        private ICityInfoRepository _cityInfoRepository;

        public PointsOfInterestController(
            ILogger<PointsOfInterestController> logger, // << Dependency injected, see file Startup.cs, method ConfigureServices()
            Services.IMailService mailService,  // << Dependency injected, see file Startup.cs, method ConfigureServices()
            ICityInfoRepository cityInfoRepository   // << Dependency injected, see file Startup.cs, method ConfigureServices()
            )
        {
            // Dependency Injection syntax
            this._logger =  logger;
            // Other possible syntax
            // var logger = HttpContext.RequestServices.GetService(typeof(ILogger<PointsOfInterestController>));

            this._mailService = mailService;
            this._cityInfoRepository = cityInfoRepository;
        }

        //private Models.CityDto FindCity(int cityId) {
        //    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        //    if(city == null)
        //        this._logger.LogInformation($"City with cityId:{cityId} not found");
        //    return city;
        //}

         private Models.CityDto FindCity(int cityId, bool includePointOfInterest = false) {

            // var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            var city = this._cityInfoRepository.GetCity(cityId, includePointOfInterest);
            if(city == null)
            {
                this._logger.LogInformation($"City with cityId:{cityId} not found");
                return null;
            }

            var cityDto = new CityDto()
            {
                Name = city.Name,
                Description = city.Description,
                Id = city.Id
            };
            foreach(var poi in city.PointsOfInterest)
            {
                cityDto.PointsOfInterests.Add(new PointOfInterestDto() { Id = poi.Id, Name = poi.Name, Description = poi.Description });
            }
            return cityDto;
        }

        private PointOfInterestDto FindPointOfInterest(CityDto city, int id)
        {
            var pointOfInterestFromStore = city.PointsOfInterests.FirstOrDefault(p => p.Id == id);
            return pointOfInterestFromStore;
        }

        /// <summary>
        /// http://localhost:1028/api/cities/1/pointsofinterest
        /// </summary>
        /// <returns></returns>
        [HttpGet(ROUTE_GET_ALL)]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try {

                if(this._cityInfoRepository.CityExists(cityId))
                {
                    this._logger.LogInformation($"City with id ${cityId} was not found when calling {nameof(GetPointsOfInterest)}");
                    return NotFound();
                }

                var pointsOfInterestForCity = this._cityInfoRepository.GetPointsOfInterestForCity(cityId);

                // throw new Exception("failing...");
                var city = this.FindCity(cityId);
                if(city == null)
                    return NotFound();
                return Ok(city.PointsOfInterests);
            }
            catch(System.Exception ex)
            {
                this._logger.LogError($"Exception in {nameof(GetPointsOfInterest)} cityId:{cityId}", ex);                
                return StatusCode(HTTP_STATUS_CODE_INTERNAL_ERROR, "An issue happened (Do not expose sensitive info in this text)");
            }
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
            var city = this.FindCity(cityId);
            if(city == null)
                return NotFound();
            var poi = FindPointOfInterest(city, id);
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

            var city = this.FindCity(cityId);
            if(city == null) return NotFound();

            var maxPointOfInterestId = CitiesDataStore.Current.Cities
                .SelectMany(c => c.PointsOfInterests)
                    .Max(p => p.Id);

            var newPointOfinterest = new PointOfInterestDto()
            {
                Id = maxPointOfInterestId+1,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterests.Add(newPointOfinterest);

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

            var city = this.FindCity(cityId);
            if(city == null)
                return NotFound();
            
            var pointOfInterestFromStore = this.FindPointOfInterest(city, id);
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

            var city = this.FindCity(cityId);
            if(city == null)
                return NotFound();
            
            // Load the resource to update from the store
            var pointOfInterestFromStore = this.FindPointOfInterest(city, id);
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

        [HttpDelete(ROUTE_DELETE)]
        public IActionResult DeletePointsOfInterest(int cityId, int id)
        {
            var city = this.FindCity(cityId);
            if(city == null)
                return NotFound();
            
            var pointOfInterestFromStore = this.FindPointOfInterest(city, id);
            if(pointOfInterestFromStore == null)
                 return NotFound();

            city.PointsOfInterests.Remove(pointOfInterestFromStore);

            this._mailService.Send("Point of interest deleted", $"Point of interest name:{pointOfInterestFromStore.Name}, id:{pointOfInterestFromStore.Id} was deleted");

            // Http put must return http code 204 - Ok with No Content to return
            return NoContent();
        }
    }
}

