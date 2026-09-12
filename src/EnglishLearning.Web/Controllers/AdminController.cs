using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Infrastructure.Identity;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace EnglishLearning.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AdminController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Users = null;

        if (User.IsInRole("Admin"))
        {
            ViewBag.Users = await db.Users.CountAsync();
        }

        ViewBag.Words =
            await db.Vocabularies.CountAsync();

        ViewBag.Quizzes =
            await db.QuizAttempts.CountAsync();

        return View();
    }

    // =====================================================
    // USER AND ROLE MANAGEMENT — ADMIN ONLY
    // =====================================================

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Users(
        string? search,
        int page = 1)
    {
        const int pageSize = 20;

        search = search?.Trim() ?? string.Empty;
        page = Math.Max(page, 1);

        var query = db.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(user =>
                (user.Email != null &&
                 user.Email.Contains(search)) ||
                user.DisplayName.Contains(search));
        }

        var totalItems = await query.CountAsync();

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(
                totalItems / (double)pageSize));

        if (page > totalPages)
        {
            page = totalPages;
        }

        var applicationUsers = await query
            .OrderBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserRoleItemViewModel>();

        foreach (var applicationUser in applicationUsers)
        {
            var roles = await userManager.GetRolesAsync(
                applicationUser);

            items.Add(new UserRoleItemViewModel
            {
                Id = applicationUser.Id,
                Email = applicationUser.Email ?? string.Empty,
                DisplayName = applicationUser.DisplayName,
                CurrentGrade = applicationUser.CurrentGrade,
                Roles = roles.ToList()
            });
        }

        return View(new UserListViewModel
        {
            Items = items,
            Search = search,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeUserRole(
     ChangeUserRoleModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Message"] =
                "Thông tin phân quyền không hợp lệ.";

            return RedirectToAction(
                nameof(Users),
                new
                {
                    search = model.Search,
                    page = model.Page
                });
        }

        string[] allowedRoles =
        [
            "Student",
        "Staff"
        ];

        if (!allowedRoles.Contains(model.Role))
        {
            TempData["Message"] = "Role không hợp lệ.";

            return RedirectToAction(
                nameof(Users),
                new
                {
                    search = model.Search,
                    page = model.Page
                });
        }

        var applicationUser =
            await userManager.FindByIdAsync(model.UserId);

        if (applicationUser is null)
        {
            TempData["Message"] =
                "Không tìm thấy tài khoản.";

            return RedirectToAction(nameof(Users));
        }

        var currentRoles =
            await userManager.GetRolesAsync(applicationUser);

        if (currentRoles.Contains("Admin"))
        {
            TempData["Message"] =
                "Không thể thay đổi role của tài khoản Admin.";

            return RedirectToAction(
                nameof(Users),
                new
                {
                    search = model.Search,
                    page = model.Page
                });
        }

        var managedRoles = currentRoles
            .Where(role =>
                role == "Student" ||
                role == "Staff")
            .ToList();

        if (managedRoles.Count > 0)
        {
            var removeResult =
                await userManager.RemoveFromRolesAsync(
                    applicationUser,
                    managedRoles);

            if (!removeResult.Succeeded)
            {
                TempData["Message"] = string.Join(
                    "; ",
                    removeResult.Errors.Select(
                        error => error.Description));

                return RedirectToAction(
                    nameof(Users),
                    new
                    {
                        search = model.Search,
                        page = model.Page
                    });
            }
        }

        var addResult =
            await userManager.AddToRoleAsync(
                applicationUser,
                model.Role);

        if (!addResult.Succeeded)
        {
            TempData["Message"] = string.Join(
                "; ",
                addResult.Errors.Select(
                    error => error.Description));

            return RedirectToAction(
                nameof(Users),
                new
                {
                    search = model.Search,
                    page = model.Page
                });
        }

        TempData["Message"] =
            $"Đã cập nhật {applicationUser.Email} thành {model.Role}.";

        return RedirectToAction(
            nameof(Users),
            new
            {
                search = model.Search,
                page = model.Page
            });
    }

    private IActionResult RedirectToUsers(
        ChangeUserRoleModel model)
    {
        return RedirectToAction(
            nameof(Users),
            new
            {
                search = model.Search,
                page = model.Page
            });
    }

    private static string GetIdentityErrors(
        IdentityResult result)
    {
        return string.Join(
            "; ",
            result.Errors.Select(error => error.Description));
    }

    // =====================================================
    // SELECT OPTIONS
    // =====================================================

    private async Task Options()
    {
        ViewBag.Grades = new SelectList(
            await db.Grades
                .OrderBy(grade => grade.Number)
                .ToListAsync(),
            "Id",
            "Name");

        ViewBag.Topics = new SelectList(
            await db.Topics
                .Include(topic => topic.Grade)
                .OrderBy(topic => topic.GradeId)
                .ThenBy(topic => topic.Name)
                .Select(topic => new
                {
                    topic.Id,
                    Label =
                        topic.Grade!.Name +
                        " / " +
                        topic.Name
                })
                .ToListAsync(),
            "Id",
            "Label");

        ViewBag.Sources = new SelectList(
            await db.TopicSources
                .OrderBy(source => source.Id)
                .Select(source => new
                {
                    source.Id,
                    Label =
                        source.Topic!.Name +
                        " / " +
                        source.Textbook +
                        " / " +
                        source.Unit
                })
                .ToListAsync(),
            "Id",
            "Label");

        ViewBag.Words = new SelectList(
            await db.Vocabularies
                .OrderBy(vocabulary => vocabulary.Word)
                .Select(vocabulary => new
                {
                    vocabulary.Id,
                    Label =
                        vocabulary.Word +
                        " (#" +
                        vocabulary.Id +
                        ")"
                })
                .ToListAsync(),
            "Id",
            "Label");

        ViewBag.Exercises = new SelectList(
            await db.Exercises
                .OrderBy(exercise => exercise.Id)
                .ToListAsync(),
            "Id",
            "Title");

        ViewBag.Questions = new SelectList(
            await db.Questions
                .OrderBy(question => question.Id)
                .Select(question => new
                {
                    question.Id,
                    Label =
                        question.Exercise!.Title +
                        " / " +
                        question.Prompt
                })
                .ToListAsync(),
            "Id",
            "Label");
    }

    // =====================================================
    // CONTENT VALIDATION
    // =====================================================

    private async Task ValidateContent(object entity)
    {
        switch (entity)
        {
            case Grade grade:
                {
                    var duplicate = await db.Grades.AnyAsync(
                        item =>
                            item.Number == grade.Number &&
                            item.Id != grade.Id);

                    if (duplicate)
                    {
                        ModelState.AddModelError(
                            nameof(Grade.Number),
                            "Số lớp đã tồn tại.");
                    }

                    break;
                }

            case Topic topic:
                {
                    var gradeExists = await db.Grades.AnyAsync(
                        grade => grade.Id == topic.GradeId);

                    if (!gradeExists)
                    {
                        ModelState.AddModelError(
                            nameof(Topic.GradeId),
                            "Lớp không tồn tại.");
                    }

                    if (topic.IsPublished && !topic.IsDemo)
                    {
                        var hasSource =
                            await db.TopicSources.AnyAsync(
                                source =>
                                    source.TopicId == topic.Id);

                        if (!hasSource)
                        {
                            ModelState.AddModelError(
                                nameof(Topic.IsPublished),
                                "Lưu bản nháp và thêm nguồn SGK " +
                                "trước khi xuất bản.");
                        }

                        var hasVocabularyWithoutSource =
                            await db.Vocabularies.AnyAsync(
                                vocabulary =>
                                    vocabulary.TopicId == topic.Id &&
                                    vocabulary.TopicSourceId == null);

                        if (hasVocabularyWithoutSource)
                        {
                            ModelState.AddModelError(
                                nameof(Topic.IsPublished),
                                "Tất cả từ thuộc bài chính thức " +
                                "phải có nguồn.");
                        }
                    }

                    break;
                }

            case TopicSource source:
                {
                    var topicExists = await db.Topics.AnyAsync(
                        topic => topic.Id == source.TopicId);

                    if (!topicExists)
                    {
                        ModelState.AddModelError(
                            nameof(TopicSource.TopicId),
                            "Chủ đề không tồn tại.");
                    }

                    if (source.Id > 0)
                    {
                        var sourceUsedByAnotherTopic =
                            await db.Vocabularies.AnyAsync(
                                vocabulary =>
                                    vocabulary.TopicSourceId ==
                                    source.Id &&
                                    vocabulary.TopicId !=
                                    source.TopicId);

                        if (sourceUsedByAnotherTopic)
                        {
                            ModelState.AddModelError(
                                nameof(TopicSource.TopicId),
                                "Không thể chuyển nguồn đang được " +
                                "từ vựng sử dụng sang chủ đề khác.");
                        }
                    }

                    break;
                }

            case Vocabulary vocabulary:
                {
                    Topic? topic = null;

                    if (vocabulary.TopicId.HasValue)
                    {
                        topic = await db.Topics.FindAsync(
                            vocabulary.TopicId.Value);

                        if (topic is null)
                        {
                            ModelState.AddModelError(
                                nameof(Vocabulary.TopicId),
                                "Chủ đề không tồn tại.");
                        }
                    }

                    if (topic is { IsDemo: false } &&
                        !vocabulary.TopicSourceId.HasValue)
                    {
                        ModelState.AddModelError(
                            nameof(Vocabulary.TopicSourceId),
                            "Từ thuộc chủ đề chính thức phải có " +
                            "nguồn SGK/Unit.");
                    }

                    if (vocabulary.TopicSourceId.HasValue)
                    {
                        var sourceMatchesTopic =
                            await db.TopicSources.AnyAsync(
                                source =>
                                    source.Id ==
                                    vocabulary.TopicSourceId &&
                                    source.TopicId ==
                                    vocabulary.TopicId);

                        if (!sourceMatchesTopic)
                        {
                            ModelState.AddModelError(
                                nameof(Vocabulary.TopicSourceId),
                                "Nguồn phải thuộc đúng chủ đề đã chọn.");
                        }
                    }

                    string[] cefrLevels =
                    [
                        "A1",
                    "A2",
                    "B1",
                    "B2",
                    "C1",
                    "C2"
                    ];

                    if (!string.IsNullOrWhiteSpace(vocabulary.Cefr) &&
                        !cefrLevels.Contains(vocabulary.Cefr))
                    {
                        ModelState.AddModelError(
                            nameof(Vocabulary.Cefr),
                            "CEFR phải là A1–C2 hoặc để trống.");
                    }

                    break;
                }

            case VocabularyExample example:
                {
                    var vocabularyExists =
                        await db.Vocabularies.AnyAsync(
                            vocabulary =>
                                vocabulary.Id ==
                                example.VocabularyId);

                    if (!vocabularyExists)
                    {
                        ModelState.AddModelError(
                            nameof(VocabularyExample.VocabularyId),
                            "Từ vựng không tồn tại.");
                    }

                    break;
                }

            case VocabularyAudio audio:
                {
                    var vocabularyExists =
                        await db.Vocabularies.AnyAsync(
                            vocabulary =>
                                vocabulary.Id ==
                                audio.VocabularyId);

                    if (!vocabularyExists)
                    {
                        ModelState.AddModelError(
                            nameof(VocabularyAudio.VocabularyId),
                            "Từ vựng không tồn tại.");
                    }

                    var validUrl =
                        Uri.TryCreate(
                            audio.Url,
                            UriKind.Absolute,
                            out var url) &&
                        url.Scheme == Uri.UriSchemeHttps;

                    if (!validUrl)
                    {
                        ModelState.AddModelError(
                            nameof(VocabularyAudio.Url),
                            "Chỉ chấp nhận URL HTTPS.");
                    }

                    break;
                }

            case Exercise exercise:
                {
                    var topicExists = await db.Topics.AnyAsync(
                        topic => topic.Id == exercise.TopicId);

                    if (!topicExists)
                    {
                        ModelState.AddModelError(
                            nameof(Exercise.TopicId),
                            "Chủ đề không tồn tại.");
                    }

                    break;
                }

            case Question question:
                {
                    var exerciseExists =
                        await db.Exercises.AnyAsync(
                            exercise =>
                                exercise.Id ==
                                question.ExerciseId);

                    if (!exerciseExists)
                    {
                        ModelState.AddModelError(
                            nameof(Question.ExerciseId),
                            "Quiz không tồn tại.");
                    }

                    break;
                }

            case Answer answer:
                {
                    var questionExists =
                        await db.Questions.AnyAsync(
                            question =>
                                question.Id ==
                                answer.QuestionId);

                    if (!questionExists)
                    {
                        ModelState.AddModelError(
                            nameof(Answer.QuestionId),
                            "Câu hỏi không tồn tại.");
                    }

                    if (answer.IsCorrect)
                    {
                        var correctAnswerExists =
                            await db.Answers.AnyAsync(
                                item =>
                                    item.QuestionId ==
                                    answer.QuestionId &&
                                    item.IsCorrect &&
                                    item.Id != answer.Id);

                        if (correctAnswerExists)
                        {
                            ModelState.AddModelError(
                                nameof(Answer.IsCorrect),
                                "Đã có đáp án đúng. Bỏ chọn đáp án " +
                                "cũ trước.");
                        }
                    }

                    break;
                }
        }
    }

    private async Task<IActionResult> SaveError(
        string message,
        object model)
    {
        ModelState.AddModelError(string.Empty, message);

        await Options();

        return View(model);
    }

    // =====================================================
    // GRADES — ADMIN ONLY
    // =====================================================

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Grades()
    {
        return View(
            await db.Grades
                .AsNoTracking()
                .OrderBy(grade => grade.Number)
                .ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> EditGrade(int id = 0)
    {
        var entity = id == 0
            ? new Grade()
            : await db.Grades.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> EditGrade(
        [Bind("Id,Number,Name")] Grade input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.Grades.Add(input);
        }
        else
        {
            var entity = await db.Grades.FindAsync(input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu lớp do ràng buộc dữ liệu.",
                input);
        }

        TempData["Message"] = "Đã lưu lớp.";

        return RedirectToAction(nameof(Grades));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteGrade(int id)
    {
        var entity = await db.Grades.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.Grades.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(nameof(Grades));
    }

    // =====================================================
    // TOPICS — ADMIN AND STAFF
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Topics(
        string? search,
        int? gradeId,
        string? status,
        int page = 1)
    {
        const int pageSize = 15;

        search = search?.Trim() ?? string.Empty;
        status = status?.Trim().ToLowerInvariant() ?? string.Empty;
        page = Math.Max(page, 1);

        string[] allowedStatuses =
        [
            "",
        "draft",
        "published",
        "demo",
        "official"
        ];

        if (!allowedStatuses.Contains(status))
        {
            status = string.Empty;
        }

        var query = db.Topics
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(topic =>
                topic.Name.Contains(search) ||
                (topic.Description != null &&
                 topic.Description.Contains(search)));
        }

        if (gradeId.HasValue)
        {
            query = query.Where(topic =>
                topic.GradeId == gradeId.Value);
        }

        query = status switch
        {
            "draft" => query.Where(topic =>
                !topic.IsPublished),

            "published" => query.Where(topic =>
                topic.IsPublished),

            "demo" => query.Where(topic =>
                topic.IsDemo),

            "official" => query.Where(topic =>
                !topic.IsDemo),

            _ => query
        };

        int totalItems = await query.CountAsync();

        int totalPages = Math.Max(
            1,
            (int)Math.Ceiling(
                totalItems / (double)pageSize));

        if (page > totalPages)
        {
            page = totalPages;
        }

        var items = await query
            .OrderBy(topic => topic.Grade!.Number)
            .ThenBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(topic =>
                new TopicListItemViewModel
                {
                    Id = topic.Id,
                    GradeId = topic.GradeId,
                    GradeName = topic.Grade!.Name,
                    Name = topic.Name,
                    Description = topic.Description,
                    SortOrder = topic.SortOrder,
                    IsPublished = topic.IsPublished,
                    IsDemo = topic.IsDemo,

                    SourceCount = db.TopicSources.Count(source =>
                        source.TopicId == topic.Id),

                    VocabularyCount = db.Vocabularies.Count(vocabulary =>
                        vocabulary.TopicId == topic.Id)
                })
            .ToListAsync();

        ViewBag.Grades = new SelectList(
            await db.Grades
                .AsNoTracking()
                .OrderBy(grade => grade.Number)
                .ToListAsync(),
            "Id",
            "Name",
            gradeId);

        var model = new TopicListViewModel
        {
            Items = items,
            Search = search,
            GradeId = gradeId,
            Status = status,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditTopic(int id = 0)
    {
        var entity = id == 0
            ? new Topic()
            : await db.Topics.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditTopic(
        [Bind(
            "Id,GradeId,Name,Description,SortOrder," +
            "IsPublished,IsDemo")]
        Topic input)
    {
        Topic? existingTopic = null;

        if (input.Id > 0)
        {
            existingTopic =
                await db.Topics.FindAsync(input.Id);

            if (existingTopic is null)
            {
                return NotFound();
            }
        }

        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            if (existingTopic is null)
            {
                input.IsPublished = false;
                input.IsDemo = false;
            }
            else
            {
                input.IsPublished =
                    existingTopic.IsPublished;

                input.IsDemo =
                    existingTopic.IsDemo;
            }
        }

        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (existingTopic is null)
        {
            db.Topics.Add(input);
        }
        else
        {
            db.Entry(existingTopic)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu chủ đề do ràng buộc dữ liệu.",
                input);
        }

        TempData["Message"] = isAdmin
            ? "Đã lưu chủ đề."
            : "Đã lưu chủ đề. Admin sẽ kiểm tra và xuất bản.";

        return RedirectToAction(nameof(Topics));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        var entity = await db.Topics.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.Topics.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(nameof(Topics));
    }

    // =====================================================
    // TOPIC SOURCES
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> TopicSources()
    {
        return View(
            await db.TopicSources
                .AsNoTracking()
                .OrderBy(source => source.Id)
                .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> EditTopicSource(int id = 0)
    {
        var entity = id == 0
            ? new TopicSource()
            : await db.TopicSources.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditTopicSource(
        [Bind(
            "Id,TopicId,Textbook,OriginalTopicName,Unit")]
        TopicSource input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.TopicSources.Add(input);
        }
        else
        {
            var entity =
                await db.TopicSources.FindAsync(input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu nguồn SGK.",
                input);
        }

        TempData["Message"] = "Đã lưu nguồn SGK.";

        return RedirectToAction(nameof(TopicSources));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteTopicSource(int id)
    {
        var entity =
            await db.TopicSources.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        var isLastSourceOfPublishedTopic =
            await db.Topics.AnyAsync(topic =>
                topic.Id == entity.TopicId &&
                topic.IsPublished &&
                !topic.IsDemo) &&
            await db.TopicSources.CountAsync(source =>
                source.TopicId == entity.TopicId) <= 1;

        if (isLastSourceOfPublishedTopic)
        {
            TempData["Message"] =
                "Chuyển chủ đề về bản nháp trước khi " +
                "xoá nguồn cuối cùng.";

            return RedirectToAction(nameof(TopicSources));
        }

        db.TopicSources.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(nameof(TopicSources));
    }

    // =====================================================
    // VOCABULARY
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Vocabularies(
      string? search,
      int? gradeId,
      int? topicId,
      string? cefr,
      int page = 1)
    {
        const int pageSize = 20;

        search = search?.Trim() ?? string.Empty;
        cefr = cefr?.Trim().ToUpperInvariant() ?? string.Empty;
        page = Math.Max(page, 1);

        string[] allowedCefr =
        [
            "",
        "A1",
        "A2",
        "B1",
        "B2",
        "C1",
        "C2"
        ];

        if (!allowedCefr.Contains(cefr))
        {
            cefr = string.Empty;
        }

        var query = db.Vocabularies
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(vocabulary =>
                vocabulary.Word.Contains(search) ||
                vocabulary.MeaningVi.Contains(search));
        }

        if (gradeId.HasValue)
        {
            query = query.Where(vocabulary =>
                vocabulary.TopicId.HasValue &&
                db.Topics.Any(topic =>
                    topic.Id == vocabulary.TopicId.Value &&
                    topic.GradeId == gradeId.Value));
        }

        if (topicId.HasValue)
        {
            query = query.Where(vocabulary =>
                vocabulary.TopicId == topicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(cefr))
        {
            query = query.Where(vocabulary =>
                vocabulary.Cefr == cefr);
        }

        int totalItems = await query.CountAsync();

        int totalPages = Math.Max(
            1,
            (int)Math.Ceiling(
                totalItems / (double)pageSize));

        if (page > totalPages)
        {
            page = totalPages;
        }

        var items = await query
            .OrderBy(vocabulary => vocabulary.Word)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(vocabulary =>
                new VocabularyListItemViewModel
                {
                    Id = vocabulary.Id,
                    TopicId = vocabulary.TopicId,
                    Word = vocabulary.Word,
                    MeaningVi = vocabulary.MeaningVi,
                    PartOfSpeech = vocabulary.PartOfSpeech,
                    Cefr = vocabulary.Cefr,

                    GradeName = vocabulary.TopicId.HasValue
                        ? db.Topics
                            .Where(topic =>
                                topic.Id ==
                                vocabulary.TopicId.Value)
                            .Select(topic =>
                                topic.Grade!.Name)
                            .FirstOrDefault() ?? "—"
                        : "Từ vựng chung",

                    TopicName = vocabulary.TopicId.HasValue
                        ? db.Topics
                            .Where(topic =>
                                topic.Id ==
                                vocabulary.TopicId.Value)
                            .Select(topic => topic.Name)
                            .FirstOrDefault() ?? "—"
                        : "Từ vựng chung",

                    SourceName = vocabulary.TopicSourceId.HasValue
                        ? db.TopicSources
                            .Where(source =>
                                source.Id ==
                                vocabulary.TopicSourceId.Value)
                            .Select(source => source.Textbook)
                            .FirstOrDefault() ?? "—"
                        : "—",

                    Unit = vocabulary.TopicSourceId.HasValue
                        ? db.TopicSources
                            .Where(source =>
                                source.Id ==
                                vocabulary.TopicSourceId.Value)
                            .Select(source => source.Unit)
                            .FirstOrDefault() ?? "—"
                        : "—",

                    ExampleCount =
                        db.VocabularyExamples.Count(example =>
                            example.VocabularyId ==
                            vocabulary.Id),

                    AudioCount =
                        db.VocabularyAudios.Count(audio =>
                            audio.VocabularyId ==
                            vocabulary.Id)
                })
            .ToListAsync();

        ViewBag.Grades = new SelectList(
            await db.Grades
                .AsNoTracking()
                .OrderBy(grade => grade.Number)
                .ToListAsync(),
            "Id",
            "Name",
            gradeId);

        var topicFilterQuery = db.Topics
            .AsNoTracking()
            .AsQueryable();

        if (gradeId.HasValue)
        {
            topicFilterQuery = topicFilterQuery.Where(topic =>
                topic.GradeId == gradeId.Value);
        }

        var topicOptions = await topicFilterQuery
            .OrderBy(topic => topic.Grade!.Number)
            .ThenBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Name)
            .Select(topic => new
            {
                topic.Id,
                Label = topic.Grade!.Name +
                        " / " +
                        topic.Name
            })
            .ToListAsync();

        ViewBag.TopicFilters = new SelectList(
            topicOptions,
            "Id",
            "Label",
            topicId);

        var model = new VocabularyListViewModel
        {
            Items = items,
            Search = search,
            GradeId = gradeId,
            TopicId = topicId,
            Cefr = cefr,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditVocabulary(int id = 0)
    {
        var entity = id == 0
            ? new Vocabulary()
            : await db.Vocabularies.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditVocabulary(
        [Bind(
            "Id,TopicId,TopicSourceId,Word,MeaningVi," +
            "PartOfSpeech,UkPhonetic,UsPhonetic,Cefr," +
            "RelatedWords,WordFamily")]
        Vocabulary input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.Vocabularies.Add(input);
        }
        else
        {
            var entity =
                await db.Vocabularies.FindAsync(input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu từ vựng.",
                input);
        }

        TempData["Message"] = "Đã lưu từ vựng.";

        return RedirectToAction(nameof(Vocabularies));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteVocabulary(int id)
    {
        var entity =
            await db.Vocabularies.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.Vocabularies.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(nameof(Vocabularies));
    }

    // =====================================================
    // VOCABULARY EXAMPLES
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> VocabularyExamples()
    {
        return View(
            await db.VocabularyExamples
                .AsNoTracking()
                .OrderBy(example => example.Id)
                .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> EditVocabularyExample(
        int id = 0)
    {
        var entity = id == 0
            ? new VocabularyExample()
            : await db.VocabularyExamples.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditVocabularyExample(
        [Bind(
            "Id,VocabularyId,Sentence,TranslationVi")]
        VocabularyExample input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.VocabularyExamples.Add(input);
        }
        else
        {
            var entity =
                await db.VocabularyExamples.FindAsync(
                    input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu câu ví dụ.",
                input);
        }

        TempData["Message"] = "Đã lưu câu ví dụ.";

        return RedirectToAction(
            nameof(VocabularyExamples));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteVocabularyExample(
        int id)
    {
        var entity =
            await db.VocabularyExamples.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.VocabularyExamples.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(
            nameof(VocabularyExamples));
    }

    // =====================================================
    // VOCABULARY AUDIO
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> VocabularyAudios()
    {
        return View(
            await db.VocabularyAudios
                .AsNoTracking()
                .OrderBy(audio => audio.Id)
                .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> EditVocabularyAudio(
        int id = 0)
    {
        var entity = id == 0
            ? new VocabularyAudio()
            : await db.VocabularyAudios.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditVocabularyAudio(
        [Bind("Id,VocabularyId,Accent,Url")]
        VocabularyAudio input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.VocabularyAudios.Add(input);
        }
        else
        {
            var entity =
                await db.VocabularyAudios.FindAsync(input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu âm thanh.",
                input);
        }

        TempData["Message"] = "Đã lưu âm thanh.";

        return RedirectToAction(nameof(VocabularyAudios));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteVocabularyAudio(
        int id)
    {
        var entity =
            await db.VocabularyAudios.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.VocabularyAudios.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(
            nameof(VocabularyAudios));
    }

    // =====================================================
    // EXERCISES
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Exercises()
    {
        return View(
            await db.Exercises
                .AsNoTracking()
                .OrderBy(exercise => exercise.Id)
                .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> EditExercise(int id = 0)
    {
        var entity = id == 0
            ? new Exercise()
            : await db.Exercises.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditExercise(
        [Bind("Id,TopicId,Title")] Exercise input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.Exercises.Add(input);
        }
        else
        {
            var entity =
                await db.Exercises.FindAsync(input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu quiz.",
                input);
        }

        TempData["Message"] = "Đã lưu quiz.";

        return RedirectToAction(nameof(Exercises));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteExercise(int id)
    {
        var entity = await db.Exercises.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.Exercises.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(nameof(Exercises));
    }

    // =====================================================
    // QUESTIONS
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Questions()
    {
        return View(
            await db.Questions
                .AsNoTracking()
                .OrderBy(question => question.Id)
                .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int id = 0)
    {
        var entity = id == 0
            ? new Question()
            : await db.Questions.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditQuestion(
        [Bind("Id,ExerciseId,Prompt")]
        Question input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.Questions.Add(input);
        }
        else
        {
            var entity =
                await db.Questions.FindAsync(input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu câu hỏi.",
                input);
        }

        TempData["Message"] = "Đã lưu câu hỏi.";

        return RedirectToAction(nameof(Questions));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var entity = await db.Questions.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.Questions.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(nameof(Questions));
    }

    // =====================================================
    // ANSWERS
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Answers()
    {
        return View(
            await db.Answers
                .AsNoTracking()
                .OrderBy(answer => answer.Id)
                .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> EditAnswer(int id = 0)
    {
        var entity = id == 0
            ? new Answer()
            : await db.Answers.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        await Options();

        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> EditAnswer(
        [Bind("Id,QuestionId,Text,IsCorrect")]
        Answer input)
    {
        await ValidateContent(input);

        if (!ModelState.IsValid)
        {
            await Options();

            return View(input);
        }

        if (input.Id == 0)
        {
            db.Answers.Add(input);
        }
        else
        {
            var entity = await db.Answers.FindAsync(input.Id);

            if (entity is null)
            {
                return NotFound();
            }

            db.Entry(entity)
                .CurrentValues
                .SetValues(input);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return await SaveError(
                "Không thể lưu đáp án.",
                input);
        }

        TempData["Message"] = "Đã lưu đáp án.";

        return RedirectToAction(nameof(Answers));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        var entity = await db.Answers.FindAsync(id);

        if (entity is null)
        {
            return NotFound();
        }

        db.Answers.Remove(entity);

        await SaveDeleteChanges();

        return RedirectToAction(nameof(Answers));
    }

    // =====================================================
    // DELETE HELPER
    // =====================================================

    private async Task SaveDeleteChanges()
    {
        try
        {
            await db.SaveChangesAsync();

            TempData["Message"] = "Đã xoá dữ liệu.";
        }
        catch (DbUpdateException)
        {
            TempData["Message"] =
                "Không thể xoá vì dữ liệu đang được sử dụng.";
        }
    }
}