using System.Text.Json.Serialization;

namespace SIGRA.Data.Models;

public class ResourceData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("@odata.type")]
    public string OdataType { get; set; }
}