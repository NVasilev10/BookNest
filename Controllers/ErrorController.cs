using Microsoft.AspNetCore.Mvc;

namespace BookNest.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            return statusCode switch
            {
                404 => View("NotFound"),
                500 => View("InternalServerError"),
                _ => View("Error")
            };
        }
    }
}
