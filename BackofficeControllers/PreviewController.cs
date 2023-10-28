using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using UmbracoProject.Models;
using File = System.IO.File;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace UmbracoProject.BackofficeControllers;

public class PreviewController : UmbracoAuthorizedApiController
{
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ILogger<PreviewController> _logger;
    private readonly IContentTypeService _contentTypeService;
    private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ServiceContext _serviceContext;
    private readonly PropertyEditorCollection _propertyEditorCollection;

    public PreviewController(IApiElementBuilder apiElementBuilder, IJsonSerializer jsonSerializer,
        IProfilingLogger profilingLogger, ILogger<PreviewController> logger, IContentTypeService contentTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory, IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext serviceContext, PropertyEditorCollection propertyEditorCollection)
    {
        _apiElementBuilder = apiElementBuilder;
        _jsonSerializer = jsonSerializer;
        _profilingLogger = profilingLogger;
        _logger = logger;
        _contentTypeService = contentTypeService;
        _publishedContentTypeFactory = publishedContentTypeFactory;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _publishedModelFactory = publishedModelFactory;
        _umbracoContextAccessor = umbracoContextAccessor;
        _serviceContext = serviceContext;
        _propertyEditorCollection = propertyEditorCollection;
    }

    public IActionResult Index()
    {
        return Content("");
    }
    
    [HttpPost]
    public IActionResult GetPreview( BlockPreview data)
    {
        var contentType = _contentTypeService.Get(data.ContentTypeAlias);
        var publishedContentType =
            new Lazy<IPublishedContentType>(() => _publishedContentTypeFactory.CreateContentType(contentType)).Value;
        var propertyType = publishedContentType.PropertyTypes.FirstOrDefault(x => x.Alias == data.PropertyAlias);

        var editor = new BlockGridPropertyValueConverter(_profilingLogger,
            new BlockEditorConverter(_publishedSnapshotAccessor, _publishedModelFactory), _jsonSerializer,
            _apiElementBuilder);


        var page = default(IPublishedContent);

        // If the page is new, then the ID will be zero
        if (data.PageId > 0)
        {
            var context = _umbracoContextAccessor.GetRequiredUmbracoContext();
            // Get page container node
            page = context.Content!.GetById(data.PageId);
            if (page == null)
            {
                // If unpublished, then fake PublishedContent
                page = new UnpublishedContent(data.PageId, _serviceContext, _publishedContentTypeFactory,_propertyEditorCollection);
            }
        }
        
        
        
        var converted = editor.ConvertIntermediateToObject(page, propertyType, PropertyCacheLevel.None, data.Value, false);
        /*var model = converted[0];*/
       
        
        return Content("<h1>Hello World!</h1>");
    }
}

/// <summary>
///     The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.ContentItemSave" />
/// </summary>
internal class ContentItemBinder : IModelBinder
{
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ContentModelBinderHelper _modelBinderHelper;

    public ContentItemBinder(
        IJsonSerializer jsonSerializer,
        IUmbracoMapper umbracoMapper,
        IContentService contentService,
        IContentTypeService contentTypeService,
        IHostingEnvironment hostingEnvironment)
    {
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _modelBinderHelper = new ContentModelBinderHelper();
    }


    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        ContentItemSave? model =
            await _modelBinderHelper.BindModelFromMultipartRequestAsync<ContentItemSave>(_jsonSerializer,
                _hostingEnvironment, bindingContext);

        if (model is null)
        {
            return;
        }

        IContent? persistedContent = CreateNew(model);
        BindModel(model, persistedContent!, _modelBinderHelper, _umbracoMapper);

        bindingContext.Result = ModelBindingResult.Success(model);
    }

    protected virtual IContent? GetExisting(ContentItemSave model) => _contentService.GetById(model.Id);

    private IContent CreateNew(ContentItemSave model)
    {
        IContentType? contentType = _contentTypeService.Get(model.ContentTypeAlias);
        if (contentType == null)
        {
            throw new InvalidOperationException("No content type found with alias " + model.ContentTypeAlias);
        }

        return new Content(
            contentType.VariesByCulture() ? null : model.Variants.First().Name,
            model.ParentId,
            contentType);
    }

    internal static void BindModel(ContentItemSave model, IContent persistedContent,
        ContentModelBinderHelper modelBinderHelper, IUmbracoMapper umbracoMapper)
    {
        model.PersistedContent = persistedContent;

        //create the dto from the persisted model
        if (model.PersistedContent != null)
        {
            foreach (ContentVariantSave variant in model.Variants)
            {
                //map the property dto collection with the culture of the current variant
                variant.PropertyCollectionDto = umbracoMapper.Map<ContentPropertyCollectionDto>(
                    model.PersistedContent,
                    context =>
                    {
                        // either of these may be null and that is ok, if it's invariant they will be null which is what is expected
                        context.SetCulture(variant.Culture);
                        context.SetSegment(variant.Segment);
                    });

                //now map all of the saved values to the dto
                modelBinderHelper.MapPropertyValuesFromSaved(variant, variant.PropertyCollectionDto);
            }
        }
    }
}

internal class ContentModelBinderHelper
{
    public async Task<T?> BindModelFromMultipartRequestAsync<T>(
        IJsonSerializer jsonSerializer,
        IHostingEnvironment hostingEnvironment,
        ModelBindingContext bindingContext)
        where T : class, IHaveUploadedFiles
    {
        var modelName = !string.IsNullOrWhiteSpace(bindingContext.ModelName) ? bindingContext.ModelName : "";

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return null;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        // Check if the argument value is null or empty
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        T? model = jsonSerializer.Deserialize<T>(value);
        if (model is null)
        {
            // Non-integer arguments result in model state errors
            bindingContext.ModelState.TryAddModelError(
                modelName, $"Cannot deserialize {modelName} as {nameof(T)}.");

            return null;
        }

        //Handle file uploads
        foreach (IFormFile formFile in bindingContext.HttpContext.Request.Form.Files)
        {
            //The name that has been assigned in JS has 2 or more parts. The second part indicates the property id
            // for which the file belongs, the remaining parts are just metadata that can be used by the property editor.
            var parts = formFile.Name.Trim(Constants.CharArrays.DoubleQuote).Split(Constants.CharArrays.Underscore);
            if (parts.Length < 2)
            {
                bindingContext.HttpContext.SetReasonPhrase(
                    "The request was not formatted correctly the file name's must be underscore delimited");
                return null;
            }

            var propAlias = parts[1];

            //if there are 3 parts part 3 is always culture
            string? culture = null;
            if (parts.Length > 2)
            {
                culture = parts[2];
                //normalize to null if empty
                if (culture.IsNullOrWhiteSpace())
                {
                    culture = null;
                }
            }

            //if there are 4 parts part 4 is always segment
            string? segment = null;
            if (parts.Length > 3)
            {
                segment = parts[3];
                //normalize to null if empty
                if (segment.IsNullOrWhiteSpace())
                {
                    segment = null;
                }
            }

            // TODO: anything after 4 parts we can put in metadata

            var fileName = formFile.FileName.Trim(Constants.CharArrays.DoubleQuote);

            var tempFileUploadFolder =
                hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
            Directory.CreateDirectory(tempFileUploadFolder);
            var tempFilePath = Path.Combine(tempFileUploadFolder, Guid.NewGuid().ToString());

            using (FileStream stream = File.Create(tempFilePath))
            {
                await formFile.CopyToAsync(stream);
            }

            model.UploadedFiles.Add(new ContentPropertyFile
            {
                TempFilePath = tempFilePath,
                PropertyAlias = propAlias,
                Culture = culture,
                Segment = segment,
                FileName = fileName
            });
        }

        return model;
    }

    /// <summary>
    ///     we will now assign all of the values in the 'save' model to the DTO object
    /// </summary>
    /// <param name="saveModel"></param>
    /// <param name="dto"></param>
    public void MapPropertyValuesFromSaved(IContentProperties<ContentPropertyBasic> saveModel,
        ContentPropertyCollectionDto? dto)
    {
        //NOTE: Don't convert this to linq, this is much quicker
        foreach (ContentPropertyBasic p in saveModel.Properties)
        {
            if (dto is not null)
            {
                foreach (ContentPropertyDto propertyDto in dto.Properties)
                {
                    if (propertyDto.Alias != p.Alias)
                    {
                        continue;
                    }

                    propertyDto.Value = p.Value;
                    break;
                }
            }
        }
    }
}