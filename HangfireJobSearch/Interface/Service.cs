using System.Net;
using System.Security.Authentication;
using HangfireJobSearch.Database;
using HangfireJobSearch.Interface;
using HangfireJobSearch.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace hangfire;

public class Service : IService
{
    private static readonly HttpClient Client = new();
    private static List<string> _newJobsIds = new();
    private static readonly List<string> scrapJobIds = new();
    private static List<CrawledJob> jobDetail = new ();
    
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Service> _logger;
    private readonly IEnumerable<string> alreadyExistingJobsIds;

    public Service(ApplicationDbContext dbContext, ILogger<Service> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Main()
    {
        Console.WriteLine("==================================================================================");
        Console.WriteLine("\t\t\tJOB WEB GHANA WORKER");
        Console.WriteLine("==================================================================================");
        _logger.LogInformation("Starting jobs in ghana worker ===================");
        Console.WriteLine("Starting jobs in ghana worker ===================");
        _logger.LogInformation("Background service executing ============");
        Console.WriteLine("Background service executing ============");
        
        List<CrawledJob> records = await ExtractJobDetails(7);
        var oldJobsIds = await FetchAlreadyExistingJobsIds();
        var filter = FilterOnlyNewJobs(oldJobsIds, scrapJobIds);
       

        if(filter.Count > 0)
        {
            var jobIds = ConvertNewJobIdsToJobsId(_newJobsIds);
            await _dbContext.Staging_Jobs.AddRangeAsync(filter);
            await _dbContext.Staging_JobsId.AddRangeAsync(jobIds);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"\nTotal jobs found: {filter.Count}");
        }
        else
        {
            Console.WriteLine($"\n================No new jobs found!==========================");
        }

    }

    public async Task<List<CrawledJob>> ExtractJobDetails(int numberOfPages)
    {
        
        string baseUrl = "https://jobwebghana.com/jobs/page/";
        HtmlDocument htmlDocument = new HtmlDocument();
        for (int currentPage = 1; currentPage <= numberOfPages; currentPage++)
        {
            Console.WriteLine($"Visiting page ======= {currentPage}");
            Console.WriteLine($"Current URL======> {baseUrl}{currentPage}");
            HttpResponseMessage response = await Client.GetAsync($"{baseUrl}{currentPage}");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                string htmlContent = await response.Content.ReadAsStringAsync();
                htmlDocument.LoadHtml(htmlContent);
                var jobNodes = htmlDocument.DocumentNode.SelectNodes("//li[@class='job']");
                if (jobNodes != null)
                {
                    foreach (var jobNode in jobNodes)
                    {
                        CrawledJob jobDetails = new CrawledJob();
                        var id = jobNode.SelectSingleNode(".//div[@id='details-2']//strong/a")
                            ?.GetAttributeValue("href", "").Trim().Split("/")[4];
                        var url = jobNode.SelectSingleNode(".//div[@id='details-2']//strong/a")?.GetAttributeValue("href", "").Trim();
                        jobDetails.JobId = id; 
                        jobDetails.JobTitle = jobNode.SelectSingleNode(".//div[@id='titlo']//a/strong")?.InnerText.Trim();
                        jobDetails.EmploymentType = jobNode.SelectSingleNode(".//div[@id='type-tag']//span")?.InnerText.Trim();
                        jobDetails.Location = jobNode.SelectSingleNode(".//div[@id='location']//strong")?.NextSibling.InnerText.Trim();
                        jobDetails.JobSourceUrl = url;
                        jobDetails.JobDescription = jobNode.SelectSingleNode(".//div[@id='exc']//div[@class='lista']")?.InnerText.Trim();
                        jobDetails.JobSourceUrl = jobNode.SelectSingleNode(".//div[@id='details-2']//strong/a")?.GetAttributeValue("href", "").Trim();
                        jobDetails.CreatedAt = DateTime.Now;
                        jobDetail.Add(jobDetails);
                        scrapJobIds.Add(id);
                    }
                }
            }
        }
        return jobDetail;
    }
    
    private List<CrawledJob> FilterOnlyNewJobs(IEnumerable<string> oldJobsIds,IEnumerable<string> scrapJobsIds)
    {
        Console.WriteLine("Started filtering only new jobs urls ============");
        _logger.LogInformation("Started performing LINQ operation ============");
        _newJobsIds = scrapJobsIds.Except(oldJobsIds).ToList();
        _logger.LogInformation("Finished performing LINQ operation ============");
        _logger.LogInformation("Started filtering for only newly added jobs urls ============");
        var filteredJobs = jobDetail
            .Where(url => _newJobsIds.Any(id => url.JobSourceUrl.Contains($"{id}")))
            .ToList();
        _logger.LogInformation("Finished filtering job urls ============");
        Console.WriteLine("Done filtering only new jobs urls ============");
        return filteredJobs;
    }

    private async Task<List<string>> FetchAlreadyExistingJobsIds()
    {
        try
        {
            var results = await _dbContext.Staging_JobsId!.ToListAsync();
            var list = results.Select(id => id.JobId).ToList();
            Console.WriteLine("Getting old job ids from database ============");
            _logger.LogInformation("Getting old job ids from database ============");
            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogError(e.Message);
            throw;
        }
    }

    private List<CrawledJobId> ConvertNewJobIdsToJobsId(List<string> newJobIds)
    {
        var records = new List<CrawledJobId>();
        foreach (var item in newJobIds)
        {
            records.Add(new CrawledJobId { JobId = item });
            Console.WriteLine("Mapping new jobs ids to JobsInGhanaId object ============");
            _logger.LogInformation("Mapping new jobs ids to JobsInGhanaIds ============");
        }

        return records;
    }
}