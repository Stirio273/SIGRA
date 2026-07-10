using System.Text.Json.Serialization;
using Microsoft.Graph.Models;

namespace SIGRA.Data.Models;

public class NotificationCollection
{
    [JsonPropertyName("value")]
    public List<ChangeNotification> Value { get; set; }
}
