using System;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using HangfireJobSearch.Database;
using HangfireJobSearch.Models;

namespace HangfireJobSearch.Interface
{
    public interface IService
    {
        public Task Main();
    }
    
}
