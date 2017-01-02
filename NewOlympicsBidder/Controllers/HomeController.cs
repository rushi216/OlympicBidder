using NewOlympicsBidder.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewOlympicsBidder.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated && User.Identity.Name.Equals(ConfigurationManager.AppSettings["AuthUser"], StringComparison.InvariantCultureIgnoreCase);

            List<Participant> allParticipants;

            Participant currentParticipant;
            List<Participant> remainingParticipants;
            List<Participant> skippedParticipants;

            List<Participant> teamAParticipants;
            List<Participant> teamBParticipants;
            List<Participant> teamCParticipants;
            List<Participant> teamDParticipants;

            using (var dbContext = new OlympicContext())
            {
                allParticipants = dbContext.Participant.ToList();
            }

            currentParticipant = allParticipants.FirstOrDefault(x => x.IsCurrent);

            if (currentParticipant == null)
            {
                ViewBag.ShowResetMessage = true;
            }

            else
            {
                ViewBag.ShowResetMessage = false;
            }

            remainingParticipants = allParticipants.Where(x => x.Status == Status.Remaining).ToList();

            skippedParticipants = allParticipants.Where(x => x.Status == Status.Skipped).ToList();

            teamAParticipants = allParticipants.Where(x => x.TeamId != null && x.TeamId.Value == 1).ToList();
            teamBParticipants = allParticipants.Where(x => x.TeamId != null && x.TeamId.Value == 2).ToList();
            teamCParticipants = allParticipants.Where(x => x.TeamId != null && x.TeamId.Value == 3).ToList();
            teamDParticipants = allParticipants.Where(x => x.TeamId != null && x.TeamId.Value == 4).ToList();


            ViewBag.CurrentParticipant = currentParticipant;

            ViewBag.RemainingParticipants = remainingParticipants;
            ViewBag.SkippedParticipants = skippedParticipants;

            ViewBag.TeamAParticipants = teamAParticipants;
            ViewBag.TeamASpent = teamAParticipants.Sum(x => x.Value);

            ViewBag.TeamBParticipants = teamBParticipants;
            ViewBag.TeamBSpent = teamBParticipants.Sum(x => x.Value);

            ViewBag.TeamCParticipants = teamCParticipants;
            ViewBag.TeamCSpent = teamCParticipants.Sum(x => x.Value);

            ViewBag.TeamDParticipants = teamDParticipants;
            ViewBag.TeamDSpent = teamDParticipants.Sum(x => x.Value);

            ViewBag.AllocatedAmount = 100000000;

            return View();
        }

        public ActionResult Skip(int participantId)
        {
            using (var dbContext = new OlympicContext())
            {
                var currentParticipant = dbContext.Participant.First(x => x.Id == participantId);

                currentParticipant.IsCurrent = false;
                currentParticipant.Status = Status.Skipped;

                dbContext.SaveChanges();

                AssignCurrentParticipant();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Buy(int participantId, long value, int teamId)
        {
            using (var dbContext = new OlympicContext())
            {
                var currentParticipant = dbContext.Participant.First(x => x.Id == participantId);

                currentParticipant.IsCurrent = false;
                currentParticipant.Value = value;
                currentParticipant.TeamId = teamId;
                currentParticipant.Status = Status.Sold;

                dbContext.SaveChanges();

                AssignCurrentParticipant();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Reset()
        {
            using (var dbContext = new OlympicContext())
            {
                var unsoldParticipants = dbContext.Participant.Where(x => x.Status != Status.Sold).ToList();

                unsoldParticipants.ForEach(x =>
                {
                    x.Status = Status.Remaining;
                    x.IsCurrent = false;
                });

                dbContext.SaveChanges();

                AssignCurrentParticipant();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Flush()
        {
            using (var dbContext = new OlympicContext())
            {
                var allParticipants = dbContext.Participant.ToList();

                allParticipants.ForEach(x =>
                {
                    x.Status = Status.Remaining;
                    x.IsCurrent = false;
                    x.Value = 0;
                    x.TeamId = null;
                });

                dbContext.SaveChanges();

                AssignCurrentParticipant();
            }

            return RedirectToAction("Index");
        }

        private void AssignCurrentParticipant()
        {
            using (var dbContext = new OlympicContext())
            {
                var newParticipant = dbContext.Participant.Where(x => x.Status == Status.Remaining).OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                if (newParticipant != null)
                {
                    newParticipant.IsCurrent = true;

                    dbContext.SaveChanges();
                }
            }
        }
    }
}