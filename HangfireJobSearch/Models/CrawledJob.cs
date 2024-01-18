namespace HangfireJobSearch.Models
{
    public class CrawledJob
    {
        public Guid CrawledJobId { get; set; }
        public string JobId { get; set; }
        public string? JobTitle { get; set; }
        public string JobSourceUrl { get; set; }
        public string? JobDescription { get; set; }

        public string? EmploymentType { get; set; }
       
        public string? Location { get; set; }
       
       
  
       
        public DateTime? CreatedAt { get; set; }

        public override string ToString()
        {
            return $"{nameof(CrawledJobId)}: {CrawledJobId}, {nameof(JobId)}: {JobId}, {nameof(JobTitle)}: {JobTitle}, {nameof(JobSourceUrl)}: {JobSourceUrl}, {nameof(JobDescription)}: {JobDescription}, {nameof(EmploymentType)}: {EmploymentType}, {nameof(Location)}: {Location},  {nameof(CreatedAt)}: {CreatedAt}";
        }
    }
}
