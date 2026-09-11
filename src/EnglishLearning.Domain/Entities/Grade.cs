using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class Grade
{
public int Id { get; set; }
[Range(1,12)] public int Number { get; set; }
[Required,MaxLength(80)] public string Name { get; set; } = "";
public List<Topic> Topics { get; set; } = [];
}
