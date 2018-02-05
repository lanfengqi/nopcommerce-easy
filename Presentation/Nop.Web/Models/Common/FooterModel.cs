using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Common {
    public partial class FooterModel : BaseNopModel {
        public string StoreName { get; set; }
        public bool SitemapEnabled { get; set; }
        public bool NewsEnabled { get; set; }

        public bool HidePoweredByNopCommerce { get; set; }

        public int WorkingLanguageId { get; set; }

    }
}