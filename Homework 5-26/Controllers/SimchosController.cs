using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Homework_5_26.Models;
using SimchaFund.Data1;

namespace Homework_5_26.Controllers
{
    public class SimchosController : Controller
    {
        //
        // GET: /Simchos/

        public ActionResult GetSimchos()
        {
            SimchosManager manager = new SimchosManager(Properties.Settings.Default.ConnString);
            SimchosViewModel vm = new SimchosViewModel();
            vm.SimchosWithCount = manager.GetSimchosWithCount();
            vm.TotalContributers = manager.GetTotalContributors();
            vm.Message = (string) TempData["Message"];
            return View(vm);
        }
        [HttpPost]
        public ActionResult AddSimcha(Simcha simcha)
        {
            simcha.Total = 0;
            SimchosManager manager = new SimchosManager(Properties.Settings.Default.ConnString);
            manager.AddSimcha(simcha);
            return RedirectToAction("GetSimchos", "Simchos");
        }

        public ActionResult GetContributions(int simchaid)
        {
            SimchosManager manager = new SimchosManager(Properties.Settings.Default.ConnString);
            ContributionsViewModel vm = new ContributionsViewModel();

            IEnumerable<Contributor> contributors = manager.GetContributors();
            IEnumerable <Contributor> contributorsIncluded = manager.GetSimchaContributions(simchaid);
            vm.Contributions = contributors.Select(c =>
                {
                    bool included = false;
                    int prevAmount = 0;
                    if (contributorsIncluded.FirstOrDefault(contr => contr.Id == c.Id) != null)
                    {
                        included = true;
                        prevAmount = manager.getContributionAmounts(simchaid)
                            .First(cont => cont.Id == c.Id).Amount;
                    }
                    else if (c.AlwaysInclude)
                    {
                        included = true;
                    }
                    return new SimchaContribution
                    {
                        Id = c.Id,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Balance = c.Balance,
                        AlwaysInclude = c.AlwaysInclude,
                        Included = included,
                        Amount = (prevAmount != 0 ? prevAmount : 5)
                    };
                });
            vm.Simcha = manager.GetSimchaById(simchaid);
            return View(vm);
        }

        public ActionResult Update(int simchaId, List<SimchaContribution> includedContributors)
        {
            SimchosManager manager = new SimchosManager(Properties.Settings.Default.ConnString);
            manager.UpdateSimchaContributions(simchaId, includedContributors);
            TempData["message"] = "Updated Contributions for " + manager.GetSimchaById(simchaId).BaalSimcha + " Simcha!";
            return RedirectToAction("GetSimchos", "Simchos");
        }
    }
}
