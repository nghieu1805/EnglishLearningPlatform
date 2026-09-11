using EnglishLearning.Application.Interfaces;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace EnglishLearning.Web.Controllers;
[Authorize]
public class DashboardController(ILearningRepository repository,ApplicationDbContext db):Controller
{
 public async Task<IActionResult> Index(){
  var userId=User.FindFirstValue(ClaimTypes.NameIdentifier)!;
  var progress=await repository.ProgressAsync(userId);var history=await repository.HistoryAsync(userId);
  var topics=await db.Topics.AsNoTracking().Where(t=>t.IsPublished).Select(t=>new{t.Id,t.Name,Total=t.Vocabulary.Count}).ToListAsync();
  var summaries=topics.Select(t=>new TopicSummary(t.Name,t.Total,progress.Count(p=>p.Vocabulary?.TopicId==t.Id&&p.Status==LearningStatus.Learned))).ToList();
  return View(new DashboardPage(progress,history,summaries));
 }
}
