using System.Collections.Generic;

namespace LayoutModesComparer.Models
{
  public class Rendering
  {
    public string Plaseholder { get; set; }
    public string Id { get; set; }
    public string After { get; set; }
    public string Before { get; set; }
    public string Datasourse { get; set; } 
    public string RenderingId { get; set; } 
    public bool IsDeleted { get; set; }

    public Dictionary<string, string> Properties { get; set; }
  }
}