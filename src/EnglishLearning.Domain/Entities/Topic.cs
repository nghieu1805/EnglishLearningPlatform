using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class Topic
{
public int Id { get; set; }
public int GradeId { get; set; }
public Grade? Grade { get; set; }
[Required,MaxLength(160)] public string Name { get; set; } = "";
[MaxLength(1000)] public string Description { get; set; } = "";
public int SortOrder { get; set; }
public bool IsPublished { get; set; }
public bool IsDemo { get; set; }
public List<TopicSource> Sources { get; set; } = [];
public List<Vocabulary> Vocabulary { get; set; } = [];
}
