﻿using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    /// <summary>
    /// HTTP Status Code
    /// 200 - Ok
    /// 201 - Ok created
    /// 204 - Ok no content (delete)
    /// 300 - Redirection page moved
    /// 400 - Bad request
    /// 401 - Unauthorized
    /// 403 - Forbidden
    /// 404 - Not found
    /// 409 - Conflict
    /// 500 - Server error - internal error
    /// </summary>
    [Route(CitiesController.ROUTE)]
    public class CitiesController : Controller
    {
        public const string ROUTE = "api/cities";

        
        private ILogger<CitiesController> _logger;
        private ICityInfoRepository _cityInfoRepository;

        public CitiesController(
            ILogger<CitiesController> logger,
            ICityInfoRepository cityInfoRepository
            )
        {
            this._logger =  logger; // Dependency Injection syntax
            this._cityInfoRepository = cityInfoRepository;
        }

        /// <summary>
        /// http://localhost:1028/api/cities
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public IActionResult GetCities()
        {
            // In memory database
            // return Ok(CitiesDataStore.Current.Cities);

            var cityEntities = this._cityInfoRepository.GetCities();
            var results = new List<CityWithoutPointsOfInterestDto>();
            foreach(var cityEntity in cityEntities)
            {
                results.Add(new CityWithoutPointsOfInterestDto() {
                    Id = cityEntity.Id,
                    Description = cityEntity.Description,
                    Name = cityEntity.Name
                });
            }
            return Ok(results);
        }

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

        /// <summary>
        /// http://localhost:1028/api/cities/1
        /// http://localhost:1028/api/cities/1?includePointOfInterest=true
        /// http://localhost:1028/api/cities/1?includePointOfInterest=false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointOfInterest = false)
        {
            var city = this.FindCity(id, includePointOfInterest);
            if(city == null)
                return NotFound();

            if(includePointOfInterest)
                return Ok(city);
            else 
                return Ok(CityWithoutPointsOfInterestDto.From(city));
        }

        // [HttpGet()]
        /*
        public JsonResult GetJsonCities()
        {
            return new JsonResult(
                new List<object>()
                {
                    new { id=1, Name="New York City"},
                    new { id=2, Name="Antwer"},
                }
            );
        }
        */
    }
}
