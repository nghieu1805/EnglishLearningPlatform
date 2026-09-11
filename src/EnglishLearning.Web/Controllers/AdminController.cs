using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace EnglishLearning.Web.Controllers;
[Authorize(Roles="Admin")]
public class AdminController(ApplicationDbContext db,UserManager<ApplicationUser> users):Controller
{
 public async Task<IActionResult> Index(){ViewBag.Users=await db.Users.CountAsync();ViewBag.Words=await db.Vocabularies.CountAsync();ViewBag.Quizzes=await db.QuizAttempts.CountAsync();return View();}
 public async Task<IActionResult> Users()=>View(await db.Users.AsNoTracking().OrderBy(x=>x.Email).Take(200).ToListAsync());
 private async Task Options(){
  ViewBag.Grades=new SelectList(await db.Grades.OrderBy(x=>x.Number).ToListAsync(),"Id","Name");
  ViewBag.Topics=new SelectList(await db.Topics.Include(x=>x.Grade).OrderBy(x=>x.GradeId).ThenBy(x=>x.Name).Select(x=>new{x.Id,Label=x.Grade!.Name+" / "+x.Name}).ToListAsync(),"Id","Label");
  ViewBag.Sources=new SelectList(await db.TopicSources.OrderBy(x=>x.Id).Select(x=>new{x.Id,Label=x.Topic!.Name+" / "+x.Textbook+" / "+x.Unit}).ToListAsync(),"Id","Label");
  ViewBag.Words=new SelectList(await db.Vocabularies.OrderBy(x=>x.Word).Select(x=>new{x.Id,Label=x.Word+" (#"+x.Id+")"}).ToListAsync(),"Id","Label");
  ViewBag.Exercises=new SelectList(await db.Exercises.OrderBy(x=>x.Id).ToListAsync(),"Id","Title");
  ViewBag.Questions=new SelectList(await db.Questions.OrderBy(x=>x.Id).Select(x=>new{x.Id,Label=x.Exercise!.Title+" / "+x.Prompt}).ToListAsync(),"Id","Label");
 }
 private async Task ValidateContent(object entity){
  switch(entity){
   case Grade g:
    if(await db.Grades.AnyAsync(x=>x.Number==g.Number&&x.Id!=g.Id))ModelState.AddModelError("Number","Số lớp đã tồn tại.");break;
   case Topic t:
    if(!await db.Grades.AnyAsync(x=>x.Id==t.GradeId))ModelState.AddModelError("GradeId","Lớp không tồn tại.");
    if(t.IsPublished&&!t.IsDemo){
     if(!await db.TopicSources.AnyAsync(x=>x.TopicId==t.Id))ModelState.AddModelError("IsPublished","Lưu bản nháp và thêm nguồn SGK trước khi xuất bản.");
     if(await db.Vocabularies.AnyAsync(x=>x.TopicId==t.Id&&x.TopicSourceId==null))ModelState.AddModelError("IsPublished","Tất cả từ thuộc bài chính thức phải có nguồn.");
    }break;
   case TopicSource s:
    if(!await db.Topics.AnyAsync(x=>x.Id==s.TopicId))ModelState.AddModelError("TopicId","Chủ đề không tồn tại.");
    if(s.Id>0&&await db.Vocabularies.AnyAsync(x=>x.TopicSourceId==s.Id&&x.TopicId!=s.TopicId))ModelState.AddModelError("TopicId","Không thể chuyển nguồn đang được từ vựng sử dụng sang chủ đề khác.");break;
   case Vocabulary v:
    Topic? topic=null;
    if(v.TopicId.HasValue){topic=await db.Topics.FindAsync(v.TopicId.Value);if(topic is null)ModelState.AddModelError("TopicId","Chủ đề không tồn tại.");}
    if(topic is {IsDemo:false}&&!v.TopicSourceId.HasValue)ModelState.AddModelError("TopicSourceId","Từ thuộc chủ đề chính thức phải có nguồn SGK/Unit.");
    if(v.TopicSourceId.HasValue&&!await db.TopicSources.AnyAsync(x=>x.Id==v.TopicSourceId&&x.TopicId==v.TopicId))ModelState.AddModelError("TopicSourceId","Nguồn phải thuộc đúng chủ đề đã chọn.");
    if(v.Cefr!=""&&!new[]{"A1","A2","B1","B2","C1","C2"}.Contains(v.Cefr))ModelState.AddModelError("Cefr","CEFR phải là A1–C2 hoặc để trống.");break;
   case VocabularyExample e:
    if(!await db.Vocabularies.AnyAsync(x=>x.Id==e.VocabularyId))ModelState.AddModelError("VocabularyId","Từ vựng không tồn tại.");break;
   case VocabularyAudio a:
    if(!await db.Vocabularies.AnyAsync(x=>x.Id==a.VocabularyId))ModelState.AddModelError("VocabularyId","Từ vựng không tồn tại.");
    if(!Uri.TryCreate(a.Url,UriKind.Absolute,out var url)||url.Scheme!="https")ModelState.AddModelError("Url","Chỉ chấp nhận URL HTTPS.");break;
   case Exercise e:
    if(!await db.Topics.AnyAsync(x=>x.Id==e.TopicId))ModelState.AddModelError("TopicId","Chủ đề không tồn tại.");break;
   case Question q:
    if(!await db.Exercises.AnyAsync(x=>x.Id==q.ExerciseId))ModelState.AddModelError("ExerciseId","Quiz không tồn tại.");break;
   case Answer a:
    if(!await db.Questions.AnyAsync(x=>x.Id==a.QuestionId))ModelState.AddModelError("QuestionId","Câu hỏi không tồn tại.");
    if(a.IsCorrect&&await db.Answers.AnyAsync(x=>x.QuestionId==a.QuestionId&&x.IsCorrect&&x.Id!=a.Id))ModelState.AddModelError("IsCorrect","Đã có đáp án đúng. Bỏ chọn đáp án cũ trước.");break;
  }
 }

 public async Task<IActionResult> Grades()=>View(await db.Grades.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditGrade(int id=0){var entity=id==0?new Grade():await db.Grades.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditGrade([Bind("Id,Number,Name")] Grade input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.Grades.Add(input);else{var entity=await db.Grades.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(Grades));
 }
 [HttpPost] public async Task<IActionResult> DeleteGrade(int id){
  var entity=await db.Grades.FindAsync(id);if(entity is null)return NotFound();
  
  db.Grades.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(Grades));
 }

 public async Task<IActionResult> Topics()=>View(await db.Topics.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditTopic(int id=0){var entity=id==0?new Topic():await db.Topics.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditTopic([Bind("Id,GradeId,Name,Description,SortOrder,IsPublished,IsDemo")] Topic input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.Topics.Add(input);else{var entity=await db.Topics.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(Topics));
 }
 [HttpPost] public async Task<IActionResult> DeleteTopic(int id){
  var entity=await db.Topics.FindAsync(id);if(entity is null)return NotFound();
  
  db.Topics.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(Topics));
 }

 public async Task<IActionResult> TopicSources()=>View(await db.TopicSources.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditTopicSource(int id=0){var entity=id==0?new TopicSource():await db.TopicSources.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditTopicSource([Bind("Id,TopicId,Textbook,OriginalTopicName,Unit")] TopicSource input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.TopicSources.Add(input);else{var entity=await db.TopicSources.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(TopicSources));
 }
 [HttpPost] public async Task<IActionResult> DeleteTopicSource(int id){
  var entity=await db.TopicSources.FindAsync(id);if(entity is null)return NotFound();
  if(await db.Topics.AnyAsync(t=>t.Id==entity.TopicId&&t.IsPublished&&!t.IsDemo)&&await db.TopicSources.CountAsync(s=>s.TopicId==entity.TopicId)<=1){TempData["Message"]="Chuyển chủ đề về bản nháp trước khi xoá nguồn cuối cùng.";return RedirectToAction(nameof(TopicSources));}
  db.TopicSources.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(TopicSources));
 }

 public async Task<IActionResult> Vocabularies()=>View(await db.Vocabularies.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditVocabulary(int id=0){var entity=id==0?new Vocabulary():await db.Vocabularies.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditVocabulary([Bind("Id,TopicId,TopicSourceId,Word,MeaningVi,PartOfSpeech,UkPhonetic,UsPhonetic,Cefr,RelatedWords,WordFamily")] Vocabulary input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.Vocabularies.Add(input);else{var entity=await db.Vocabularies.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(Vocabularies));
 }
 [HttpPost] public async Task<IActionResult> DeleteVocabulary(int id){
  var entity=await db.Vocabularies.FindAsync(id);if(entity is null)return NotFound();
  
  db.Vocabularies.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(Vocabularies));
 }

 public async Task<IActionResult> VocabularyExamples()=>View(await db.VocabularyExamples.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditVocabularyExample(int id=0){var entity=id==0?new VocabularyExample():await db.VocabularyExamples.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditVocabularyExample([Bind("Id,VocabularyId,Sentence,TranslationVi")] VocabularyExample input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.VocabularyExamples.Add(input);else{var entity=await db.VocabularyExamples.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(VocabularyExamples));
 }
 [HttpPost] public async Task<IActionResult> DeleteVocabularyExample(int id){
  var entity=await db.VocabularyExamples.FindAsync(id);if(entity is null)return NotFound();
  
  db.VocabularyExamples.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(VocabularyExamples));
 }

 public async Task<IActionResult> VocabularyAudios()=>View(await db.VocabularyAudios.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditVocabularyAudio(int id=0){var entity=id==0?new VocabularyAudio():await db.VocabularyAudios.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditVocabularyAudio([Bind("Id,VocabularyId,Accent,Url")] VocabularyAudio input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.VocabularyAudios.Add(input);else{var entity=await db.VocabularyAudios.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(VocabularyAudios));
 }
 [HttpPost] public async Task<IActionResult> DeleteVocabularyAudio(int id){
  var entity=await db.VocabularyAudios.FindAsync(id);if(entity is null)return NotFound();
  
  db.VocabularyAudios.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(VocabularyAudios));
 }

 public async Task<IActionResult> Exercises()=>View(await db.Exercises.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditExercise(int id=0){var entity=id==0?new Exercise():await db.Exercises.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditExercise([Bind("Id,TopicId,Title")] Exercise input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.Exercises.Add(input);else{var entity=await db.Exercises.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(Exercises));
 }
 [HttpPost] public async Task<IActionResult> DeleteExercise(int id){
  var entity=await db.Exercises.FindAsync(id);if(entity is null)return NotFound();
  
  db.Exercises.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(Exercises));
 }

 public async Task<IActionResult> Questions()=>View(await db.Questions.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditQuestion(int id=0){var entity=id==0?new Question():await db.Questions.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditQuestion([Bind("Id,ExerciseId,Prompt")] Question input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.Questions.Add(input);else{var entity=await db.Questions.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(Questions));
 }
 [HttpPost] public async Task<IActionResult> DeleteQuestion(int id){
  var entity=await db.Questions.FindAsync(id);if(entity is null)return NotFound();
  
  db.Questions.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(Questions));
 }

 public async Task<IActionResult> Answers()=>View(await db.Answers.AsNoTracking().OrderBy(x=>x.Id).ToListAsync());
 [HttpGet] public async Task<IActionResult> EditAnswer(int id=0){var entity=id==0?new Answer():await db.Answers.FindAsync(id);if(entity is null)return NotFound();await Options();return View(entity);}
 [HttpPost] public async Task<IActionResult> EditAnswer([Bind("Id,QuestionId,Text,IsCorrect")] Answer input){
  await ValidateContent(input);
  if(!ModelState.IsValid){await Options();return View(input);}
  if(input.Id==0)db.Answers.Add(input);else{var entity=await db.Answers.FindAsync(input.Id);if(entity is null)return NotFound();db.Entry(entity).CurrentValues.SetValues(input);}
  try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu do ràng buộc dữ liệu. Kiểm tra các bản ghi liên quan.");await Options();return View(input);}
  TempData["Message"]="Đã lưu dữ liệu.";return RedirectToAction(nameof(Answers));
 }
 [HttpPost] public async Task<IActionResult> DeleteAnswer(int id){
  var entity=await db.Answers.FindAsync(id);if(entity is null)return NotFound();
  
  db.Answers.Remove(entity);try{await db.SaveChangesAsync();TempData["Message"]="Đã xoá.";}catch(DbUpdateException){TempData["Message"]="Không thể xoá vì dữ liệu còn được sử dụng. Hãy xử lý bản ghi liên quan hoặc ẩn chủ đề.";}
  return RedirectToAction(nameof(Answers));
 }

}
