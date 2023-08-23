using Sitecore.Shell.Web;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Web.UI.HtmlControls;

namespace SitecoreExtension.MediaUploadBatch.Utilities
{
    public class CustomUploadManagerPage : ClientPage
    {
        protected HtmlGenericControl Upload;

        protected HtmlGenericControl Result;

        protected override void OnLoad(EventArgs e)
        {
            ShellPage.IsLoggedIn();
            base.OnLoad(e);
            this.Upload.Attributes["src"] = "/sitecore/shell/Applications/Media/UploadManager/UploadBatch.aspx?" + WebUtil.GetQueryString();
            this.Result.Attributes["src"] = "/sitecore/shell/Applications/Media/UploadManager/Result.aspx?" + WebUtil.GetQueryString();
        }
    }
}