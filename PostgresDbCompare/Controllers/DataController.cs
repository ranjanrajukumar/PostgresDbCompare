using Microsoft.AspNetCore.Mvc;
using PostgresDbCompare.Services;

namespace PostgresDbCompare.Controllers
{
    public class DataController : Controller
    {
        private readonly DatabaseService _databaseService;

        public DataController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<IActionResult> Transfer()
        {
            var sourceConnection = HttpContext.Session.GetString("SourceConnection");

            if (string.IsNullOrEmpty(sourceConnection))
            {
                TempData["error"] = "Source database connection not found.";
                return RedirectToAction("Index", "Compare");
            }

            var tables = await _databaseService.GetTables(sourceConnection);

            return View(tables);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferData(List<string> selectedTables)
        {
            var sourceConnection = HttpContext.Session.GetString("SourceConnection");
            var targetConnection = HttpContext.Session.GetString("TargetConnection");

            if (string.IsNullOrEmpty(sourceConnection) || string.IsNullOrEmpty(targetConnection))
            {
                TempData["error"] = "Database connections not found.";
                return RedirectToAction("Index", "Compare");
            }

            if (selectedTables == null || !selectedTables.Any())
            {
                TempData["error"] = "No tables selected.";
                return RedirectToAction("Transfer");
            }

            try
            {
                await _databaseService.TransferData(
                    sourceConnection,
                    targetConnection,
                    selectedTables);

                TempData["msg"] = "Table data transferred successfully.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction("Transfer");
        }
    }
}