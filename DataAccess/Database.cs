namespace MovieApi.DataAccess
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using MovieApi.Models;

    public class Database
    {
        // dotnet add package --source"https://joshclose.github.io/CsvHelper"
        
        List<Metadata> metadata;
        List<Stats> stats;

        public Database()
        {
            PopulateMetadata();
            PopulateStats();
        }

        private void PopulateMetadata()
        {
            using (var reader = new StreamReader("metadata.csv")) 
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                metadata = csv.GetRecords<Metadata>().ToList();
            }

            return;
        }

        private void PopulateStats()
        {
            using (var reader = new StreamReader("stats.csv"))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                stats = csv.GetRecords<Stats>().ToList();
            }

            return;        
        }

        public IEnumerable<Metadata> GetAll()
        {
            return metadata;
        }

        public IEnumerable<Metadata> GetById(int movieId)
        {
            // Only the latest piece of metadata (highest Id) should be returned where there are
            //      multiple metadata records for a given language.
            // Only metadata with all data fields present should be returned, otherwise it should be
            //      considered invalid.

            var ids = metadata
                .Where(d => d.MovieId == movieId && d.Language != "" && d.Title != "" && d.Duration != "" && d.ReleaseYear > 0)
                .GroupBy(d => new { d.MovieId, d.Language })
                .Select(d => d.Max(g => g.Id));
            
            var data = from i in ids join m in metadata on i equals m.Id
                select m;            

            return data;

        }

        public IEnumerable<MovieStats> GetStatsById()
        {
            // GET /movies/stats
            // Returns the viewing statistics for all movies.
            // ● The movies are ordered by most watched, then by release year with newer releases
            //      being considered more important.
            // ● The data returned only needs to contain information from the supplied csv documents
            //      and does not need to return data provided by the POST metadata endpoint.

            var statsSummary = stats
                .GroupBy(x => x.movieId)
                .Select(g => new { 
                    MovieId = g.Key,
                    DurationMs = g.Sum(x => x.watchDurationMs),
                    Watches = g.Count()
            }).ToList();                        

            var movieSummary = metadata
                .GroupBy(x => x.MovieId)
                .Select(g => new {
                    MovieId = g.Key,
                    Title = g.First().Title,
                    ReleaseYear = g.First().ReleaseYear
                }).ToList();	

            var dataToReturn = from m in movieSummary join s in statsSummary on m.MovieId equals s.MovieId
                                select new MovieStats {
                                    MovieId = m.MovieId,
                                    Title = m.Title,
                                    ReleaseYear = m.ReleaseYear,
                                    Watches = s.Watches,
                                    averageWatchDurationS = (s.DurationMs / s.Watches) / 1000
                                };

            return dataToReturn;
        }

        public Metadata Add(Metadata m)
        {
            m.Id = metadata.Select(md => md.Id).Max() + 1;
            metadata.ToList().Add(m);

            return m;
        }
    }
}