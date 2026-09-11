using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace EnglishLearning.Infrastructure.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):IdentityDbContext<ApplicationUser>(options)
{
 public DbSet<Grade> Grades => Set<Grade>();
 public DbSet<Topic> Topics => Set<Topic>();
 public DbSet<TopicSource> TopicSources => Set<TopicSource>();
 public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
 public DbSet<VocabularyExample> VocabularyExamples => Set<VocabularyExample>();
 public DbSet<VocabularyAudio> VocabularyAudios => Set<VocabularyAudio>();
 public DbSet<Exercise> Exercises => Set<Exercise>();
 public DbSet<Question> Questions => Set<Question>();
 public DbSet<Answer> Answers => Set<Answer>();
 public DbSet<UserVocabularyProgress> UserVocabularyProgresses => Set<UserVocabularyProgress>();
 public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
 public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();
 public DbSet<StudyHistory> StudyHistories => Set<StudyHistory>();
 protected override void OnModelCreating(ModelBuilder b)
 {
  base.OnModelCreating(b);
  b.Entity<Grade>().HasIndex(x=>x.Number).IsUnique();
  b.Entity<Vocabulary>().HasIndex(x=>x.Word);
  b.Entity<UserVocabularyProgress>().HasIndex(x=>new{x.UserId,x.VocabularyId}).IsUnique();
  b.Entity<TopicSource>().HasOne(x=>x.Topic).WithMany(x=>x.Sources).HasForeignKey(x=>x.TopicId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Vocabulary>().HasOne(x=>x.TopicSource).WithMany().HasForeignKey(x=>x.TopicSourceId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Vocabulary>().HasOne(x=>x.Topic).WithMany(x=>x.Vocabulary).HasForeignKey(x=>x.TopicId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Topic>().HasOne(x=>x.Grade).WithMany(x=>x.Topics).HasForeignKey(x=>x.GradeId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Exercise>().HasOne(x=>x.Topic).WithMany().HasForeignKey(x=>x.TopicId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<QuizAttempt>().HasOne(x=>x.Exercise).WithMany().HasForeignKey(x=>x.ExerciseId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<UserVocabularyProgress>().HasOne(x=>x.Vocabulary).WithMany().HasForeignKey(x=>x.VocabularyId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<UserVocabularyProgress>().HasOne<ApplicationUser>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<QuizAttempt>().HasOne<ApplicationUser>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<StudyHistory>().HasOne<ApplicationUser>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);
 }
}
