using System.Runtime.Serialization;

namespace UmbracoProject.Models;

[DataContract]
public class BlockPreview
{
    [DataMember(Name = "contentTypeAlias")]
    public string? ContentTypeAlias { get; set; }

    [DataMember(Name = "propertyAlias")]
    public string? PropertyAlias { get; set; }

    [DataMember(Name = "pageId")]
    public int PageId { get; set; }

    [DataMember(Name = "value")]
    public string? Value { get; set; }
}