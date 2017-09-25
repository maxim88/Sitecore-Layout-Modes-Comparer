using System;
using System.Collections.Specialized;
using LayoutModesComparer.Repository;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.StringExtensions;
using Sitecore.Web.UI.Sheer;
using Version = Sitecore.Data.Version;

namespace LayoutModesComparer.Commands
{
  [Serializable, UsedImplicitly]
  public class ResetComponentsChanges : WebEditCommand 
  {
    public override void Execute(CommandContext context)
    {
      try
      {
        Assert.ArgumentNotNull((object)context, "context");
        NameValueCollection parameters = new NameValueCollection();
        var item = context.Items[0];
        if (item == null)
        {
          return;
        }

        parameters["database"] = item.Database.Name; 

        // Get Language
        var itemUri = ItemUri.ParseQueryString();
        var language = context.Parameters["language"];

        if (itemUri == null)
        {
          Context.ClientPage.ClientResponse.Alert(Translate.Text("The Reset of the Component Changes is FAILED!"));
          return;
        }

        if (String.IsNullOrEmpty(language))
        {
          language = itemUri.Language.ToString();
        }

        parameters["language"] = language;

        // Store the custom parameters 
        parameters["referenceId"] = context.Parameters["referenceId"] ?? String.Empty;
        parameters["version"] = itemUri.Version.Number.ToString();
        parameters["itemId"] =  itemUri.ItemID.ToString();
        Context.ClientPage.Start((object)this, "Run", parameters);
      }
      catch (Exception ex)
      {
        Context.ClientPage.ClientResponse.Alert(Translate.Text("The Reset of the Component Changes is FAILED!"));
        Log.Error(ex.Message, ex.InnerException, this); 
      } 
    }
      
    protected static void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, "args"); 

      try
      {
        var dbName = args.Parameters["database"];

        Version version;
        Language lan;
        Guid uid, itemId;
        if (!Guid.TryParse(args.Parameters["referenceId"], out uid) ||
          !Guid.TryParse(args.Parameters["itemId"], out itemId) ||
          !Version.TryParse(args.Parameters["version"], out version) ||
          !Language.TryParse(args.Parameters["language"], out lan))
        {
          Context.ClientPage.ClientResponse.Alert(Translate.Text("The Reset of the Component Changes is FAILED!"));
          return;
        }

        if (args.IsPostBack)
        {
          if (!args.HasResult || args.Result != "yes")
          {
            return;
          }

          var item = Database.GetDatabase(dbName).GetItem(new ID(itemId), lan, version);

          CompareProvider.GetRenderingsHelper().ResetRendering(item, new ID(uid));
          Context.ClientPage.ClientResponse.Alert(Translate.Text("The Reset of the Component Changes is SUCCEEDED on the FINAL Layout for the Item Version {1} of the '{0}' language!").FormatWith(args.Parameters["language"].ToUpper(), args.Parameters["version"]));
        }
        else
        {
          Context.ClientPage.ClientResponse.Confirm(Translate.Text("Do you want to RESET all Component Changes on the FINAL Layout for the Item Version {1} of the '{0}' language?").FormatWith(args.Parameters["language"].ToUpper(), args.Parameters["version"]));
          args.WaitForPostBack();
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.Message, ex, new ResetComponentsChanges());
      } 
    }

    public override CommandState QueryState(CommandContext context)
    {
      Guid uid;
      if (Context.Item == null || !Guid.TryParse(context.Parameters["referenceId"], out uid))
      {
        return CommandState.Hidden;
      }

      return CompareProvider.GetRenderingsHelper().IsChanged(Context.Item, new ID(uid)) ? CommandState.Enabled : CommandState.Hidden;
    }
  }
}