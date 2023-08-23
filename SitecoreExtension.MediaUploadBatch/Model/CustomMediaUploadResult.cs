using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace SitecoreExtension.MediaUploadBatch.Model
{
    public class CustomMediaUploadResult
    {
        private Item _item;
        private string _path;
        private string _validMediaPath;

        public Item Item
        {
            get => this._item;
            internal set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this._item = value;
            }
        }

        public string Path
        {
            get => this._path;
            internal set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this._path = value;
            }
        }

        public string ValidMediaPath
        {
            get => this._validMediaPath;
            internal set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this._validMediaPath = value;
            }
        }
    }
}
