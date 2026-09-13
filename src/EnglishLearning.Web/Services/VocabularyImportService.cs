using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace EnglishLearning.Web.Services;

public class VocabularyImportService(
    ApplicationDbContext db)
{
    private const long MaxFileSize =
        5 * 1024 * 1024;

    private static readonly string[] RequiredHeaders =
    [
        "GradeNumber",
        "TopicName",
        "Textbook",
        "Unit",
        "Level",
        "Word",
        "MeaningVi",
        "PartOfSpeech",
        "UkPhonetic",
        "UsPhonetic",
        "Cefr",
        "RelatedWords",
        "WordFamily",
        "Example",
        "TranslationVi",
        "UkAudioUrl",
        "UsAudioUrl"
    ];

    public async Task<VocabularyImportResultViewModel>
        ProcessAsync(
            IFormFile? file,
            bool import,
            CancellationToken cancellationToken = default)
    {
        var result =
            new VocabularyImportResultViewModel();

        if (file is null || file.Length == 0)
        {
            AddGeneralError(
                result,
                "Vui lòng chọn file CSV.");

            return result;
        }

        result.FileName =
            Path.GetFileName(file.FileName);

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".csv",
                StringComparison.OrdinalIgnoreCase))
        {
            AddGeneralError(
                result,
                "Chỉ chấp nhận file có định dạng .csv.");

            return result;
        }

        if (file.Length > MaxFileSize)
        {
            AddGeneralError(
                result,
                "File CSV không được lớn hơn 5 MB.");

            return result;
        }

        await ReadCsvAsync(
            file,
            result);

        if (result.Rows.Count == 0)
        {
            if (result.ErrorRows == 0)
            {
                AddGeneralError(
                    result,
                    "File CSV không có dữ liệu.");
            }

            return result;
        }

        await ValidateRowsAsync(
            result,
            cancellationToken);

        result.TotalRows =
            result.Rows.Count;

        result.ValidRows =
            result.Rows.Count(row =>
                row.IsValid);

        result.ErrorRows =
            result.Rows.Count(row =>
                !row.IsValid);

        if (!import || result.ErrorRows > 0)
        {
            return result;
        }

        await ImportRowsAsync(
            result,
            cancellationToken);

        return result;
    }

    private static async Task ReadCsvAsync(
        IFormFile file,
        VocabularyImportResultViewModel result)
    {
        try
        {
            await using var stream =
                file.OpenReadStream();

            using var reader =
                new StreamReader(
                    stream,
                    new UTF8Encoding(
                        false,
                        true),
                    detectEncodingFromByteOrderMarks: true);

            var configuration =
                new CsvConfiguration(
                    CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    BadDataFound = null,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    PrepareHeaderForMatch = args =>
                        args.Header.Trim()
                };

            using var csv =
                new CsvReader(
                    reader,
                    configuration);

            if (!await csv.ReadAsync())
            {
                AddGeneralError(
                    result,
                    "Không đọc được file CSV.");

                return;
            }

            csv.ReadHeader();

            string[] headers =
                csv.HeaderRecord ??
                [];

            var headerSet =
                headers.ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

            var missingHeaders =
                RequiredHeaders
                    .Where(header =>
                        !headerSet.Contains(header))
                    .ToList();

            if (missingHeaders.Count > 0)
            {
                AddGeneralError(
                    result,
                    "File thiếu các cột: " +
                    string.Join(
                        ", ",
                        missingHeaders));

                return;
            }

            while (await csv.ReadAsync())
            {
                int rowNumber =
                    (int)csv.Context.Parser.Row;

                var data =
                    new VocabularyImportCsvRow
                    {
                        GradeNumber =
                            Get(csv, "GradeNumber"),

                        TopicName =
                            Get(csv, "TopicName"),

                        Textbook =
                            Get(csv, "Textbook"),

                        Unit =
                            Get(csv, "Unit"),

                        Level =
                            Get(csv, "Level"),

                        Word =
                            Get(csv, "Word"),

                        MeaningVi =
                            Get(csv, "MeaningVi"),

                        PartOfSpeech =
                            Get(csv, "PartOfSpeech"),

                        UkPhonetic =
                            Get(csv, "UkPhonetic"),

                        UsPhonetic =
                            Get(csv, "UsPhonetic"),

                        Cefr =
                            Get(csv, "Cefr"),

                        RelatedWords =
                            Get(csv, "RelatedWords"),

                        WordFamily =
                            Get(csv, "WordFamily"),

                        Example =
                            Get(csv, "Example"),

                        TranslationVi =
                            Get(csv, "TranslationVi"),

                        UkAudioUrl =
                            Get(csv, "UkAudioUrl"),

                        UsAudioUrl =
                            Get(csv, "UsAudioUrl")
                    };

                Normalize(data);

                if (IsEmpty(data))
                {
                    continue;
                }

                result.Rows.Add(
                    new VocabularyImportRowResult
                    {
                        RowNumber = rowNumber,
                        Data = data
                    });
            }
        }
        catch (DecoderFallbackException)
        {
            AddGeneralError(
                result,
                "File không phải UTF-8 hợp lệ. " +
                "Hãy lưu lại dưới dạng CSV UTF-8.");
        }
        catch (CsvHelperException exception)
        {
            AddGeneralError(
                result,
                "Không thể đọc CSV: " +
                exception.Message);
        }
        catch (IOException)
        {
            AddGeneralError(
                result,
                "Không thể đọc file CSV.");
        }
    }

    private async Task ValidateRowsAsync(
        VocabularyImportResultViewModel result,
        CancellationToken cancellationToken)
    {
        var grades =
            await db.Grades
                .AsNoTracking()
                .ToListAsync(
                    cancellationToken);

        var topics =
            await db.Topics
                .AsNoTracking()
                .ToListAsync(
                    cancellationToken);

        var sources =
            await db.TopicSources
                .AsNoTracking()
                .ToListAsync(
                    cancellationToken);

        var existingWords =
            await db.Vocabularies
                .AsNoTracking()
                .Where(vocabulary =>
                    vocabulary.TopicId.HasValue)
                .Select(vocabulary => new
                {
                    TopicId =
                        vocabulary.TopicId!.Value,

                    vocabulary.Word
                })
                .ToListAsync(
                    cancellationToken);

        var existingKeys =
            existingWords
                .Select(item =>
                    CreateWordKey(
                        item.TopicId,
                        item.Word))
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        var fileKeys =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var row in result.Rows)
        {
            var data = row.Data;

            ValidateRequired(
                row,
                data.TopicName,
                "TopicName");

            ValidateRequired(
                row,
                data.Textbook,
                "Textbook");

            ValidateRequired(
                row,
                data.Unit,
                "Unit");

            ValidateRequired(
                row,
                data.Level,
                "Level");

            ValidateRequired(
                row,
                data.Word,
                "Word");

            ValidateRequired(
                row,
                data.MeaningVi,
                "MeaningVi");

            ValidateLength(
                row,
                data.TopicName,
                200,
                "TopicName");

            ValidateLength(
                row,
                data.Textbook,
                200,
                "Textbook");

            ValidateLength(
                row,
                data.Unit,
                100,
                "Unit");

            ValidateLength(
                row,
                data.Word,
                100,
                "Word");

            ValidateLength(
                row,
                data.MeaningVi,
                500,
                "MeaningVi");

            ValidateLength(
                row,
                data.PartOfSpeech,
                50,
                "PartOfSpeech");

            ValidateLength(
                row,
                data.UkPhonetic,
                100,
                "UkPhonetic");

            ValidateLength(
                row,
                data.UsPhonetic,
                100,
                "UsPhonetic");

            ValidateLength(
                row,
                data.RelatedWords,
                500,
                "RelatedWords");

            ValidateLength(
                row,
                data.WordFamily,
                500,
                "WordFamily");

            if (!int.TryParse(
                    data.GradeNumber,
                    out int gradeNumber) ||
                gradeNumber < 1 ||
                gradeNumber > 12)
            {
                row.Errors.Add(
                    "GradeNumber phải từ 1 đến 12.");

                continue;
            }

            var grade =
                grades.FirstOrDefault(item =>
                    item.Number == gradeNumber);

            if (grade is null)
            {
                row.Errors.Add(
                    $"Không tồn tại Lớp {gradeNumber}.");

                continue;
            }

            var topic =
                topics.FirstOrDefault(item =>
                    item.GradeId == grade.Id &&
                    string.Equals(
                        item.Name,
                        data.TopicName,
                        StringComparison.OrdinalIgnoreCase));

            if (topic is null)
            {
                row.Errors.Add(
                    "Không tìm thấy chủ đề trong lớp đã chọn.");

                continue;
            }

            var source =
                sources.FirstOrDefault(item =>
                    item.TopicId == topic.Id &&
                    string.Equals(
                        item.Textbook,
                        data.Textbook,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        item.Unit,
                        data.Unit,
                        StringComparison.OrdinalIgnoreCase));

            if (source is null)
            {
                row.Errors.Add(
                    "Không tìm thấy nguồn SGK/Unit " +
                    "thuộc đúng chủ đề.");

                continue;
            }

            if (!Enum.TryParse<VocabularyLevel>(
                    data.Level,
                    true,
                    out _))
            {
                row.Errors.Add(
                    "Level chỉ nhận Core hoặc Advanced.");
            }

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

            if (!allowedCefr.Contains(data.Cefr))
            {
                row.Errors.Add(
                    "CEFR phải từ A1 đến C2 hoặc để trống.");
            }

            ValidateHttpsUrl(
                row,
                data.UkAudioUrl,
                "UkAudioUrl");

            ValidateHttpsUrl(
                row,
                data.UsAudioUrl,
                "UsAudioUrl");

            if (string.IsNullOrWhiteSpace(data.Example) &&
                !string.IsNullOrWhiteSpace(
                    data.TranslationVi))
            {
                row.Errors.Add(
                    "Có TranslationVi nhưng thiếu Example.");
            }

            string wordKey =
                CreateWordKey(
                    topic.Id,
                    data.Word);

            if (existingKeys.Contains(wordKey))
            {
                row.Errors.Add(
                    "Từ đã tồn tại trong chủ đề.");
            }

            if (!fileKeys.Add(wordKey))
            {
                row.Errors.Add(
                    "Từ bị trùng trong chính file CSV.");
            }
        }
    }

    private async Task ImportRowsAsync(
        VocabularyImportResultViewModel result,
        CancellationToken cancellationToken)
    {
        var grades =
            await db.Grades
                .AsNoTracking()
                .ToListAsync(
                    cancellationToken);

        var topics =
            await db.Topics
                .AsNoTracking()
                .ToListAsync(
                    cancellationToken);

        var sources =
            await db.TopicSources
                .AsNoTracking()
                .ToListAsync(
                    cancellationToken);

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            foreach (var row in result.Rows)
            {
                var data = row.Data;

                int gradeNumber =
                    int.Parse(
                        data.GradeNumber,
                        CultureInfo.InvariantCulture);

                var grade =
                    grades.Single(item =>
                        item.Number == gradeNumber);

                var topic =
                    topics.Single(item =>
                        item.GradeId == grade.Id &&
                        string.Equals(
                            item.Name,
                            data.TopicName,
                            StringComparison.OrdinalIgnoreCase));

                var source =
                    sources.Single(item =>
                        item.TopicId == topic.Id &&
                        string.Equals(
                            item.Textbook,
                            data.Textbook,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            item.Unit,
                            data.Unit,
                            StringComparison.OrdinalIgnoreCase));

                Enum.TryParse<VocabularyLevel>(
                    data.Level,
                    true,
                    out var level);

                var vocabulary =
                    new Vocabulary
                    {
                        TopicId = topic.Id,
                        TopicSourceId = source.Id,
                        Level = level,
                        Word = data.Word,
                        MeaningVi = data.MeaningVi,
                        PartOfSpeech =
                            data.PartOfSpeech,
                        UkPhonetic =
                            data.UkPhonetic,
                        UsPhonetic =
                            data.UsPhonetic,
                        Cefr = data.Cefr,
                        RelatedWords =
                            data.RelatedWords,
                        WordFamily =
                            data.WordFamily
                    };

                if (!string.IsNullOrWhiteSpace(
                        data.Example))
                {
                    vocabulary.Examples.Add(
                        new VocabularyExample
                        {
                            Sentence =
                                data.Example,

                            TranslationVi =
                                data.TranslationVi
                        });
                }

                if (!string.IsNullOrWhiteSpace(
                        data.UkAudioUrl))
                {
                    vocabulary.Audios.Add(
                        new VocabularyAudio
                        {
                            Accent = "UK",
                            Url = data.UkAudioUrl
                        });
                }

                if (!string.IsNullOrWhiteSpace(
                        data.UsAudioUrl))
                {
                    vocabulary.Audios.Add(
                        new VocabularyAudio
                        {
                            Accent = "US",
                            Url = data.UsAudioUrl
                        });
                }

                db.Vocabularies.Add(vocabulary);
            }

            await db.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            result.ImportedRows =
                result.Rows.Count;

            result.WasImported = true;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private static string Get(
        CsvReader csv,
        string header)
    {
        return csv.GetField<string>(header)?
            .Trim() ?? string.Empty;
    }

    private static void Normalize(
        VocabularyImportCsvRow row)
    {
        row.GradeNumber =
            row.GradeNumber.Trim();

        row.TopicName =
            row.TopicName.Trim();

        row.Textbook =
            row.Textbook.Trim();

        row.Unit =
            row.Unit.Trim();

        row.Level =
            row.Level.Trim();

        row.Word =
            row.Word.Trim();

        row.MeaningVi =
            row.MeaningVi.Trim();

        row.PartOfSpeech =
            row.PartOfSpeech.Trim();

        row.UkPhonetic =
            row.UkPhonetic.Trim();

        row.UsPhonetic =
            row.UsPhonetic.Trim();

        row.Cefr =
            row.Cefr
                .Trim()
                .ToUpperInvariant();

        row.RelatedWords =
            row.RelatedWords.Trim();

        row.WordFamily =
            row.WordFamily.Trim();

        row.Example =
            row.Example.Trim();

        row.TranslationVi =
            row.TranslationVi.Trim();

        row.UkAudioUrl =
            row.UkAudioUrl.Trim();

        row.UsAudioUrl =
            row.UsAudioUrl.Trim();
    }

    private static bool IsEmpty(
        VocabularyImportCsvRow row)
    {
        return string.IsNullOrWhiteSpace(
                   row.Word) &&
               string.IsNullOrWhiteSpace(
                   row.MeaningVi) &&
               string.IsNullOrWhiteSpace(
                   row.TopicName);
    }

    private static string CreateWordKey(
        int topicId,
        string word)
    {
        return topicId +
               "|" +
               word.Trim().ToUpperInvariant();
    }

    private static void ValidateRequired(
        VocabularyImportRowResult row,
        string value,
        string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            row.Errors.Add(
                $"{field} không được để trống.");
        }
    }

    private static void ValidateLength(
        VocabularyImportRowResult row,
        string value,
        int maximum,
        string field)
    {
        if (value.Length > maximum)
        {
            row.Errors.Add(
                $"{field} không được vượt quá " +
                $"{maximum} ký tự.");
        }
    }

    private static void ValidateHttpsUrl(
        VocabularyImportRowResult row,
        string value,
        string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        bool valid =
            Uri.TryCreate(
                value,
                UriKind.Absolute,
                out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps;

        if (!valid)
        {
            row.Errors.Add(
                $"{field} phải là URL HTTPS.");
        }
    }

    private static void AddGeneralError(
        VocabularyImportResultViewModel result,
        string message)
    {
        result.Rows.Add(
            new VocabularyImportRowResult
            {
                RowNumber = 0,
                Errors = [message]
            });

        result.TotalRows =
            result.Rows.Count;

        result.ErrorRows =
            result.Rows.Count;
    }
}