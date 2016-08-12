using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data1;

namespace Homework_5_26.Models
{
    public class SimchosViewModel
    {
       public IEnumerable<SimchaWithContrCount> SimchosWithCount{get;set;}
       public int TotalContributers { get; set; }
       public string Message { get; set; }
    }
}