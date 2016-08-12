using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFund.Data1
{
   public class Transaction
    {
       public TransactionType Type { get; set; }
       public string SimchaName { get; set; }
       public int Amount { get; set; }
       public DateTime Date { get; set; }
    }
}
