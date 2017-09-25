using System.Web.Helpers;
using LayoutModesComparer.Models;
using LayoutModesComparer.Repository;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Globalization;

namespace LayoutModesComparer.Pipelines.Messages
{
  public class FinalLayoutMessage : PipelineProcessorRequest<ItemContext>
  {
    public override PipelineProcessorResponseValue ProcessRequest()
    {
      var item = this.RequestContext.Item;
      var operations = CompareProvider.GetRenderingsHelper().GetRenderingsOperations(item);
      return new CustomPipelineProcessorResponseValue
      {
        Operations = Json.Encode(operations),
        Value = Translate.Text("You are editing the FINAL layout of this page. All the changes you make to the FINAL layout will be applied only to the current version of this page in current language {go to Shared layout?}")
      };
    }
  }
}