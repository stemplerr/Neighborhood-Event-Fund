using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventFund.Data
{
    public class EventWithContributorCount : Event
    {
        public int Count { get; set; }
    }
}