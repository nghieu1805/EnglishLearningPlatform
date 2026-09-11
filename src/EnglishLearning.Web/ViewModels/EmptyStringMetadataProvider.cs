using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
namespace EnglishLearning.Web.ViewModels;
public sealed class EmptyStringMetadataProvider:IDisplayMetadataProvider
{
 public void CreateDisplayMetadata(DisplayMetadataProviderContext context){context.DisplayMetadata.ConvertEmptyStringToNull=false;}
}
