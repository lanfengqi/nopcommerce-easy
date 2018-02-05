using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework;

namespace Nop.Web.Areas.Admin.Controllers {
    public partial class CommonController : BaseAdminController
    {
        #region Fields
        private readonly IWebHelper _webHelper;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public CommonController(IWebHelper webHelper,
            ILanguageService languageService,
            IWorkContext workContext,
            IPermissionService permissionService,
            IStaticCacheManager cacheManager)
        {
            this._webHelper = webHelper;
            this._languageService = languageService;
            this._workContext = workContext;
            this._permissionService = permissionService;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        
        public virtual IActionResult SetLanguage(int langid, string returnUrl = "")
        {
            var language = _languageService.GetLanguageById(langid);
            if (language != null)
            {
                _workContext.WorkingLanguage = language;
            }

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.Action("Index", "Home", new { area = AreaNames.Admin });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = AreaNames.Admin });
            return Redirect(returnUrl);
        }

        [HttpPost]
        public virtual IActionResult ClearCache(string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();
            
            _cacheManager.Clear();

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home", new { area = AreaNames.Admin });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = AreaNames.Admin });
            return Redirect(returnUrl);
        }

        [HttpPost]
        public virtual IActionResult RestartApplication(string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            //restart application
            _webHelper.RestartAppDomain();

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home", new { area = AreaNames.Admin });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = AreaNames.Admin });
            return Redirect(returnUrl);
        }

       
        
        #endregion
    }
}