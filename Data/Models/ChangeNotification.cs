using System.Text.Json.Serialization;

namespace SIGRA.Data.Models;

public class ChangeNotification
{
    [JsonPropertyName("clientState")]
    public string ClientState { get; set; }

    [JsonPropertyName("changeType")]
    public string ChangeType { get; set; }

    [JsonPropertyName("resource")]
    public string Resource { get; set; }

    [JsonPropertyName("resourceData")]
    public ResourceData ResourceData { get; set; }
}