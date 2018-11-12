using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Entities
{
    public class PointOfInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        
        [MaxLength(200)]
        public string Description2 { get; set; }

        /// <summary>
        /// This property will not be persisted per say
        /// But will be initialized at loading time with the right data coming
        /// from another table
        /// </summary>
        [ForeignKey("CityId")]
        public City City { get; set; } 

        public int CityId { get; set; }
    }
}
