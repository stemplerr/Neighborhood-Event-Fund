using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data1;

namespace Homework_5_26.Models
{
    public class ContributionsViewModel
    {
        public Simcha Simcha { get; set; }
        public IEnumerable<SimchaContribution> Contributions { get; set; }
    }
}