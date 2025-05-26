using Catalog.API.Models.Requests;

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
    
    public IActionResult Index()
    {
        return new RedirectResult("~/swagger");
    }
    
    
    public IActionResult GetPagedPlates([FromQuery] PlatesApiRequest request)
    {
        throw new NotImplementedException();
    }
}
