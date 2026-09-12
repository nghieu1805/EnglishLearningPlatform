using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roles,
        UserManager<ApplicationUser> users,
        string? adminEmail,
        string? adminPassword,
        bool demo)
    {
        await SeedRolesAsync(roles);
        await SeedGradesAsync(db);
        await SeedAdminAsync(users, adminEmail, adminPassword);

        if (demo)
        {
            await SeedDemoContentAsync(db);
        }
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames =
        [
            "Admin",
            "Staff",
            "Student"
        ];

        foreach (var roleName in roleNames)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Không thể tạo role {roleName}: {errors}");
            }
        }
    }

    private static async Task SeedGradesAsync(
        ApplicationDbContext db)
    {
        for (var gradeNumber = 1; gradeNumber <= 12; gradeNumber++)
        {
            var gradeExists = await db.Grades.AnyAsync(
                grade => grade.Number == gradeNumber);

            if (gradeExists)
            {
                continue;
            }

            db.Grades.Add(new Grade
            {
                Number = gradeNumber,
                Name = $"Lớp {gradeNumber}"
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        string? adminEmail,
        string? adminPassword)
    {
        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(
                admin,
                adminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Không thể tạo tài khoản Admin: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(
                admin,
                "Admin");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Không thể cấp quyền Admin: {errors}");
            }
        }
    }

    private static async Task SeedDemoContentAsync(
        ApplicationDbContext db)
    {
        var demoExists = await db.Topics.AnyAsync(
            topic => topic.IsDemo);

        if (demoExists)
        {
            return;
        }

        var grade6 = await db.Grades.SingleAsync(
            grade => grade.Number == 6);

        var topic = new Topic
        {
            GradeId = grade6.Id,
            Name = "School — DEMO",
            Description =
                "Dữ liệu tự viết để kiểm tra ứng dụng, chưa đối chiếu SGK.",
            IsDemo = true,
            IsPublished = true,
            SortOrder = 1
        };

        var vocabularyData = new[]
        {
            new
            {
                Word = "student",
                Meaning = "học sinh",
                Example = "I am a student."
            },
            new
            {
                Word = "teacher",
                Meaning = "giáo viên",
                Example = "Our teacher is kind."
            },
            new
            {
                Word = "classroom",
                Meaning = "phòng học",
                Example = "The classroom is clean."
            },
            new
            {
                Word = "homework",
                Meaning = "bài tập về nhà",
                Example = "I do my homework after school."
            }
        };

        foreach (var item in vocabularyData)
        {
            topic.Vocabulary.Add(new Vocabulary
            {
                Word = item.Word,
                MeaningVi = item.Meaning,
                PartOfSpeech = "noun",
                Examples =
                [
                    new VocabularyExample
                    {
                        Sentence = item.Example
                    }
                ]
            });
        }

        db.Topics.Add(topic);
        await db.SaveChangesAsync();

        var exercise = new Exercise
        {
            TopicId = topic.Id,
            Title = "School — Quiz minh hoạ",
            Questions =
            [
                new Question
                {
                    Prompt = "student có nghĩa là gì?",
                    Answers =
                    [
                        new Answer
                        {
                            Text = "học sinh",
                            IsCorrect = true
                        },
                        new Answer
                        {
                            Text = "giáo viên",
                            IsCorrect = false
                        },
                        new Answer
                        {
                            Text = "phòng học",
                            IsCorrect = false
                        }
                    ]
                },
                new Question
                {
                    Prompt =
                        "Từ nào có nghĩa là bài tập về nhà?",
                    Answers =
                    [
                        new Answer
                        {
                            Text = "teacher",
                            IsCorrect = false
                        },
                        new Answer
                        {
                            Text = "homework",
                            IsCorrect = true
                        },
                        new Answer
                        {
                            Text = "classroom",
                            IsCorrect = false
                        }
                    ]
                }
            ]
        };

        db.Exercises.Add(exercise);

        var dictionaryVocabularyExists =
            await db.Vocabularies.AnyAsync(
                vocabulary =>
                    vocabulary.Word == "sustainability" &&
                    vocabulary.TopicId == null);

        if (!dictionaryVocabularyExists)
        {
            db.Vocabularies.Add(new Vocabulary
            {
                Word = "sustainability",
                MeaningVi = "tính bền vững",
                PartOfSpeech = "noun",
                Examples =
                [
                    new VocabularyExample
                    {
                        Sentence =
                            "Sustainability matters for our future.",
                        TranslationVi =
                            "Tính bền vững rất quan trọng đối với tương lai."
                    }
                ]
            });
        }

        await db.SaveChangesAsync();
    }
}