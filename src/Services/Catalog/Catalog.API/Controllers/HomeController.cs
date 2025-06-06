using Catalog.Domain.Api.Requests;
using Catalog.Domain.Api.Responses;
using Catalog.Domain.Enums;

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
        var plateStatuses = await _context.GetPlateStatuses(plates.Select(plate => plate.Id).ToList());
        var plateDtos = plates.Select(plate => new PlateDto
        {
            Id = plate.Id,
            Registration = plate.Registration,
            PurchasePrice = plate.PurchasePrice,
            SalePrice = plate.SalePrice,
            Letters = plate.Letters,
            Numbers = plate.Numbers,
            Status = plateStatuses.FirstOrDefault(plateStatus => plateStatus.Id == plate.Id)?.Status ?? PlateStatusOption.None
        }).ToList();
        var response = new PlatesApiResponse
        {
            Plates = plateDtos,
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
        await _context.ReservePlate(plateId);
        return NoContent();
    }
    
    [HttpPost]
    [Route("/plates/sell")]
    public async Task<IActionResult> SellPlate([FromBody] Guid plateId)
    {
        await _context.ReservePlate(plateId);
        return NoContent();
    }
    
    [HttpPost]
    [Route("/plates/unreserve")]
    public async Task<IActionResult> UnreservePlate([FromBody] Guid plateId)
    {
        await _context.ReservePlate(plateId);
        return NoContent();
    }
}
