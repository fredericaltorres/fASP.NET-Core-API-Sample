using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Models
{
    public class CityWithoutPointsOfInterestDto
    {
        public int Id { get; set;}
        public string Name { get; set;}
        public string Description { get; set;}
        

        public static CityWithoutPointsOfInterestDto From(CityDto city)
        {
            return new CityWithoutPointsOfInterestDto()
            {
                Id = city.Id,
                Name = city.Name,
                Description = city.Description
            };
        }
    }
}
