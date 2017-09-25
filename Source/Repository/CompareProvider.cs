using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Xml;
using LayoutModesComparer.Models;
using LayoutsComparer;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.StringExtensions;
using Version = Sitecore.Data.Version;

namespace LayoutModesComparer.Repository
{
  public class CompareProvider
  {
    private const string noValue = "null";

    private static readonly CompareProvider _instance = new CompareProvider();
    private static NameValueCollection settings = new NameValueCollection();

    public static CompareProvider GetRenderingsHelper()
    {
      InitOperationSettings();
      return _instance;
    }

    protected static void InitOperationSettings()
    {
      var settingsItem = Database.GetDatabase("core").GetItem(ItemIds.ColorsSettings);
      if (settingsItem == null)
      {
        return;
      }

      settings = ((NameValueListField)settingsItem.Fields[Fields.Color]).NameValues; 
    }


    public virtual List<Rendering> GetRenderings(string renderingsField, bool isLayout=false)
    {
      var renderings = new List<Rendering>();
      try
      {
        if (string.IsNullOrWhiteSpace(renderingsField))
        {
          return new List<Rendering>();
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(renderingsField);
        var renderingsFiled = xmlDoc.GetElementsByTagName("r");
  
        foreach (XmlElement r in renderingsFiled)
        {
          var rendering = isLayout ? SetLayoutRendering(r) : GetRendering(r);
          if (rendering != null)
          {
            renderings.Add(rendering);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("XEditorExtender GetRenderings failed: {0}".FormatWith(ex.Message), ex, new object()); 
      }
     
      return renderings;
    }
     
    public virtual bool IsChanged(Item pageItem, ID renderingId)
    {
      var renderingsField = pageItem[FieldIDs.FinalLayoutField];
      if (string.IsNullOrWhiteSpace(renderingsField))
      {
        return false;
      }

      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(renderingsField);

      return xmlDoc.SelectSingleNode("//r[@uid='" + renderingId + "']") != null; 
    }

    public virtual List<Rendering> ResetRendering(Item pageItem, ID renderingId)
    {
      var renderings = new List<Rendering>();
      try
      { 
        var renderingsField = pageItem[FieldIDs.FinalLayoutField];
        if (string.IsNullOrWhiteSpace(renderingsField))
        {
          return new List<Rendering>();
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(renderingsField); 

        XmlNode rendering = xmlDoc.SelectSingleNode("//r[@uid='" + renderingId + "']");
        if (rendering != null && rendering.ParentNode!=null)
        { 
          rendering.ParentNode.RemoveChild(rendering);
        }

        pageItem.Editing.BeginEdit();
        pageItem[FieldIDs.FinalLayoutField] = xmlDoc.InnerXml;
        pageItem.Editing.EndEdit();
      }
      catch (Exception ex)
      {
        Log.Error("XEditorExtender GetRenderings failed: {0}".FormatWith(ex.Message), ex, new object()); 
      }
     
      return renderings;
    }

    protected virtual Rendering GetRendering(XmlElement renderingElement)
    {
      if (!renderingElement.HasAttributes || renderingElement.Attributes["uid"] == null)
      {
        return null;
      }

      var delete = renderingElement.GetElementsByTagName("p:d");

      return new Rendering
      {
        Id = renderingElement.Attributes["s:id"] == null ? noValue : renderingElement.Attributes["s:id"].Value,
        Before = renderingElement.Attributes["p:before"] == null ? noValue : renderingElement.Attributes["p:before"].Value,
        After = renderingElement.Attributes["p:after"] == null ? noValue : renderingElement.Attributes["p:after"].Value,
        Datasourse = renderingElement.Attributes["s:ds"] == null ? noValue : renderingElement.Attributes["s:ds"].Value,
        Plaseholder = renderingElement.Attributes["s:ph"] == null ? noValue : renderingElement.Attributes["s:ph"].Value,
        Properties = GetProperties(renderingElement, "s:par"),
        RenderingId = "r_{0}".FormatWith(new Guid(renderingElement.Attributes["uid"].Value).ToString("N").ToUpper()),
        IsDeleted = delete.Count > 0
      };
    }

    protected virtual Dictionary<string, string> GetProperties(XmlElement renderingElement, string attrName)
    {
      if (!renderingElement.HasAttributes || renderingElement.Attributes[attrName] == null)
      {
        return new Dictionary<string, string>();
      }

      var parametersRes = new Dictionary<string,string>();
      var parameters = HttpUtility.HtmlDecode(renderingElement.Attributes[attrName].Value);
      foreach (var parameter in  parameters.Split('&'))
      {
        var pair = parameter.Split('=');
        var value = pair.Length > 1 ? pair[1] : string.Empty;
        var key = pair[0];
        if (string.IsNullOrWhiteSpace(key) || parametersRes.ContainsKey(key))
        {
          continue;
        }
        parametersRes.Add(key, value);
      }

      return parametersRes;
    }

    protected virtual Rendering SetLayoutRendering(XmlElement renderingElement)
    {
      if (!renderingElement.HasAttributes || renderingElement.Attributes["uid"] == null)
      {
        return null;
      }

      var rendering = new Rendering
      {
        Id = renderingElement.Attributes["id"] == null ? noValue : renderingElement.Attributes["id"].Value,
        Datasourse = renderingElement.Attributes["ds"] == null ? noValue : renderingElement.Attributes["ds"].Value,
        Plaseholder = renderingElement.Attributes["ph"] == null ? noValue : renderingElement.Attributes["ph"].Value,
        RenderingId = "r_{0}".FormatWith(new Guid(renderingElement.Attributes["uid"].Value).ToString("N").ToUpper()),
        Properties = GetProperties(renderingElement, "par")
      };
       
      return rendering;
    }

    public virtual List<RenderingsOperation> GetRenderingsOperations(Item pageItem)
    {
      var operations = new List<RenderingsOperation>();
      try
      {
        var sharedRenderingsField = pageItem[FieldIDs.LayoutField];
        var finalRenderingsField = pageItem[FieldIDs.FinalLayoutField];

        var sharedRenderings = GetRenderings(sharedRenderingsField, true);
        var finalRenderings = GetRenderings(finalRenderingsField);

        foreach (var finalRendering in finalRenderings)
        {
          var sharedRendering = sharedRenderings.FirstOrDefault(r => r.RenderingId == finalRendering.RenderingId);
          if (finalRendering.IsDeleted)
          {
            operations.Add(new RenderingsOperation
            {
              Rendering = finalRendering,
              RenderingsStatus = Status.Deleted,
              Color = settings["deleted"]
            });
            continue;
          }

          if (sharedRendering == null)
          {
            operations.Add(new RenderingsOperation
            {
              Rendering = finalRendering,
              RenderingsStatus = Status.Added,
              Color = settings["added"]
            });
          }
        }

        foreach (var sharedRendering in sharedRenderings)
        {
          var finalRendering = finalRenderings.FirstOrDefault(r => r.RenderingId == sharedRendering.RenderingId);
          if (finalRendering == null || finalRendering.IsDeleted)
          {
            continue;
          }

          var props = GetOperationProperties(finalRendering, sharedRendering);
          if (props.Count > 0)
          {
            var operation = new RenderingsOperation { 
              RenderingsStatus = Status.Updated,
              Rendering = finalRendering, 
              Properties = props ,
              Color = settings["updated"]
            };
            operations.Add(operation);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("XEditorExtender GetRenderingsOperations failed: {0}".FormatWith(ex.Message), ex, new object()); 
      }
      
      return operations;
    }

    protected virtual void ResetChangedProperties(Item item, Rendering finalRendering, Rendering sharedRendering)
    {
      item.Editing.BeginEdit();

      item.Editing.EndEdit();
    }

    protected virtual Dictionary<string, string> GetOperationProperties(Rendering finalRendering, Rendering sharedRendering)
    {
      var props = new Dictionary<string, string>();
      try
      {
        foreach (var property in finalRendering.Properties)
        {
          if (!sharedRendering.Properties.ContainsKey(property.Key))
          {
            props.Add(property.Key, "shared: '' -> final: {0}".FormatWith(property.Value));
          }
          else if (sharedRendering.Properties[property.Key] != property.Value)
          {
            props.Add(property.Key, "updated value: shared:{0} -> final:{1}".FormatWith(sharedRendering.Properties[property.Key], property.Value));
          }
        }

        if ((finalRendering.After != noValue && finalRendering.After != sharedRendering.After) || (finalRendering.Before != noValue && finalRendering.Before != sharedRendering.Before))
        {
          props.Add("The component is moved within the Placeholder", "");
        }

        if (finalRendering.Datasourse != noValue && finalRendering.Datasourse != sharedRendering.Datasourse)
        {
          props.Add("Datasourse", "shared: {0} -> final: {1}".FormatWith(sharedRendering.Datasourse, finalRendering.Datasourse));
        }

        if (finalRendering.Plaseholder != noValue && finalRendering.Plaseholder != sharedRendering.Plaseholder)
        {
          props.Add("The component is moved in other Placeholder", "shared: {0} -> final: {1}".FormatWith(sharedRendering.Plaseholder, finalRendering.Plaseholder));
        }

      }
      catch (Exception ex)
      {
        Log.Error("XEditorExtender GetOperationProperties failed: {0}".FormatWith(ex.Message), ex, new object());
      }
      return props;
    }
 
    public class OperationSettings
    { 
      public string Color { get; set; }
      public string Description { get; set; }
    }
  }
}