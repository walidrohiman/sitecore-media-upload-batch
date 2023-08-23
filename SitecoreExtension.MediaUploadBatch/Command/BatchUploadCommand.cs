using Sitecore;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;
using System.Collections.Specialized;

namespace SitecoreExtension.MediaUploadBatch.Command
{
    public class BatchUploadCommand : Sitecore.Shell.Framework.Commands.Command
    {
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length != 1)
                return;
            Item obj = context.Items[0];
            Context.ClientPage.Start((object)this, "Run", new NameValueCollection()
            {
                ["id"] = obj.ID.ToString(),
                ["language"] = obj.Language.ToString(),
                ["version"] = obj.Version.ToString()
            });
        }

        public override CommandState QueryState(CommandContext context)
        {
            if (UIUtil.UseFlashUpload() || context.Items.Length != 1)
                return CommandState.Hidden;
            Item obj = context.Items[0];
            return !obj.Access.CanCreate() || !obj.Access.CanRead() || !obj.Access.CanWriteLanguage() ? CommandState.Disabled : base.QueryState(context);
        }

        protected void Run(ClientPipelineArgs args)
        {
            string parameter1 = args.Parameters["id"];
            string parameter2 = args.Parameters["language"];
            string parameter3 = args.Parameters["version"];
            Item obj = Context.ContentDatabase.Items[parameter1, Language.Parse(parameter2), Sitecore.Data.Version.Parse(parameter3)];
            if (obj == null)
                SheerResponse.Alert("Item not found.");
            else if (args.IsPostBack)
            {
                Context.ClientPage.SendMessage((object)this, "item:refresh");
                Context.ClientPage.SendMessage((object)this, "item:refreshchildren(id=" + parameter1 + ")");
            }
            else
            {
                UrlString urlString = new UrlString("/sitecore/shell/Applications/Media/UploadManager/UploadBatchManager.aspx");
                obj.Uri.AddToUrlString(urlString);
                SheerResponse.ShowModalDialog(urlString.ToString(), true);
                args.WaitForPostBack();
            }
        }
    }
}
