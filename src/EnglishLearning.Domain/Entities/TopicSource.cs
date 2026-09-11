using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class TopicSource
{
public int Id { get; set; }
public int TopicId { get; set; }
public Topic? Topic { get; set; }
[Required,MaxLength(200)] public string Textbook { get; set; } = "";
[Required,MaxLength(160)] public string OriginalTopicName { get; set; } = "";
[Required,MaxLength(100)] public string Unit { get; set; } = "";
}
