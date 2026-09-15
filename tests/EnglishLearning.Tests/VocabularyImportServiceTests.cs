using System.Text;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EnglishLearning.Tests;

public class VocabularyImportServiceTests
{
    private const string Header =
        "GradeNumber,TopicName,Textbook,Unit,Level," +
        "Word,MeaningVi,PartOfSpeech,UkPhonetic," +
        "UsPhonetic,Cefr,RelatedWords,WordFamily," +
        "Example,TranslationVi,UkAudioUrl,UsAudioUrl";

    private const string ValidRow =
        "6,School,Test Book,Unit 1,Core,principal," +
        "hiệu trưởng,noun,/principal/,/principal/," +
        "A2,school,principal," +
        "\"The principal is kind.\"," +
        "\"Hiệu trưởng rất tốt.\"," +
        "https://example.com/uk.mp3," +
        "https://example.com/us.mp3";

    [Fact]
    public async Task PreviewValidCsvDoesNotSaveData()
    {
        await using var database =
            await CreateDatabaseAsync();

        var service =
            new VocabularyImportService(
                database.Context);

        var file =
            CreateCsvFile(
                Header +
                Environment.NewLine +
                ValidRow);

        var result =
            await service.ProcessAsync(
                file,
                import: false);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(1, result.ValidRows);
        Assert.Equal(0, result.ErrorRows);
        Assert.Equal(0, result.ImportedRows);
        Assert.False(result.WasImported);

        Assert.Equal(
            0,
            await database.Context
                .Vocabularies
                .CountAsync());
    }

    [Fact]
    public async Task ImportValidCsvSavesVocabularyAndChildren()
    {
        await using var database =
            await CreateDatabaseAsync();

        var service =
            new VocabularyImportService(
                database.Context);

        var file =
            CreateCsvFile(
                Header +
                Environment.NewLine +
                ValidRow);

        var result =
            await service.ProcessAsync(
                file,
                import: true);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(1, result.ValidRows);
        Assert.Equal(0, result.ErrorRows);
        Assert.Equal(1, result.ImportedRows);
        Assert.True(result.WasImported);

        var vocabulary =
            await database.Context
                .Vocabularies
                .Include(item => item.Examples)
                .Include(item => item.Audios)
                .SingleAsync();

        Assert.Equal(
            "principal",
            vocabulary.Word);

        Assert.Equal(
            "hiệu trưởng",
            vocabulary.MeaningVi);

        Assert.Equal(
            VocabularyLevel.Core,
            vocabulary.Level);

        Assert.Equal(
            "A2",
            vocabulary.Cefr);

        Assert.Single(
            vocabulary.Examples);

        Assert.Equal(
            "The principal is kind.",
            vocabulary.Examples[0].Sentence);

        Assert.Equal(
            2,
            vocabulary.Audios.Count);

        Assert.Contains(
            vocabulary.Audios,
            audio =>
                audio.Accent == "UK");

        Assert.Contains(
            vocabulary.Audios,
            audio =>
                audio.Accent == "US");
    }

    [Fact]
    public async Task RejectsWordAlreadyInTopic()
    {
        await using var database =
            await CreateDatabaseAsync();

        var topic =
            await database.Context
                .Topics
                .SingleAsync();

        var source =
            await database.Context
                .TopicSources
                .SingleAsync();

        database.Context.Vocabularies.Add(
            new Vocabulary
            {
                TopicId = topic.Id,
                TopicSourceId = source.Id,
                Word = "principal",
                MeaningVi = "hiệu trưởng",
                Level = VocabularyLevel.Core
            });

        await database.Context.SaveChangesAsync();

        var service =
            new VocabularyImportService(
                database.Context);

        var file =
            CreateCsvFile(
                Header +
                Environment.NewLine +
                ValidRow);

        var result =
            await service.ProcessAsync(
                file,
                import: true);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(0, result.ValidRows);
        Assert.Equal(1, result.ErrorRows);
        Assert.Equal(0, result.ImportedRows);
        Assert.False(result.WasImported);

        Assert.Equal(
            1,
            await database.Context
                .Vocabularies
                .CountAsync());

        Assert.Contains(
            result.Rows.SelectMany(row => row.Errors),
            error =>
                error.Contains(
                    "đã tồn tại",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InvalidRowPreventsEntireFileImport()
    {
        await using var database =
            await CreateDatabaseAsync();

        const string invalidRow =
            "6,School,Wrong Book,Unit 1,Core,library," +
            "thư viện,noun,,,A1,,,," +
            ",,https://example.com/us.mp3";

        var service =
            new VocabularyImportService(
                database.Context);

        var file =
            CreateCsvFile(
                Header +
                Environment.NewLine +
                ValidRow +
                Environment.NewLine +
                invalidRow);

        var result =
            await service.ProcessAsync(
                file,
                import: true);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.ValidRows);
        Assert.Equal(1, result.ErrorRows);
        Assert.Equal(0, result.ImportedRows);
        Assert.False(result.WasImported);

        Assert.Equal(
            0,
            await database.Context
                .Vocabularies
                .CountAsync());
    }

    [Fact]
    public async Task RejectsCsvWithMissingHeaders()
    {
        await using var database =
            await CreateDatabaseAsync();

        var service =
            new VocabularyImportService(
                database.Context);

        var file =
            CreateCsvFile(
                "GradeNumber,Word" +
                Environment.NewLine +
                "6,student");

        var result =
            await service.ProcessAsync(
                file,
                import: true);

        Assert.True(result.ErrorRows > 0);
        Assert.Equal(0, result.ImportedRows);
        Assert.False(result.WasImported);

        Assert.Contains(
            result.Rows.SelectMany(row => row.Errors),
            error =>
                error.Contains(
                    "thiếu các cột",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Equal(
            0,
            await database.Context
                .Vocabularies
                .CountAsync());
    }

    private static FormFile CreateCsvFile(
        string content)
    {
        byte[] bytes =
            Encoding.UTF8.GetBytes(content);

        var stream =
            new MemoryStream(bytes);

        return new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            "vocabulary-test.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }

    private static async Task<TestDatabase>
        CreateDatabaseAsync()
    {
        var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<
                ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

        var context =
            new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var grade =
            new Grade
            {
                Number = 6,
                Name = "Lớp 6"
            };

        var topic =
            new Topic
            {
                Grade = grade,
                Name = "School",
                Description = "Test topic",
                IsPublished = true,
                IsDemo = false
            };

        topic.Sources.Add(
            new TopicSource
            {
                Textbook = "Test Book",
                OriginalTopicName = "School",
                Unit = "Unit 1"
            });

        context.Topics.Add(topic);

        await context.SaveChangesAsync(); ;

        return new TestDatabase(
            connection,
            context);
    }

    private sealed class TestDatabase(
        SqliteConnection connection,
        ApplicationDbContext context)
        : IAsyncDisposable
    {
        public ApplicationDbContext Context { get; } =
            context;

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}