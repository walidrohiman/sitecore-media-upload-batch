using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Mvc.Extensions;
using Sitecore.Pipelines.GetMediaCreatorOptions;
using Sitecore.Resources.Media;
using Sitecore.Zip;
using SitecoreExtension.MediaUploadBatch.Model;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace SitecoreExtension.MediaUploadBatch.Utilities
{
    public class CustomMediaUploader
    {
        private string _alternateText;
        private HttpPostedFile _file;
        private Language _language;
        private string _folder = string.Empty;

        public string AlternateText
        {
            get => this._alternateText;
            set => this._alternateText = value;
        }

        public HttpPostedFile File
        {
            get => this._file;
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this._file = value;
            }
        }

        public string Folder
        {
            get => this._folder;
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this._folder = value;
            }
        }

        public bool Unpack { get; set; }

        public bool Versioned { get; set; }

        public Language Language
        {
            get => this._language;
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this._language = value;
            }
        }

        public bool Overwrite { get; set; }

        public bool FileBased { get; set; }

        public Sitecore.Data.Database Database { get; set; }

        public List<CustomMediaUploadResult> Upload()
        {
            List<CustomMediaUploadResult> list = new List<CustomMediaUploadResult>();

            this.UnpackToDatabase(list);

            return list;
        }

        private void UnpackToDatabase(List<CustomMediaUploadResult> list)
        {
            Assert.ArgumentNotNull((object)list, nameof(list));
            string str = FileUtil.MapPath(TempFolder.GetFilename("temp.zip"));
            this.File.SaveAs(str);
            var fileDirectory = GetFileDirectory();
            try
            {
                using (ZipReader zipReader = new ZipReader(str))
                {
                    foreach (Sitecore.Zip.ZipEntry entry in zipReader.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            CustomMediaUploadResult CustomMediaUploadResult = new CustomMediaUploadResult();
                            list.Add(CustomMediaUploadResult);
                           
                            CustomMediaUploadResult.Path = !fileDirectory.IsEmptyOrNull() ? FileUtil.MakePath(fileDirectory, entry.Name, '/') : FileUtil.MakePath(this.Folder, entry.Name, '/');  //change path based on file name
                            CustomMediaUploadResult.ValidMediaPath = MediaPathManager.ProposeValidMediaPath(CustomMediaUploadResult.Path);
                            
                            MediaCreatorOptions options = new MediaCreatorOptions()
                            {
                                Language = this.Language,
                                Versioned = this.Versioned,
                                OverwriteExisting = this.Overwrite,
                                Destination = CustomMediaUploadResult.ValidMediaPath,
                                FileBased = this.FileBased,
                                Database = this.Database,
                                AlternateText = MediaPathManager.GetMediaName(CustomMediaUploadResult.ValidMediaPath).ToLower()
                            };
                            options.Build(GetMediaCreatorOptionsArgs.UploadContext);
                            Stream stream = entry.GetStream();
                            CustomMediaUploadResult.Item = MediaManager.Creator.CreateFromStream(stream, CustomMediaUploadResult.Path, options);
                        }
                    }
                }
            }
            finally
            {
                FileUtil.Delete(str);
            }
        }

        private string GetFileDirectory()
        {
            var database = Factory.GetDatabase("master");

            var fileName = this.File.FileName.Replace(Path.GetExtension(this.File.FileName), ""); //removes file extension

            var item = database.GetItem($"/sitecore/system/Modules/Media Upload/{fileName}");

            if(item != null)
            {
                var path = item.Fields["Path"].Value;

                if (!path.IsEmptyOrNull())
                {
                    return database.GetItem(new ID(path)).Paths.FullPath;
                }
            }

            return string.Empty;
        }
    }
}