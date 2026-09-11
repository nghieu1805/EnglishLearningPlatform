using EnglishLearning.Application.Interfaces;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace EnglishLearning.Web.Controllers;
public class LearnController(ILearningRepository repository,ApplicationDbContext db):Controller
{
 public async Task<IActionResult> Grade(int id){var g=await db.Grades.FindAsync(id);if(g is null)return NotFound();ViewBag.Grade=g.Name;return View(await repository.TopicsAsync(id));}
 public async Task<IActionResult> Topic(int id){var t=await repository.TopicAsync(id);if(t is null)return NotFound();return View(new TopicPage(t,await db.Exercises.AsNoTracking().Where(x=>x.TopicId==id).ToListAsync()));}
 public async Task<IActionResult> Practice(int id){var t=await repository.TopicAsync(id);return t is null?NotFound():View(t);}
 [Authorize,HttpPost] public async Task<IActionResult> Progress(int vocabularyId,LearningStatus status,string? returnUrl){
  try{await repository.SaveProgressAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!,vocabularyId,status);}
  catch(ArgumentException e){return BadRequest(e.Message);}
  return Url.IsLocalUrl(returnUrl)?LocalRedirect(returnUrl!):RedirectToAction("Index","Dashboard");
 }
}
