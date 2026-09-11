using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace EnglishLearning.Infrastructure.Data;
public static class DbSeeder
{
 public static async Task SeedAsync(ApplicationDbContext db,RoleManager<IdentityRole> roles,UserManager<ApplicationUser> users,string? adminEmail,string? adminPassword,bool demo)
 {
  foreach(var role in new[]{"Admin","Student"})if(!await roles.RoleExistsAsync(role)){var result=await roles.CreateAsync(new(role));if(!result.Succeeded)throw new InvalidOperationException("Không tạo được role.");}
  for(int i=1;i<=12;i++)if(!await db.Grades.AnyAsync(x=>x.Number==i))db.Grades.Add(new(){Number=i,Name=$"Lớp {i}"});
  await db.SaveChangesAsync();
  if(!string.IsNullOrWhiteSpace(adminEmail)&&!string.IsNullOrWhiteSpace(adminPassword))
  {
   var admin=await users.FindByEmailAsync(adminEmail);
   if(admin is null){admin=new(){Email=adminEmail,UserName=adminEmail,DisplayName="Administrator"};var result=await users.CreateAsync(admin,adminPassword);if(!result.Succeeded)throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description)));}
   var added=await users.AddToRoleAsync(admin,"Admin");if(!added.Succeeded&&!await users.IsInRoleAsync(admin,"Admin"))throw new InvalidOperationException("Không cấp được quyền Admin.");
  }
  if(demo&&!await db.Topics.AnyAsync(x=>x.IsDemo))
  {
   var grade=await db.Grades.SingleAsync(x=>x.Number==6);
   var t=new Topic{GradeId=grade.Id,Name="School — DEMO",IsDemo=true,IsPublished=true,Description="Dữ liệu tự viết để kiểm tra ứng dụng, chưa đối chiếu SGK."};
   foreach(var (word,meaning,example) in new[]{("student","học sinh","I am a student."),("teacher","giáo viên","Our teacher is kind."),("classroom","phòng học","The classroom is clean."),("homework","bài tập về nhà","I do my homework after school.")})
    t.Vocabulary.Add(new(){Word=word,MeaningVi=meaning,PartOfSpeech="noun",Examples=[new(){Sentence=example}]});
   db.Topics.Add(t);await db.SaveChangesAsync();
   db.Exercises.Add(new(){TopicId=t.Id,Title="School — Quiz minh hoạ",Questions=[new(){Prompt="student có nghĩa là gì?",Answers=[new(){Text="học sinh",IsCorrect=true},new(){Text="giáo viên"},new(){Text="phòng học"}]},new(){Prompt="Từ nào có nghĩa là bài tập về nhà?",Answers=[new(){Text="teacher"},new(){Text="homework",IsCorrect=true},new(){Text="classroom"}]}]});
   db.Vocabularies.Add(new(){Word="sustainability",MeaningVi="tính bền vững",PartOfSpeech="noun",Examples=[new(){Sentence="Sustainability matters for our future."}]});
   await db.SaveChangesAsync();
  }
 }
}
