using System.Web.Helpers;
using LayoutModesComparer.Models;
using LayoutModesComparer.Repository;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Globalization;

namespace LayoutModesComparer.Pipelines.Messages
{
  public class SharedLayoutMessage : PipelineProcessorRequest<ItemContext>
  {
    public override PipelineProcessorResponseValue ProcessRequest()
    {
      var item = this.RequestContext.Item;
      var operations = CompareProvider.GetRenderingsHelper().GetRenderingsOperations(item);

      return new CustomPipelineProcessorResponseValue
      {
        Operations = Json.Encode(operations),
        Value = Translate.Text("You are editing the Shared layout of this page. All the changes you make to the shared layout will be applied to all versions of this page in every language {go to Final layout?}")
      };
    }
  }
}