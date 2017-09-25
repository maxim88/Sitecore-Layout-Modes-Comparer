define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function (Sitecore, ExperienceEditor) {
  Sitecore.Commands.SelectLayout =
  {
    isNotificationRendered: false,
    canExecute: function (context) {
      if (ExperienceEditor.Web.getUrlQueryStringValue("sc_disable_edit") == "yes") {
        return false;
      }

      if (context.currentContext.isFallback) {
        return false;
      }

      if (!ExperienceEditor.isInMode("edit")) {
        return false;
      }

      if (!context.app.canExecute("ExperienceEditor.IsEditAllVersionsTicked", context.currentContext)) {
        return false;
      }

      if (!this.isNotificationRendered) {

        if (context.app.canExecute("ExperienceEditor.Versions.GetStatus", context.currentContext)) {
          this.defineNotification(context);
        }
        else {
          this.defineFinalNotification(context);

        }
      }

      this.isNotificationRendered = true;

      return true;
    },
    execute: function (context, operations) {
      var postContext = context || this.currentContext;
      ExperienceEditor.PipelinesUtil.generateRequestProcessor("ExperienceEditor.Versions.SelectLayout", function (response) {
        window.parent.location.reload();
      },
          {
            value: encodeURIComponent(context.currentContext.argument)
          })
        .execute(postContext);
    },

    defineNotification: function (context) {
      var that = this;
      var operations = [];
      ExperienceEditor.PipelinesUtil.generateRequestProcessor("LayoutModesComparer.Versions.SharedLayoutMessage", function (response) {


        operations = jQuery.parseJSON(response.responseValue.Operations);
        var message = "notification";
        var text = that.getMessage(operations);

        var id = "EditAllVersionsID";
        var notification = Sitecore.ExperienceEditor.Context.instance.showNotification(message, response.responseValue.value, true);
        notification.innerHTML = notification.innerHTML.replace("{", "<b><a href='#' id='" + id + "'>").replace("}", "</a></b>  " + text);
        jQuery("#" + id).click(function () {
          that.execute(context);
        });
      }).execute(context);

      $.each(operations, function (i, operation) { that.highlightRenderings(operation, that) });

    },

    defineFinalNotification: function (context) {
      var that = this;
      var operations = [];
      ExperienceEditor.PipelinesUtil.generateRequestProcessor("LayoutModesComparer.Versions.FinalLayoutNotificationMessage", function (response) {

        operations = jQuery.parseJSON(response.responseValue.Operations);
        var message = "error";
        var text = that.getMessage(operations);

        var id = "EditSharedVersionsID";
        var notification = Sitecore.ExperienceEditor.Context.instance.showNotification(message, response.responseValue.value, true);
        notification.innerHTML = notification.innerHTML.replace("{", "<b><a href='#' id='" + id + "'>").replace("}", "</a></b>" + text);

        jQuery("#" + id).click(function () {
          context.currentContext.argument = "{E27EE57B-0A0A-4622-BC85-54470522A448}";
          that.execute(context);
        });

      }).execute(context);


      $.each(operations,
	      function (i, operation) { that.highlightRenderings(operation, that) });

    },

    getMessage: function (operations) {
      var text = "";

      if (operations && operations.length > 0) {
        var deleted = 0;
        var updated = 0;
        var added = 0;
        $.each(operations, function (i, operation) {

          if (operation.RenderingsStatus == "1") {
            deleted++;
          }
          else if (operation.RenderingsStatus == "2") {
            updated++;
          }
          else if (operation.RenderingsStatus == "0") {
            added++;
          }
        });

        if (deleted > 0) {
          text += "  <br/> <b>" + deleted + " components were deleted in Final Layout!</b>";
        }
        if (updated > 0) {
          text += "   <br/><b>" + updated + " components were updated in Final Layout!</b>";
        }
        if (added > 0) {
          text += "  <br/> <b>" + added + " components were added in Final Layout!</b>";
        }
      }

      return text;
    },

    defineMousePosition: function (rendering) {
      $(window.parent.document).mousemove(function (event) {
        rendering.css("top", event.clientY + 50 + "px").css("left", event.pageX + "px");
      });
    },

    definePopup: function (rendering, popupHtml) {
      var that = this;
      rendering.append('<div class="jsPopUpToolTip" style="position: fixed;color:black; display: none; padding: 4px; background-color: #f4f4f4; border: 1px solid #000; z-index: 9999;">' + popupHtml + '</div>').hover(function () {
        var popup = $(this).children(".jsPopUpToolTip").last();
        that.defineMousePosition(popup);
        $(popup).show()
      }).mouseleave(function () {
        $(this).children(".jsPopUpToolTip").last().hide();
      });
    },

    highlightRenderings: function (operation, that) {
      var id = operation.Rendering.RenderingId;

      var code = window.parent.document.getElementById(id);
      var rendering = $(code).next();
      rendering.css("margin", "2px 2px 2px 2px");
      if (operation.RenderingsStatus == "0") {
        rendering.css("border", (operation.Color ? operation.Color : "blue") + " 3px dashed");
        that.definePopup(rendering, "This is a new component, created on the Final Layout");
      }

      if (operation.RenderingsStatus == "1") {
        that.definePopup(rendering, "This component is Deleted on the Final Layout");
        rendering.css("border", (operation.Color ? operation.Color : "rgb(194, 55, 55)") + " 3px dashed");
      } 

      if (operation.RenderingsStatus == "2") {
        var changes = "";
        rendering.css("border", (operation.Color ? operation.Color:"rgb(219, 182, 24)") + " 3px dashed");

        for (var c in operation.Properties) {
          changes += "<br /><strong>" + c + "</strong>";
          if (operation.Properties.hasOwnProperty(c)) {
            changes += ":  " + operation.Properties[c];
          }
        }

        that.definePopup(rendering, 'This component is Updated on the Final Layout: ' + changes);
      }
    }
  };
});