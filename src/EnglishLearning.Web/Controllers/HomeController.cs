using EnglishLearning.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Web.Controllers;

public class HomeController(
    ILearningRepository repository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var grades =
            await repository.GradesAsync();

        return View(grades);
    }

    [HttpGet]
    public IActionResult Error()
    {
        Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        return View();
    }

    [HttpGet]
    public IActionResult HttpStatus(int code)
    {
        Response.StatusCode = code;

        return View(code);
    }
}