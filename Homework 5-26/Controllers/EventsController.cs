using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Homework_5_26.Models;
using EventFund.Data;


namespace Homework_5_26.Controllers
{
    public class EventsController : Controller
    {
        //
        // GET: /Events/

        public ActionResult GetEvents()
        {
            EventRepository repo = new EventRepository(Properties.Settings.Default.ConnString);
            EventsViewModel vm = new EventsViewModel();
            vm.EventsWithCount = repo.GetEventsWithCount();
            vm.TotalContributers = repo.GetTotalContributors();
            vm.Message = (string) TempData["Message"];
            return View(vm);
        }
        [HttpPost]
        public ActionResult AddEvent(Event ev)
        {
            ev.Total = 0;
            EventRepository repo = new EventRepository(Properties.Settings.Default.ConnString);
            repo.AddEvent(ev);
            return RedirectToAction("GetEvents", "Events");
        }

        public ActionResult GetContributions(int eventId)
        {
            //check for anything coming up null
            EventRepository repo = new EventRepository(Properties.Settings.Default.ConnString);
            ContributionsViewModel vm = new ContributionsViewModel();

            IEnumerable<Contributor> contributors = repo.GetContributors();
            IEnumerable <Contributor> contributorsIncluded = repo.GetEventContributors(eventId);
            vm.Contributions = contributors.Select(c =>
                {
                    bool included = false;
                    int prevAmount = 0;
                    if (contributorsIncluded.FirstOrDefault(contr => contr.Id == c.Id) != null)
                    {
                        included = true;
                        prevAmount = repo.getContributionAmounts(eventId)
                            .FirstOrDefault(cont => cont.Id == c.Id).Amount;
                    }
                    else if (c.AlwaysInclude)
                    {
                        included = true;
                    }
                    return new EventContribution
                    {
                        Id = c.Id,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Balance = repo.GetContributorBalance(c.Id),
                        AlwaysInclude = c.AlwaysInclude,
                        Included = included,
                        Amount = (prevAmount != 0 ? prevAmount : 5)
                    };
                });
            vm.Event = repo.GetEventById(eventId);
            return View(vm);
        }

        public ActionResult Update(int eventId, List<EventContribution> includedContributors)
        {
            EventRepository repo = new EventRepository(Properties.Settings.Default.ConnString);
            repo.UpdateEventContributions(eventId, includedContributors);
            TempData["message"] = "Updated Contributions for " + repo.GetEventById(eventId).EventHost + " Event!";
            return RedirectToAction("GetEvents", "Events");
        }
    }
}
