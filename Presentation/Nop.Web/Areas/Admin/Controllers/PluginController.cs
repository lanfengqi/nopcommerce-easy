using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Plugins;
using Nop.Services;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Themes;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class PluginController : BaseAdminController
	{
		#region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
	    private readonly ISettingService _settingService;
	    private readonly IStoreService _storeService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IUploadService _uploadService;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public PluginController(IPluginFinder pluginFinder,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IPermissionService permissionService, 
            ILanguageService languageService,
            ISettingService settingService, 
            IStoreService storeService,
            ExternalAuthenticationSettings externalAuthenticationSettings, 
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IUploadService uploadService,
            IEventPublisher eventPublisher)
        {
            this._pluginFinder = pluginFinder;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._permissionService = permissionService;
            this._languageService = languageService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._customerActivityService = customerActivityService;
            this._customerService = customerService;
            this._uploadService = uploadService;
            this._eventPublisher = eventPublisher;
        }

		#endregion 

        #region Utilities

        protected virtual PluginModel PreparePluginModel(PluginDescriptor pluginDescriptor, 
            bool prepareLocales = true, bool prepareStores = true, bool prepareAcl = true)
        {
            var pluginModel = pluginDescriptor.ToModel();
            //logo
            pluginModel.LogoUrl = pluginDescriptor.GetLogoUrl(_webHelper);

            if (prepareLocales)
            {
                //locales
                AddLocales(_languageService, pluginModel.Locales, (locale, languageId) =>
                {
                    locale.FriendlyName = pluginDescriptor.Instance().GetLocalizedFriendlyName(_localizationService, languageId, false);
                });
            }
            if (prepareStores)
            {
                //stores
                pluginModel.SelectedStoreIds = pluginDescriptor.LimitedToStores;
                var allStores = _storeService.GetAllStores();
                foreach (var store in allStores)
                {
                    pluginModel.AvailableStores.Add(new SelectListItem
                    {
                        Text = store.Name,
                        Value = store.Id.ToString(),
                        Selected = pluginModel.SelectedStoreIds.Contains(store.Id)
                    });
                }
            }

            if (prepareAcl)
            {
                //acl
                pluginModel.SelectedCustomerRoleIds = pluginDescriptor.LimitedToCustomerRoles;
                foreach (var role in _customerService.GetAllCustomerRoles(true))
                {
                    pluginModel.AvailableCustomerRoles.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id.ToString(),
                        Selected = pluginModel.SelectedCustomerRoleIds.Contains(role.Id)
                    });
                }
            }

            //configuration URLs
            if (pluginDescriptor.Installed)
            {
                //display configuration URL only when a plugin is already installed
                var pluginInstance = pluginDescriptor.Instance();
                pluginModel.ConfigurationUrl = pluginInstance.GetConfigurationPageUrl();

            }
            return pluginModel;
        }
        
        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new PluginListModel
            {
                //load modes
                AvailableLoadModes = LoadPluginsMode.All.ToSelectList(false).ToList()
            };
            //groups
            model.AvailableGroups.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var g in _pluginFinder.GetPluginGroups())
                model.AvailableGroups.Add(new SelectListItem { Text = g, Value = g });
            return View(model);
        }

	    [HttpPost]
        public virtual IActionResult ListSelect(DataSourceRequest command, PluginListModel model)
	    {
	        if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
	            return AccessDeniedKendoGridJson();

	        var loadMode = (LoadPluginsMode) model.SearchLoadModeId;
            var pluginDescriptors = _pluginFinder.GetPluginDescriptors(loadMode, group: model.SearchGroup).ToList();
	        var gridModel = new DataSourceResult
            {
                Data = pluginDescriptors.Select(x => PreparePluginModel(x, false, false, false))
                .OrderBy(x => x.Group)
                .ToList(),
                Total = pluginDescriptors.Count()
            };
	        return Json(gridModel);
	    }

	    [HttpPost]
	    public virtual IActionResult UploadPluginsAndThemes(IFormFile archivefile)
	    {
	        if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
	            return AccessDeniedView();

	        try
	        {
                if (archivefile == null || archivefile.Length == 0)
                {

                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }

                var descriptors = _uploadService.UploadPluginsAndThemes(archivefile);
                var pluginDescriptors = descriptors.OfType<PluginDescriptor>().ToList();
                var themeDescriptors = descriptors.OfType<ThemeDescriptor>().ToList();

                //activity log
                foreach (var descriptor in pluginDescriptors)
                {
                    _customerActivityService.InsertActivity("UploadNewPlugin", 
                        _localizationService.GetResource("ActivityLog.UploadNewPlugin"), descriptor.FriendlyName);
                }

                foreach (var descriptor in themeDescriptors)
                {
                    _customerActivityService.InsertActivity("UploadNewTheme",
                        _localizationService.GetResource("ActivityLog.UploadNewTheme"), descriptor.FriendlyName);
                }

                //events
                if (pluginDescriptors?.Any() ?? false)
                    _eventPublisher.Publish(new PluginsUploadedEvent(pluginDescriptors));

                if (themeDescriptors?.Any() ?? false)
                    _eventPublisher.Publish(new ThemesUploadedEvent(themeDescriptors));

                var message = string.Format(_localizationService.GetResource("Admin.Configuration.Plugins.Uploaded"), pluginDescriptors.Count, themeDescriptors.Count);
                SuccessNotification(message);

                //restart application
                _webHelper.RestartAppDomain();
	        }
	        catch (Exception exc)
	        {
	            ErrorNotification(exc);
	        }

	        return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired(FormValueRequirement.StartsWith, "install-plugin-link-")]
        public virtual IActionResult Install(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                //get plugin system name
                string systemName = null;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("install-plugin-link-", StringComparison.InvariantCultureIgnoreCase))
                        systemName = formValue.Substring("install-plugin-link-".Length);

                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("List");

                //check whether plugin is not installed
                if (pluginDescriptor.Installed)
                    return RedirectToAction("List");

                //install plugin
                pluginDescriptor.Instance().Install();

                //activity log
                _customerActivityService.InsertActivity("InstallNewPlugin", _localizationService.GetResource("ActivityLog.InstallNewPlugin"), pluginDescriptor.FriendlyName);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Plugins.Installed"));

                //restart application
                _webHelper.RestartAppDomain();
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }
             
            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired(FormValueRequirement.StartsWith, "uninstall-plugin-link-")]
        public virtual IActionResult Uninstall(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                //get plugin system name
                string systemName = null;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("uninstall-plugin-link-", StringComparison.InvariantCultureIgnoreCase))
                        systemName = formValue.Substring("uninstall-plugin-link-".Length);

                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("List");

                //check whether plugin is installed
                if (!pluginDescriptor.Installed)
                    return RedirectToAction("List");

                //uninstall plugin
                pluginDescriptor.Instance().Uninstall();

                //activity log
                _customerActivityService.InsertActivity("UninstallPlugin", _localizationService.GetResource("ActivityLog.UninstallPlugin"), pluginDescriptor.FriendlyName);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Plugins.Uninstalled"));

                //restart application
                _webHelper.RestartAppDomain();
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            return RedirectToAction("List");
        }

	    [HttpPost, ActionName("List")]
	    [FormValueRequired(FormValueRequirement.StartsWith, "delete-plugin-link-")]
	    public virtual IActionResult Delete(IFormCollection form)
	    {
	        if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
	            return AccessDeniedView();

	        try
	        {
	            //get plugin system name
	            string systemName = null;
	            foreach (var formValue in form.Keys)
	                if (formValue.StartsWith("delete-plugin-link-", StringComparison.InvariantCultureIgnoreCase))
	                    systemName = formValue.Substring("delete-plugin-link-".Length);

	            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
	            if (!PluginManager.DeletePlugin(pluginDescriptor))
                    return RedirectToAction("List");

                //activity log
                _customerActivityService.InsertActivity("DeletePlugin", _localizationService.GetResource("ActivityLog.DeletePlugin"), pluginDescriptor.FriendlyName);

	            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Plugins.Deleted"));

	            //restart application
	            _webHelper.RestartAppDomain();
	        }
	        catch (Exception exc)
	        {
	            ErrorNotification(exc);
	        }

	        return RedirectToAction("List");
	    }

        [HttpPost, ActionName("List")]
        [FormValueRequired("plugin-reload-grid")]
        public virtual IActionResult ReloadList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //restart application
            _webHelper.RestartAppDomain();
            return RedirectToAction("List");
        }
        
        //edit
        public virtual IActionResult EditPopup(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return RedirectToAction("List");

            var model = PreparePluginModel(pluginDescriptor);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult EditPopup(PluginModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(model.SystemName, LoadPluginsMode.All);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //we allow editing of 'friendly name', 'display order', store mappings
                pluginDescriptor.FriendlyName = model.FriendlyName;
                pluginDescriptor.DisplayOrder = model.DisplayOrder;
                pluginDescriptor.LimitedToStores.Clear();
                if (model.SelectedStoreIds.Any())
                    pluginDescriptor.LimitedToStores = model.SelectedStoreIds;
                pluginDescriptor.LimitedToCustomerRoles.Clear();
                if (model.SelectedCustomerRoleIds.Any())
                    pluginDescriptor.LimitedToCustomerRoles = model.SelectedCustomerRoleIds;

                //update the description file
                PluginManager.SavePluginDescriptor(pluginDescriptor);

                //reset plugin cache
                _pluginFinder.ReloadPlugins();

                //locales
                foreach (var localized in model.Locales)
                {
                    pluginDescriptor.Instance().SaveLocalizedFriendlyName(_localizationService, localized.LanguageId, localized.FriendlyName);
                }
                //enabled/disabled
                if (pluginDescriptor.Installed)
                {
                    var pluginInstance = pluginDescriptor.Instance();
                    
                    if (pluginInstance is IExternalAuthenticationMethod)
                    {
                        //external auth method
                        var eam = (IExternalAuthenticationMethod)pluginInstance;
                        if (eam.IsMethodActive(_externalAuthenticationSettings))
                        {
                            if (!model.IsEnabled)
                            {
                                //mark as disabled
                                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(eam.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_externalAuthenticationSettings);
                            }
                        }
                        else
                        {
                            if (model.IsEnabled)
                            {
                                //mark as active
                                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(eam.PluginDescriptor.SystemName);
                                _settingService.SaveSetting(_externalAuthenticationSettings);
                            }
                        }
                    }
                    

                    //activity log
                    _customerActivityService.InsertActivity("EditPlugin", _localizationService.GetResource("ActivityLog.EditPlugin"), pluginDescriptor.FriendlyName);
                }

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        

        #endregion
    }
}