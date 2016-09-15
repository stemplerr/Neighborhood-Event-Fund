using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventFund.Data
{
    public class ContributorWithBalance : Contributor
    {
        public int Balance { get; set; }
    }
}