using System.Text.Json.Serialization;

namespace ChargePadLine.Client.Services.Mes.Dto
{
    public class UploadCheckRequestDto
    {
        [JsonPropertyName("SN")]
        public string SN { get; set; } = string.Empty;

        [JsonPropertyName("Resource")]
        public string Resource { get; set; } = string.Empty;

        [JsonPropertyName("StationCode")]
        public string StationCode { get; set; } = string.Empty;

        [JsonPropertyName("WorkOrderCode")]
        public string WorkOrderCode { get; set; } = string.Empty;

        [JsonPropertyName("TestResult")]
        public string TestResult { get; set; } = string.Empty;

        [JsonPropertyName("TestData")]
        public string TestData { get; set; } = string.Empty;
    }
}
