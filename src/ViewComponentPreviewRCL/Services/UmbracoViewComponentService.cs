namespace ViewComponentPreviewRCL.Services;

public class UmbracoViewComponentService
{
    private readonly Dictionary<string, IUmbracoBlockComponentBase> _viewComponentLookup;

    public UmbracoViewComponentService(IEnumerable<IUmbracoBlockComponentBase> umbracoViewComponents)
    {
        _viewComponentLookup = umbracoViewComponents.Where(x => !string.IsNullOrWhiteSpace(x.UmbracoBlockName)).ToDictionary(x => x.UmbracoBlockName!, StringComparer.InvariantCultureIgnoreCase);        
    }

    public IUmbracoBlockComponentBase? GetComponent(string blockName)
    {
        return _viewComponentLookup.GetValueOrDefault(blockName);
    }
}