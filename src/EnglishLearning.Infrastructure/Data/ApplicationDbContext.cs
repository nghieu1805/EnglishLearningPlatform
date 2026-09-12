using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Grade> Grades =>
        Set<Grade>();

    public DbSet<Topic> Topics =>
        Set<Topic>();

    public DbSet<TopicSource> TopicSources =>
        Set<TopicSource>();

    public DbSet<Vocabulary> Vocabularies =>
        Set<Vocabulary>();

    public DbSet<VocabularyExample> VocabularyExamples =>
        Set<VocabularyExample>();

    public DbSet<VocabularyAudio> VocabularyAudios =>
        Set<VocabularyAudio>();

    public DbSet<Exercise> Exercises =>
        Set<Exercise>();

    public DbSet<Question> Questions =>
        Set<Question>();

    public DbSet<Answer> Answers =>
        Set<Answer>();

    public DbSet<UserVocabularyProgress> UserVocabularyProgresses =>
        Set<UserVocabularyProgress>();

    public DbSet<QuizAttempt> QuizAttempts =>
        Set<QuizAttempt>();

    public DbSet<QuizAnswer> QuizAnswers =>
        Set<QuizAnswer>();

    public DbSet<StudyHistory> StudyHistories =>
        Set<StudyHistory>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureGrade(builder);
        ConfigureTopic(builder);
        ConfigureTopicSource(builder);
        ConfigureVocabulary(builder);
        ConfigureProgress(builder);
        ConfigureExercise(builder);
        ConfigureQuizAttempt(builder);
        ConfigureStudyHistory(builder);
    }

    private static void ConfigureGrade(
        ModelBuilder builder)
    {
        builder.Entity<Grade>()
            .HasIndex(grade => grade.Number)
            .IsUnique();
    }

    private static void ConfigureTopic(
        ModelBuilder builder)
    {
        builder.Entity<Topic>()
            .HasOne(topic => topic.Grade)
            .WithMany(grade => grade.Topics)
            .HasForeignKey(topic => topic.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureTopicSource(
        ModelBuilder builder)
    {
        builder.Entity<TopicSource>()
            .HasIndex(source => source.TopicId)
            .HasDatabaseName(
                "IX_TopicSources_TopicId");

        builder.Entity<TopicSource>()
            .HasIndex(source => new
            {
                source.TopicId,
                source.Textbook,
                source.Unit
            })
            .IsUnique();

        builder.Entity<TopicSource>()
            .HasOne(source => source.Topic)
            .WithMany(topic => topic.Sources)
            .HasForeignKey(source => source.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureVocabulary(
        ModelBuilder builder)
    {
        builder.Entity<Vocabulary>()
            .HasIndex(vocabulary => vocabulary.Word);

        builder.Entity<Vocabulary>()
            .HasIndex(vocabulary =>
                vocabulary.TopicId)
            .HasDatabaseName(
                "IX_Vocabularies_TopicId");

        builder.Entity<Vocabulary>()
            .HasIndex(vocabulary => new
            {
                vocabulary.TopicId,
                vocabulary.Word
            })
            .IsUnique();

        builder.Entity<Vocabulary>()
            .Property(vocabulary =>
                vocabulary.Level)
            .HasConversion<int>();

        builder.Entity<Vocabulary>()
            .HasOne(vocabulary =>
                vocabulary.TopicSource)
            .WithMany()
            .HasForeignKey(vocabulary =>
                vocabulary.TopicSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Vocabulary>()
            .HasOne(vocabulary =>
                vocabulary.Topic)
            .WithMany(topic =>
                topic.Vocabulary)
            .HasForeignKey(vocabulary =>
                vocabulary.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureProgress(
        ModelBuilder builder)
    {
        builder.Entity<UserVocabularyProgress>()
            .HasIndex(progress => new
            {
                progress.UserId,
                progress.VocabularyId
            })
            .IsUnique();

        builder.Entity<UserVocabularyProgress>()
            .HasOne(progress =>
                progress.Vocabulary)
            .WithMany()
            .HasForeignKey(progress =>
                progress.VocabularyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserVocabularyProgress>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(progress =>
                progress.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureExercise(
        ModelBuilder builder)
    {
        builder.Entity<Exercise>()
            .HasOne(exercise =>
                exercise.Topic)
            .WithMany()
            .HasForeignKey(exercise =>
                exercise.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureQuizAttempt(
        ModelBuilder builder)
    {
        builder.Entity<QuizAttempt>()
            .HasOne(attempt =>
                attempt.Exercise)
            .WithMany()
            .HasForeignKey(attempt =>
                attempt.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizAttempt>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(attempt =>
                attempt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureStudyHistory(
        ModelBuilder builder)
    {
        builder.Entity<StudyHistory>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(history =>
                history.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}