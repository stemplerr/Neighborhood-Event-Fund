using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EventFund.Data;

namespace Homework_5_26.Models
{
    public class ContributorsViewModel
    {
        public IEnumerable<ContributorWithBalance> Contributors {get;set;}
    }
}