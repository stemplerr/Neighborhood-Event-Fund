using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data1;

namespace Homework_5_26.Models
{
    public class HistoryViewModel
    {
        public IEnumerable<Transaction> Transactions { get; set; }
        public string ContributorName { get; set; }
        public int ContributorBalance { get; set; }

    }
}