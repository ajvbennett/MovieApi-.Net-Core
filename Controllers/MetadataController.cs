using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MovieApi.DataAccess;
using MovieApi.Models;

namespace MovieApi.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    [ApiController]
    [Route("metadata")]
    public class MetadataController : ControllerBase
    {
        private Database database = new Database(); // This would be an interface which is injected via the constructor...

        public MetadataController()
        {
        }
       
        [HttpPost()]
        public Metadata Add(Metadata m)
        {
            // Normally would return a 201 status code with the location of the new object but the rules do not stipulate this
            return database.Add(m);
        }

        [HttpGet("movies/stats")]
        public IEnumerable<MovieStats> GetStats()
        {
            var data = database.GetStatsById();

            return data
                .OrderByDescending(d => d.Watches)
                .ThenByDescending(d => d.ReleaseYear);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            // If no metadata has been POSTed for a specified movie, a 404 should be returned.
            // Results are ordered alphabetically by language.

            var movieData = database.GetById(id).ToList();
            if (!movieData.Any())
            {
                return NotFound();
            } 

            return Ok(movieData.OrderBy(d => d.Language));
        }
    }
}
