using Newtonsoft.Json;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class AttachFileToRequest
    {
        [JsonProperty("size_bytes")]
        public string SizeBytes { get; set; }

        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("sys_mod_count")]
        public string SysModCount { get; set; }

        [JsonProperty("average_image_color")]
        public string AverageImageColor { get; set; }

        [JsonProperty("image_width")]
        public string ImageWidth { get; set; }

        [JsonProperty("sys_updated_on")]
        public string SysUpdatedOn { get; set; }

        [JsonProperty("sys_tags")]
        public string SysTags { get; set; }

        [JsonProperty("table_name")]
        public string TableName { get; set; }

        [JsonProperty("sys_id")]
        public string SysId { get; set; }

        [JsonProperty("image_height")]
        public string ImageHeight { get; set; }

        [JsonProperty("sys_updated_by")]
        public string SysUpdatedBy { get; set; }

        [JsonProperty("download_link")]
        public string DownloadLink { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("sys_created_on")]
        public string SysCreatedOn { get; set; }

        [JsonProperty("size_compressed")]
        public string SizeCompressed { get; set; }

        [JsonProperty("compressed")]
        public string Compressed { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("table_sys_id")]
        public string TableSysId { get; set; }

        [JsonProperty("chunk_size_bytes")]
        public string ChunkSizeBytes { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("sys_created_by")]
        public string SysCreatedBy { get; set; }
    }
}
