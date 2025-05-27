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

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
    {
        var pagedPlates = await _platesApiService.GetPagedPlatesAsync(
            pageNumber,
            pageSize);
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
}
