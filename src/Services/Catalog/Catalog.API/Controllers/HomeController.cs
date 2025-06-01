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
            request.PageSize,
            request.SortBy,
            request.OrderBy,
            request.SearchTerm);
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
    
    [HttpPost]
    [Route("/plates/reserve")]
    public async Task<IActionResult> ReservePlate([FromBody] Guid plateId)
    {
        return NoContent();
    }
    
    [HttpPost]
    [Route("/plates/sell")]
    public async Task<IActionResult> SellPlate([FromBody] Guid plateId)
    {
        return NoContent();
    }
    
    [HttpPost]
    [Route("/plates/unreserve")]
    public async Task<IActionResult> UnreservePlate([FromBody] Guid plateId)
    {
        return NoContent();
    }
}
