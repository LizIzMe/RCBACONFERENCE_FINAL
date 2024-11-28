using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RCBACONFERENCE.Data;
using System.Security.Claims;

public class BaseController : Controller
{
    private readonly ApplicationDbContext _context;

    public BaseController(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            var user = _context.UsersConference.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                ViewBag.UserAffiliation = user.Affiliation;
            }
        }

        base.OnActionExecuting(context);
    }
}
