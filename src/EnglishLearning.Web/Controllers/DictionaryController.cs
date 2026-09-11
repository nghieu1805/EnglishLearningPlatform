using EnglishLearning.Application.Interfaces;
using EnglishLearning.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
namespace EnglishLearning.Web.Controllers;
public class DictionaryController(ILearningRepository repository):Controller
{
 public async Task<IActionResult> Index(string? q){q=q?.Trim();if(q?.Length>100)return BadRequest();ViewBag.Query=q;return View(string.IsNullOrWhiteSpace(q)?new List<Vocabulary>():await repository.SearchAsync(q));}
}
