using Microsoft.AspNetCore.Mvc;
using PostgresDbCompare.Models;
using PostgresDbCompare.Services;

namespace PostgresDbCompare.Controllers
{
    public class CompareController : Controller
    {
        private readonly CompareService _compareService;

        public CompareController(CompareService compareService)
        {
            _compareService = compareService;
        }

        public IActionResult Index()
        {
            return View(new DbConnectionModel());
        }

        private string BuildConnectionString(
            string host,
            int port,
            string database,
            string username,
            string password)
        {
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};Pooling=true;";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compare(DbConnectionModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            try
            {
                var sourceConnection = BuildConnectionString(
                    model.SourceHost,
                    model.SourcePort,
                    model.SourceDatabase,
                    model.SourceUsername,
                    model.SourcePassword);

                var targetConnection = BuildConnectionString(
                    model.TargetHost,
                    model.TargetPort,
                    model.TargetDatabase,
                    model.TargetUsername,
                    model.TargetPassword);

                var result = await _compareService.Compare(sourceConnection, targetConnection);

                ViewBag.TargetConnection = targetConnection;

                return View("Result", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Database comparison failed: " + ex.Message);
                return View("Index", model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferObjects(
            string script,
            string connection,
            List<string>? selectedTables,
            List<string>? selectedViews,
            List<string>? selectedFunctions)
        {
            if (string.IsNullOrEmpty(script) || string.IsNullOrEmpty(connection))
                return RedirectToAction("Index");

            try
            {
                var filteredScript = _compareService.FilterScript(
                    script,
                    selectedTables ?? new List<string>(),
                    selectedViews ?? new List<string>(),
                    selectedFunctions ?? new List<string>());

                await _compareService.ExecuteScript(filteredScript, connection);

                TempData["msg"] = "Selected objects transferred successfully";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Migration failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}