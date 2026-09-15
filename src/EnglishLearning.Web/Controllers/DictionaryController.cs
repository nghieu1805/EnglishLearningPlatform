using EnglishLearning.Application.Interfaces;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Web.Services;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishLearning.Web.Controllers;

public class DictionaryController(
    ILearningRepository repository,
    DictionaryApiService dictionaryApi) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? q,
        CancellationToken cancellationToken)
    {
        string query = q?.Trim() ?? string.Empty;

        var model = new DictionaryPage
        {
            Query = query,
            HasSearched =
                !string.IsNullOrWhiteSpace(query)
        };

        if (!model.HasSearched)
        {
            return View(model);
        }

        if (query.Length > 100)
        {
            model.ErrorMessage =
                "Từ khóa không được dài quá 100 ký tự.";

            return View(model);
        }

        model.Results =
            await repository.SearchAsync(query);

        if (model.Results.Count > 0)
        {
            await LoadStatusesAsync(model);

            return View(model);
        }

        model.ExternalResult =
            await dictionaryApi.SearchAsync(
                query,
                cancellationToken);

        if (!model.ExternalResult.IsAvailable)
        {
            model.ErrorMessage =
                model.ExternalResult.ErrorMessage;
        }

        return View(model);
    }

    private async Task LoadStatusesAsync(
        DictionaryPage model)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var progress =
            await repository.ProgressAsync(userId);

        var resultIds =
            model.Results
                .Select(result => result.Id)
                .ToHashSet();

        model.Statuses = progress
            .Where(item =>
                resultIds.Contains(
                    item.VocabularyId))
            .ToDictionary(
                item => item.VocabularyId,
                item => item.Status);
    }
}