namespace ViewComponentPreviewRCL.Services;

public interface IUmbracoBlockComponentBase
{
    public string? UmbracoBlockName { get; }
    public Type Type { get; }
}