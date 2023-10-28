using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace UmbracoProject.Models;

internal class UnpublishedProperty : IPublishedProperty
{
    private readonly IPublishedPropertyType propertyType;
    private readonly object dataValue;
    private readonly Lazy<bool> hasValue;
    private readonly Lazy<object> sourceValue;
    private readonly Lazy<object> objectValue;
    private readonly Lazy<object> xpathValue;

    public UnpublishedProperty(IPublishedPropertyType propertyType, object value)
    {
        this.propertyType = propertyType;

        this.dataValue = value;
        this.hasValue = new Lazy<bool>(() => value != null && value.ToString().Trim().Length > 0);

        this.sourceValue = new Lazy<object>(() => this.propertyType.ConvertSourceToInter(null, this.dataValue, true));
        this.objectValue = new Lazy<object>(() =>
            this.propertyType.ConvertInterToObject(null, PropertyCacheLevel.None,
                this.sourceValue.Value, true));
        this.xpathValue = new Lazy<object>(() =>
            this.propertyType.ConvertInterToXPath(null, PropertyCacheLevel.None, this.sourceValue.Value,
                true));
    }

    public string PropertyTypeAlias => this.propertyType.DataType.EditorAlias;

    bool IPublishedProperty.HasValue(string culture, string segment) => this.hasValue.Value;

    public object DataValue => this.dataValue;

    public object Value => this.objectValue.Value;

    public object XPathValue => this.xpathValue.Value;

    public object GetSourceValue(string culture = null, string segment = null) => this.sourceValue.Value;

    public object GetValue(string culture = null, string segment = null) => this.objectValue.Value;

    public object GetXPathValue(string culture = null, string segment = null) => this.xpathValue.Value;
    public object? GetDeliveryApiValue(bool expanding, string? culture = null, string? segment = null)
    {
        throw new NotImplementedException();
    }

    public IPublishedPropertyType PropertyType => this.PropertyType;

    public string Alias => this.Alias;
}