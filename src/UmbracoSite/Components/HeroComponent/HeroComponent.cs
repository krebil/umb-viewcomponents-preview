using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Web.Common.PublishedModels;
using ViewComponentPreviewRCL.Services;

namespace UmbracoSite.Components;



[ViewComponent]
public class HeroComponent : ViewComponent, IUmbracoBlockComponentBase
{
    public string? UmbracoBlockName { get; } = nameof(Hero);
    public Type Type { get; } = typeof(HeroComponent);

    public IViewComponentResult Invoke(BlockGridItem model)
    {
        if (model.Content is not Hero composition)
        {
            throw new ArgumentException($"{nameof(BlockGridItem)} is not of type {UmbracoBlockName}");
        }
            
        return View(new HeroModel(composition, model));
    }
        
    public class HeroModel
    {
        public HeroModel(Hero content, BlockGridItem blockContext)
        {
            Content = content;
            BlockContext = blockContext;
        }

        public Hero Content { get; set; }
        public BlockGridItem BlockContext { get; set; }
    }
}