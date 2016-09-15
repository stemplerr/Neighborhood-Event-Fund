using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EventFund.Data;

namespace Homework_5_26.Models
{
    public class EventsViewModel
    {
       public IEnumerable<EventWithContributorCount> EventsWithCount{get;set;}
       public int TotalContributers { get; set; }
       public string Message { get; set; }
    }
}