using Catalog.Domain.Api.Requests;
using Catalog.Domain.Api.Responses;

namespace Catalog.API.Controllers;

[Controller]
[Route("[controller]")]
public class HomeController : Controller
{
    private readonly IApplicationDbContext _context;

    public HomeController(IApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return new RedirectResult("~/swagger");
    }
    
    [HttpGet]
    [Route("/plates")]
    public async Task<IActionResult> GetPagedPlates([FromQuery] PlatesApiRequest request)
    {
        var plates = await _context.GetPlates(
            request.PageNumber, 
            request.PageSize);
        var totalCount = await _context.GetTotalCount();
        var response = new PlatesApiResponse
        {
            Plates = plates,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
        return Ok(response);
    }
}
