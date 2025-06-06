using RTCodingExercise.Microservices.Models;
using System.Diagnostics;
using WebMVC.Services;

namespace RTCodingExercise.Microservices.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PlatesApiService _platesApiService;

    public HomeController(
        ILogger<HomeController> logger,
        PlatesApiService platesApiService)
    {
        _logger = logger;
        _platesApiService = platesApiService;
    }

    public async Task<IActionResult> Index(
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Alphabetical",
        string orderBy = "Asc",
        string? searchTerm = null)
    {
        // Parse sortBy and orderBy to enums, fallback to defaults if invalid
        sortBy = sortBy.Equals("Price", StringComparison.OrdinalIgnoreCase) ? "Price" : "Alphabetical";
        orderBy = orderBy.Equals("Desc", StringComparison.OrdinalIgnoreCase) ? "Desc" : "Asc";

        var pagedPlates = await _platesApiService.GetPagedPlatesAsync(
            pageNumber,
            pageSize,
            sortBy,
            orderBy,
            searchTerm);
        return View(pagedPlates);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> Reserve(
        Guid plateId,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Alphabetical",
        string orderBy = "Asc",
        string? searchTerm = null)
    {
        await _platesApiService.ReservePlateAsync(plateId);
        return RedirectToAction("Index", new { pageNumber, pageSize, sortBy, orderBy, searchTerm });
    }

    [HttpPost]
    public async Task<IActionResult> Sell(
        Guid plateId,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Alphabetical",
        string orderBy = "Asc",
        string? searchTerm = null)
    {
        await _platesApiService.SellPlateAsync(plateId);
        return RedirectToAction("Index", new { pageNumber, pageSize, sortBy, orderBy, searchTerm });
    }

    [HttpPost]
    public async Task<IActionResult> Unreserve(
        Guid plateId,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Alphabetical",
        string orderBy = "Asc",
        string? searchTerm = null)
    {
        await _platesApiService.UnreservePlateAsync(plateId);
        return RedirectToAction("Index", new { pageNumber, pageSize, sortBy, orderBy, searchTerm });
    }
}
