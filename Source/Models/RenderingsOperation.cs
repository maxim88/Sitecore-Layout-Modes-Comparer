using System.Collections.Generic;

namespace LayoutModesComparer.Models
{
  public class RenderingsOperation
  {
    public RenderingsOperation()
    {
      Properties = new Dictionary<string, string>();
    }

    public string Color { get; set; }
    public Status RenderingsStatus { get; set; }
    public Rendering Rendering { get; set; }
    public Dictionary<string,string> Properties { get; set; }
  }

  public enum Status
  {
    Added = 0,
    Deleted = 1,
    Updated = 2
  }
}