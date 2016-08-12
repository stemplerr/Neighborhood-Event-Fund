using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFund.Data1
{
    public class SimchaContribution : Contributor
    {
        public bool Included { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
