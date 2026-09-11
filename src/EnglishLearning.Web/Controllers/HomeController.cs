using EnglishLearning.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace EnglishLearning.Web.Controllers;
public class HomeController(ILearningRepository repository):Controller
{
 public async Task<IActionResult> Index()=>View(await repository.GradesAsync());
 public IActionResult Error(){Response.StatusCode=500;return View();}
}
