using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using Xunit;
namespace EnglishLearning.Tests;
public class QuizScoringTests
{
 private static Exercise Quiz()=>new(){Id=9,Questions=[new(){Id=1,Prompt="First",Answers=[new(){Id=11,Text="Right",IsCorrect=true},new(){Id=12,Text="Wrong"}]},new(){Id=2,Prompt="Second",Answers=[new(){Id=21,Text="Yes",IsCorrect=true},new(){Id=22,Text="No"}]}]};
 [Fact] public void ScoresCorrectAndIncorrectAnswers(){var a=QuizService.Score(Quiz(),"student-a",new Dictionary<int,int>{{1,11},{2,22}});Assert.Equal(2,a.Total);Assert.Equal(1,a.Correct);Assert.Equal("student-a",a.UserId);Assert.Equal("Yes",a.Answers[1].CorrectSnapshot);}
 [Fact] public void UnansweredQuestionsCountAsIncorrect(){var a=QuizService.Score(Quiz(),"student-a",new Dictionary<int,int>{{1,11}});Assert.Equal(2,a.Total);Assert.Equal(1,a.Correct);Assert.Null(a.Answers[1].SelectedAnswerId);}
 [Fact] public void RejectsAnswerFromAnotherQuestion(){Assert.Throws<ArgumentException>(()=>QuizService.Score(Quiz(),"u",new Dictionary<int,int>{{1,21}}));}
 [Fact] public void RejectsUnknownQuestion(){Assert.Throws<ArgumentException>(()=>QuizService.Score(Quiz(),"u",new Dictionary<int,int>{{99,11}}));}
 [Fact] public void RejectsAmbiguousAnswerKey(){var e=Quiz();e.Questions[0].Answers[1].IsCorrect=true;Assert.False(QuizService.IsValid(e));Assert.Throws<ArgumentException>(()=>QuizService.Score(e,"u",new Dictionary<int,int>()));}
 [Fact] public void RejectsEmptyQuiz(){Assert.False(QuizService.IsValid(new Exercise()));}
 [Fact] public void KeepsHistoricalTextWhenContentChanges(){var e=Quiz();var a=QuizService.Score(e,"u",new Dictionary<int,int>{{1,11},{2,21}});e.Questions[0].Prompt="Changed";e.Questions[0].Answers[0].Text="Changed";Assert.Equal("First",a.Answers[0].PromptSnapshot);Assert.Equal("Right",a.Answers[0].CorrectSnapshot);}
}
