using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Upload;
using Sitecore.Shell.Web;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using System;
using System.Web;
using System.Web.UI.HtmlControls;

namespace SitecoreExtension.MediaUploadBatch.Utilities
{
    public class CustomUploadPage : Page
    {
        private bool requestLengthExceeded;

        protected HtmlForm UploadForm;

        protected HtmlInputHidden Uri;

        protected HtmlInputHidden Folder;

        protected HtmlInputHidden Uploading;

        protected HtmlInputHidden UploadedItems;

        protected HtmlInputHidden UploadedItemsHandle;

        protected HtmlInputHidden ErrorText;

        protected HtmlTable Grid;

        protected HtmlTableCell FileListCell;

        protected HtmlInputCheckBox Versioned;

        protected HtmlTableCell OverwriteCell;

        protected HtmlInputCheckBox Overwrite;

        public CustomUploadPage()
        {
            try
            {
                bool flag = HttpContext.Current.Request.Form["test"] == "test";
            }
            catch (HttpException ex)
            {
                this.requestLengthExceeded = true;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.requestLengthExceeded)
            {
                HttpContext.Current.Response.Write(string.Format("<script>alert('{0}');window.parent.close();</script>", (object)Translate.Text("The file is too big to be uploaded.\n\nThe maximum size of a file that can be uploaded is {0}.", (object)MainUtil.FormatSize(Settings.Upload.MaximumDatabaseUploadSize)).Replace("\n", string.Empty)));
                HttpContext.Current.Response.End();
            }
            else
            {
                ShellPage.IsLoggedIn();
                if (this.IsPostBack)
                {
                    ListString items = new ListString(StringUtil.GetString(this.Request.Form["UploadedItems"]));
                    try
                    {
                        this.UploadFiles(items); //to check file name should have .zip
                    }
                    catch
                    {
                        this.ErrorText.Value = Translate.Text("One or more files could not be uploaded.\n\nSee the Log file for more details.");
                    }
                    string key = Guid.NewGuid().ToString();
                    this.UploadedItemsHandle.Value = key;
                    WebUtil.SetSessionValue(key, (object)items.ToString());
                    this.UploadedItems.Value = items.ToString();
                }
                else
                {
                    ItemUri queryString = ItemUri.ParseQueryString(Sitecore.Context.ContentDatabase);
                    Assert.IsNotNull((object)queryString, typeof(ItemUri));
                    this.Uri.Value = queryString.ToString();
                    this.Versioned.Checked = Settings.Media.UploadAsVersionableByDefault;
                }
                if (!Settings.Upload.SimpleUploadOverwriting)
                    return;
                this.Overwrite.Checked = true;
            }
        }

        private void UploadFiles(ListString items)
        {
            Assert.ArgumentNotNull((object)items, nameof(items));
            if (this.Request.Files.Count <= 0)
                return;
            string itemUri1 = StringUtil.GetString(this.Request.Form["Uri"]);
            bool flag1 = StringUtil.GetString(this.Request.Form["Overwrite"]) == "1";
            bool flag2 = true;//StringUtil.GetString(this.Request.Form["Unpack"]) == "1";
            bool flag3 = StringUtil.GetString(this.Request.Form["Versioned"]) == "1";
            string str1 = string.Empty;
            Language language = Sitecore.Context.ContentLanguage;
            ItemUri itemUri2 = ItemUri.Parse(itemUri1);
            if (itemUri2 != (ItemUri)null)
            {
                str1 = itemUri2.GetPathOrId();
                language = itemUri2.Language;
            }
            UploadArgs args = new UploadArgs();
            args.Files = this.Request.Files;
            args.Folder = str1;
            args.Overwrite = flag1;
            args.Unpack = flag2;
            args.Versioned = flag3;
            args.Language = language;
            args.CloseDialogOnEnd = false;
            args.Destination = UploadDestination.Database;
            Pipeline pipeline = PipelineFactory.GetPipeline("uiUploadBatch");
            try
            {
                pipeline.Start((PipelineArgs)args);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is OutOfMemoryException)
                    args.ErrorText = Translate.Text("A file is too big to be uploaded.\n\nThe maximum size of a file that can be uploaded is {0}.", (object)MainUtil.FormatSize(Settings.Media.MaxSizeInDatabase));
                else
                    args.ErrorText = "An error occured while uploading:\n\n" + ex.Message;
            }
            if (!string.IsNullOrEmpty(args.ErrorText))
                this.ErrorText.Value = args.ErrorText;
            foreach (Item uploadedItem in args.UploadedItems)
            {
                string str2 = uploadedItem.Uri.ToString();
                if (!items.Contains(str2))
                    items.Add(str2);
            }
        }
    }
}