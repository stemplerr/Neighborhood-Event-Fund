using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventFund.Data
{
    public class EventContribution : Contributor
    {
        public int ContributionId { get; set; }
        public bool Included { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public int Balance { get; set; }
    }
}