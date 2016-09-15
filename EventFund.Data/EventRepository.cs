using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;

namespace EventFund.Data
{
    public class EventRepository
    {
        private string _connString;
        public EventRepository(string connString)
        {
            _connString = connString;
        }
        //---------------------------Events Page --------------------------------------//

        public IEnumerable<EventWithContributorCount> GetEventsWithCount()
        {
            var events = new List<EventWithContributorCount>();
            using (var ctx = new EventDbDataContext(_connString))
            {
                foreach (Event e in ctx.Events)
                {
                    IEnumerable<Contributor> contributors = GetEventContributors(e.Id);
                    events.Add(new EventWithContributorCount
                        {
                            Count = contributors.Count(),
                            EventHost = e.EventHost,
                            Date = e.Date,
                            Total = GetEventTotal(e.Id),
                            Id = e.Id
                        });
                }
            }
            return events;
        }

        private int GetEventTotal(int eventId)
        {
            int total = 0;
            using (var ctx = new EventDbDataContext(_connString))
            {
                List<Contribution> contr = ctx.Contributions.Where(c => c.EventId == eventId).ToList();
                
                foreach(Contribution c in contr)
                {
                    total += c.ContributionAmount;
                }
            }
            return total;
        }

        public int GetTotalContributors()
        {
            using (EventDbDataContext ctx = new EventDbDataContext(_connString))
            {
                return ctx.Contributors.Count();
            }
        }

        public void AddEvent(Event ev)
        {
            if (ev.EventHost == null
                 || ev.Date == null)
            {
                return;
            }
            using (var ctx = new EventDbDataContext(_connString))
            {
                ctx.Events.InsertOnSubmit(new Event
                    {
                        EventHost = ev.EventHost,
                        Date = ev.Date,
                        Total = 0
                    });
                ctx.SubmitChanges();
            }
        }
        //----------------------- List of Contributions for each Event ------------------

        public Event GetEventById(int eventId)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                return ctx.Events.First(e => e.Id == eventId);
            }
        }

        public IEnumerable<Contributor> GetEventContributors(int eventId)
        {
            List<Contributor> contributors = new List<Contributor>();

            using (var ctx = new EventDbDataContext(_connString))
            {
                List<Contribution> contributions = ctx.Contributions.Where(c => c.EventId == eventId).ToList();
                foreach (Contributor c in ctx.Contributors)
                {
                    if (contributions.FirstOrDefault(co => co.ContributorId == c.Id) != null)
                    {
                        contributors.Add(c);
                    }
                }
            }
            return contributors;
        }

        public void UpdateEventContributions(int eventId, List<EventContribution> includedContributors)
        {
            List<EventContribution> prevContributions = getContributionAmounts(eventId);
            foreach (EventContribution contribution in includedContributors)
            {
                EventContribution prevContribution = prevContributions.FirstOrDefault(c => c.Id == contribution.Id);
                if (prevContribution != null)
                {
                    if (contribution.Amount != prevContribution.Amount)
                    {
                        UpdateContributionAmount(contribution.Id, eventId, contribution.Amount);
                    }
                    if (!contribution.Included)
                    {
                        RemoveContribution(contribution.Id, eventId);
                    }
                }
                else if (prevContribution == null && contribution.Included)
                {
                    AddEventContribution(contribution.Id, eventId, contribution.Amount);
                }
            }
        }

        public List<EventContribution> getContributionAmounts(int eventId)
        {
            List<EventContribution> result = new List<EventContribution>();
            using (var ctx = new EventDbDataContext(_connString))
            {
                List<Contribution> contributions = ctx.Contributions.Where(c => c.EventId == eventId).ToList();
                foreach (Contribution c in contributions)
                {
                    if (c.Id != 0)
                    {
                        result.Add(new EventContribution
                            {
                                Id = c.ContributorId,
                                ContributionId = c.Id,
                                Amount = c.ContributionAmount,
                                Date = c.Date
                            });
                    }
                }
            }
            return result;
        }

        public void UpdateContributionAmount(int contributorId, int eventId, int amount)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                var contr = new Contribution
                 {
                     ContributionAmount = amount,
                     EventId = eventId,
                     ContributorId = contributorId
                 };
                ctx.Contributions.Attach(contr);
                ctx.Refresh(RefreshMode.KeepCurrentValues, contr);
                ctx.SubmitChanges();

            }
        }
        public void AddEventContribution(int contributorId, int eventId, int amount)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                ctx.Contributions.InsertOnSubmit(new Contribution
                    {
                        ContributorId = contributorId,
                        EventId = eventId,
                        Date = DateTime.Now,
                        ContributionAmount = amount
                    });
                ctx.SubmitChanges();
            }
        }
        public void RemoveContribution(int contrId, int eventId)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                ctx.Contributions.DeleteOnSubmit(ctx.Contributions.First(c => c.ContributorId == contrId && c.EventId == eventId));
                ctx.SubmitChanges();
            }
        }
        //------------------------- Contributors   ---------------------------

        public IEnumerable<ContributorWithBalance> GetContributors()
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                List<ContributorWithBalance> contrs = new List<ContributorWithBalance>();
                foreach(Contributor c in ctx.Contributors)
                {
                    contrs.Add(new ContributorWithBalance
                        {
                            Id = c.Id,
                            AlwaysInclude = c.AlwaysInclude,
                            CellNumber = c.CellNumber,
                            FirstName = c.FirstName,
                            LastName = c.LastName,                           
                            Balance = GetContributorBalance(c.Id)
                        });
                }
                return contrs;
            }
        }
        public IEnumerable<ContributorWithBalance> GetContributors(string query)
        {
            query = query.ToLower();
            var contrs = GetContributors();
            return contrs.Where(c => c.FirstName.ToLower().Contains(query) || c.LastName.ToLower().Contains(query));            
        }

        public void AddContributor(Contributor c)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                ctx.Contributors.InsertOnSubmit(c);
                ctx.SubmitChanges();
            }
        }

        public void EditContributor(Contributor c)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                ctx.Contributors.Attach(c);
                ctx.Refresh(RefreshMode.KeepCurrentValues, c);
                ctx.SubmitChanges();
            }
        }

        public int GetContributorBalance(int id)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                var depositsTotal = 0;
                foreach(Deposit d in ctx.Deposits)
                {
                    if (d.ContributorId == id)
                    {
                        depositsTotal += d.Amount;
                    }
                } 
                var contributionsTotal = 0;
                foreach (Contribution d in ctx.Contributions)
                {
                    if (d.ContributorId == id)
                    {
                        contributionsTotal += d.ContributionAmount;
                    }
                }
               return depositsTotal - contributionsTotal;
            }
        }

        public IEnumerable<Transaction> GetContributorHistory(int contributorId)
        {
            List<Transaction> transactions = new List<Transaction>();
            using (var ctx = new EventDbDataContext(_connString))
            {
                List<Deposit> deposits = ctx.Deposits.Where(d => d.ContributorId == contributorId).ToList();
                transactions = deposits.Select(d => new Transaction
                    {
                        Type = TransactionType.Deposit,
                        Date = d.Date,
                        Amount = d.Amount
                    }).ToList();
                 List<Contribution> contributions = ctx.Contributions.Where(c => c.ContributorId == contributorId).ToList();
               transactions.AddRange( contributions.Select(c => new Transaction
                    {
                        Type = TransactionType.Contribution,
                        EventHost = ctx.Events.First(e => e.Id == c.EventId).EventHost,
                        Date = c.Date,
                        Amount = c.ContributionAmount
                    }).ToList());
            }
            return transactions.OrderBy(t => t.Date);
        }

        public int Deposit(int id, int amount, DateTime date)
        {
            using (var ctx = new EventDbDataContext(_connString))
            {
                ctx.Deposits.InsertOnSubmit(new Deposit
                    {
                        ContributorId = id,
                        Date = date,
                        Amount = amount
                    });
                ctx.SubmitChanges();
                return GetContributorBalance(id);
            }
        }    
     }
}