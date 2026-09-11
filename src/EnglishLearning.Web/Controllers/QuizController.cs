using EnglishLearning.Application.Services;
using EnglishLearning.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace EnglishLearning.Web.Controllers;
[Authorize]
public class QuizController(QuizService quiz,ILearningRepository repository):Controller
{
 public async Task<IActionResult> Take(int id){var model=await quiz.GetAsync(id);return model is null?NotFound("Quiz chưa sẵn sàng."):View(model);}
 [HttpPost] public async Task<IActionResult> Take(int id,Dictionary<int,int> selections){
  if(!ModelState.IsValid)return BadRequest("Dữ liệu câu trả lời không hợp lệ.");
  try{var attempt=await quiz.SubmitAsync(id,User.FindFirstValue(ClaimTypes.NameIdentifier)!,selections);return attempt is null?NotFound():RedirectToAction(nameof(Result),new{id=attempt});}
  catch(ArgumentException e){return BadRequest(e.Message);}
 }
 public async Task<IActionResult> Result(int id){var a=await repository.AttemptAsync(id,User.FindFirstValue(ClaimTypes.NameIdentifier)!);return a is null?NotFound():View(a);}
}
