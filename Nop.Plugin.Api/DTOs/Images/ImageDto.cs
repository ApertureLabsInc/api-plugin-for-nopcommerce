using Newtonsoft.Json;
using Hausera.Core.Shared.EnumsAndConstants;
using Nop.Plugin.Api.Attributes;

namespace Nop.Plugin.Api.DTOs.Images
{
    [ImageValidation]
    public class ImageDto
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("attachment")]
        public string Attachment { get; set; }

        [JsonProperty("entitypicturetype")]
        public EntityPictureType EntityPictureType { get; set; }

        [JsonIgnore]
        public byte[] Binary { get; set; }

        [JsonIgnore]
        public string MimeType { get; set; }
    }
}
