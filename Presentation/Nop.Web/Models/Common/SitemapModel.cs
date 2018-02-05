using System.Collections.Generic;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Common
{
    public partial class SitemapModel : BaseNopModel
    {
        public SitemapModel()
        {
        }

        public bool NewsEnabled { get; set; }
        public bool BlogEnabled { get; set; }
        public bool ForumEnabled { get; set; }
    }
}