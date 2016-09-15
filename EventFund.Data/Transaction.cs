using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventFund.Data
{
    public class Transaction
    {
        public TransactionType Type { get; set; }
        public string EventHost { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
    }
}