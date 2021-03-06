﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Homework_5_26.Models;
using EventFund.Data;

namespace Homework_5_26.Controllers
{
    public class ContributorsController : Controller
    {
        //
        // GET: /Contributors/

        public ActionResult GetContributors(string query)
        {
            ContributorsViewModel vm = new ContributorsViewModel();
            EventRepository manager = new EventRepository(Properties.Settings.Default.ConnString);
            if (string.IsNullOrEmpty(query))
                vm.Contributors = manager.GetContributors();
            else
                vm.Contributors = manager.GetContributors(query);
            return View(vm);
        }
        
        [HttpPost]
        public ActionResult AddContributor(Contributor contributor)
        {
            if (contributor.FirstName != null &&
                contributor.LastName != null &&
                contributor.CellNumber != null)
            {
                ContributorsViewModel vm = new ContributorsViewModel();
                EventRepository manager = new EventRepository(Properties.Settings.Default.ConnString);
                manager.AddContributor(contributor);
            }
            return RedirectToAction("GetContributors", "Contributors");
        }
        [HttpPost]
        public ActionResult EditContributor(Contributor contributor)
        {
            EventRepository manager = new EventRepository(Properties.Settings.Default.ConnString);
            manager.EditContributor(contributor);
            return RedirectToAction("GetContributors", "Contributors");
        }

        [HttpPost]
        public ActionResult Deposit(int id, int amount)
        {
            EventRepository manager = new EventRepository(Properties.Settings.Default.ConnString);
            int newAmount = manager.Deposit(id, amount, DateTime.Now);
            return Json(newAmount, JsonRequestBehavior.AllowGet);
        }

        public ActionResult History(int contributorId)
        {
            EventRepository manager = new EventRepository(Properties.Settings.Default.ConnString);
            HistoryViewModel vm = new HistoryViewModel();
            Contributor cont = manager.GetContributors().First(c => c.Id == contributorId);
            vm.ContributorName = cont.FirstName + " " + cont.LastName;
            vm.ContributorBalance = 0;
            //vm.ContributorBalance = cont.Balance;
            vm.Transactions = manager.GetContributorHistory(contributorId);
            return View(vm);
        }

    }
}
