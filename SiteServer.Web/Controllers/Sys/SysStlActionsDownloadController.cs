﻿using System.Web;
using System.Web.Http;
using SiteServer.CMS.Api.Sys.Stl;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.DataCache.Content;
using SiteServer.CMS.Model.Attributes;
using SiteServer.CMS.Plugin.Impl;
using SiteServer.Utils;
using SiteServer.Utils.Enumerations;

namespace SiteServer.API.Controllers.Sys
{
    public class SysStlActionsDownloadController : ApiController
    {
        [HttpGet]
        [Route(ApiRouteActionsDownload.Route)]
        public void Main()
        {
            try
            {
                var request = new AuthenticatedRequest();

                if (!string.IsNullOrEmpty(request.GetQueryString("siteId")) && !string.IsNullOrEmpty(request.GetQueryString("fileUrl")) && string.IsNullOrEmpty(request.GetQueryString("contentId")))
                {
                    var siteId = request.GetQueryInt("siteId");
                    var fileUrl = TranslateUtils.DecryptStringBySecretKey(request.GetQueryString("fileUrl"));

                    if (PageUtils.IsProtocolUrl(fileUrl))
                    {
                        PageUtilsEx.Redirect(fileUrl);
                        return;
                    }

                    var siteInfo = SiteManager.GetSiteInfo(siteId);
                    var filePath = PathUtility.MapPath(siteInfo, fileUrl);
                    var fileType = EFileSystemTypeUtils.GetEnumType(PathUtils.GetExtension(filePath));
                    if (EFileSystemTypeUtils.IsDownload(fileType))
                    {
                        if (FileUtils.IsFileExists(filePath))
                        {
                            PageUtilsEx.Download(HttpContext.Current.Response, filePath);
                            return;
                        }
                    }
                    else
                    {
                        PageUtilsEx.Redirect(PageUtility.ParseNavigationUrl(siteInfo, fileUrl, false));
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(request.GetQueryString("filePath")))
                {
                    var filePath = TranslateUtils.DecryptStringBySecretKey(request.GetQueryString("filePath"));
                    var fileType = EFileSystemTypeUtils.GetEnumType(PathUtils.GetExtension(filePath));
                    if (EFileSystemTypeUtils.IsDownload(fileType))
                    {
                        if (FileUtils.IsFileExists(filePath))
                        {
                            PageUtilsEx.Download(HttpContext.Current.Response, filePath);
                            return;
                        }
                    }
                    else
                    {
                        var fileUrl = PageUtilsEx.GetRootUrlByPhysicalPath(filePath);
                        PageUtilsEx.Redirect(PageUtilsEx.ParseNavigationUrl(fileUrl));
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(request.GetQueryString("siteId")) && !string.IsNullOrEmpty(request.GetQueryString("channelId")) && !string.IsNullOrEmpty(request.GetQueryString("contentId")) && !string.IsNullOrEmpty(request.GetQueryString("fileUrl")))
                {
                    var siteId = request.GetQueryInt("siteId");
                    var channelId = request.GetQueryInt("channelId");
                    var contentId = request.GetQueryInt("contentId");
                    var fileUrl = TranslateUtils.DecryptStringBySecretKey(request.GetQueryString("fileUrl"));
                    var siteInfo = SiteManager.GetSiteInfo(siteId);
                    var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                    var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, contentId);

                    DataProvider.ContentDao.AddDownloads(ChannelManager.GetTableName(siteInfo, channelInfo), channelId, contentId);

                    if (!string.IsNullOrEmpty(contentInfo?.GetString(BackgroundContentAttribute.FileUrl)))
                    {
                        if (PageUtils.IsProtocolUrl(fileUrl))
                        {
                            PageUtilsEx.Redirect(fileUrl);
                            return;
                        }

                        var filePath = PathUtility.MapPath(siteInfo, fileUrl, true);
                        var fileType = EFileSystemTypeUtils.GetEnumType(PathUtils.GetExtension(filePath));
                        if (EFileSystemTypeUtils.IsDownload(fileType))
                        {
                            if (FileUtils.IsFileExists(filePath))
                            {
                                PageUtilsEx.Download(HttpContext.Current.Response, filePath);
                                return;
                            }
                        }
                        else
                        {
                            PageUtilsEx.Redirect(PageUtility.ParseNavigationUrl(siteInfo, fileUrl, false));
                            return;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            HttpContext.Current.Response.Write("下载失败，不存在此文件！");
        }
    }
}
