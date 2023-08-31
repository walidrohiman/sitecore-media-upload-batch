using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Upload;
using Sitecore.SecurityModel;
using SitecoreExtension.MediaUploadBatch.Model;
using SitecoreExtension.MediaUploadBatch.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace SitecoreExtension.MediaUploadBatch.Processor
{
    public class CustomSave : UploadProcessor
    {
        public void Process(UploadArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            for (int index = 0; index < args.Files.Count; ++index)
            {
                HttpPostedFile file1 = args.Files[index];
                if (!string.IsNullOrEmpty(file1.FileName))
                {
                    if (Path.GetExtension(file1.FileName).ToLower() == ".zip")
                    {
                        try
                        {
                            bool flag = UploadProcessor.IsUnpack(args, file1);

                            CustomMediaUploader mediaUploader = new CustomMediaUploader()
                            {
                                File = file1,
                                Unpack = flag,
                                Folder = args.Folder,
                                Versioned = args.Versioned,
                                Language = args.Language,
                                AlternateText = args.GetFileParameter(file1.FileName, "alt"),
                                Overwrite = args.Overwrite,
                                FileBased = args.Destination == UploadDestination.File
                            };
                            List<CustomMediaUploadResult> mediaUploadResultList;
                            using (new SecurityDisabler())
                                mediaUploadResultList = mediaUploader.Upload();
                            Log.Audit((object)this, "Upload: {0}", file1.FileName);
                            foreach (CustomMediaUploadResult mediaUploadResult in mediaUploadResultList)
                                this.ProcessItem(args, (MediaItem)mediaUploadResult.Item, mediaUploadResult.Path);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Could not save posted file: " + file1.FileName, ex, (object)this);
                            throw;
                        }
                    }
                }
            }
        }

        private void ProcessItem(UploadArgs args, MediaItem mediaItem, string path)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            Assert.ArgumentNotNull((object)mediaItem, nameof(mediaItem));
            Assert.ArgumentNotNull((object)path, nameof(path));

            if (args.Destination == UploadDestination.Database)
                Log.Info("Media Item has been uploaded to database: " + path, (object)this);
            else
                Log.Info("Media Item has been uploaded to file system: " + path, (object)this);
            args.UploadedItems.Add(mediaItem.InnerItem);
        }
    }
}
