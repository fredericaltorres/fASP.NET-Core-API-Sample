using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// http://localhost:1028/api/cities
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public IActionResult GetCities()
        {
            return Ok(CitiesDataStore.Current.Cities);
        }

        /// <summary>
        /// http://localhost:1028/api/cities/1
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult GetCity(int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);
            if(city == null)
                return NotFound();
            return Ok(city);
        }

        // [HttpGet()]
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
    }
}
