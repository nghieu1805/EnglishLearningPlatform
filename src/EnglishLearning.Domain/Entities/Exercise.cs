using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class Exercise
{
public int Id { get; set; }
public int TopicId { get; set; }
public Topic? Topic { get; set; }
[Required,MaxLength(160)] public string Title { get; set; } = "";
public List<Question> Questions { get; set; } = [];
}
