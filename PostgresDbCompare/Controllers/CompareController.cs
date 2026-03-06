using Microsoft.AspNetCore.Mvc;
using PostgresDbCompare.Models;
using PostgresDbCompare.Services;

namespace PostgresDbCompare.Controllers
{
    public class CompareController : Controller
    {
        private readonly CompareService _compareService;
        private readonly ILogger<CompareController> _logger;

        public CompareController(
            CompareService compareService,
            ILogger<CompareController> logger)
        {
            _compareService = compareService;
            _logger = logger;
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

        // ---------------------------
        // COMPARE DATABASES
        // ---------------------------
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

                // Save connections in session for later use (Data Transfer)
                HttpContext.Session.SetString("SourceConnection", sourceConnection);
                HttpContext.Session.SetString("TargetConnection", targetConnection);

                var result = await _compareService.Compare(sourceConnection, targetConnection);

                ViewBag.SourceConnection = sourceConnection;
                ViewBag.TargetConnection = targetConnection;

                return View("Result", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database comparison failed");

                ModelState.AddModelError("", "Database comparison failed. Please check connection details.");

                return View("Index", model);
            }
        }

        // ---------------------------
        // TRANSFER SCHEMA OBJECTS
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferObjects(
            string script,
            string connection,
            List<string>? selectedTables,
            List<string>? selectedViews,
            List<string>? selectedFunctions)
        {
            if (string.IsNullOrWhiteSpace(script) || string.IsNullOrWhiteSpace(connection))
            {
                TempData["error"] = "Invalid migration request.";
                return RedirectToAction("Index");
            }

            try
            {
                var filteredScript = _compareService.FilterScript(
                    script,
                    selectedTables ?? new List<string>(),
                    selectedViews ?? new List<string>(),
                    selectedFunctions ?? new List<string>());

                if (string.IsNullOrWhiteSpace(filteredScript))
                {
                    TempData["error"] = "No objects selected for migration.";
                    return RedirectToAction("Index");
                }

                await _compareService.ExecuteScript(filteredScript, connection);

                TempData["msg"] = "Selected objects transferred successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed");

                TempData["error"] = "Migration failed. Please check logs or database permissions.";
            }

            return RedirectToAction("Index");
        }
    }
}