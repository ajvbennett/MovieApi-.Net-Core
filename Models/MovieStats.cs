namespace MovieApi.Models
{
    public class MovieStats
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public long averageWatchDurationS { get; set; }
        public int Watches { get; set; }
        public int ReleaseYear { get; set; }
    
    }
}