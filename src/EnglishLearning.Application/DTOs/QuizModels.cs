namespace EnglishLearning.Application.DTOs;
public record ChoiceDto(int Id,string Text);
public record QuestionDto(int Id,string Prompt,List<ChoiceDto> Choices);
public record QuizDto(int Id,string Title,List<QuestionDto> Questions);
