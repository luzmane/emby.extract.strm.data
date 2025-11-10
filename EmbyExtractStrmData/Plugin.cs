using System;
using System.IO;
using System.Linq;

using Emby.Web.GenericEdit.Common;

using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Logging;

namespace EmbyExtractStrmData
{
    public class Plugin : BasePluginSimpleUI<PluginOptions>, IHasThumbImage
    {
        public static Plugin Instance { get; private set; }

        private readonly Guid _id = new Guid("84576315-5a93-4ad0-a5f0-b8756957cf80");
        private readonly ILogger _logger;
        private readonly ILibraryManager _libraryManager;

        public sealed override string Name => "Extract Strm Data";
        public sealed override string Description => "Extract metadata from strm files";
        public sealed override Guid Id => this._id;
        public PluginOptions Options => this.GetOptions();
        public ImageFormat ThumbImageFormat => ImageFormat.Png;


        public Plugin(IApplicationHost applicationHost, ILibraryManager libraryManager, ILogManager logManager) : base(applicationHost)
        {
            Instance = this;
            _libraryManager = libraryManager;
            _logger = logManager.GetLogger("ExtractStrmDataTool");
        }

        public Stream GetThumbImage()
        {
            Type type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }

        protected override PluginOptions OnBeforeShowUI(PluginOptions options)
        {
            base.OnBeforeShowUI(options);

            PopulateLibraries(options);

            return options;
        }

        private void PopulateLibraries(PluginOptions options)
        {
            var libraries = _libraryManager
                .GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(CollectionFolder) }
                })
                .OfType<CollectionFolder>()
                .Select(l => new EditorSelectOption()
                {
                    Value = l.InternalId.ToString(),
                    Name = l.Name,
                })
                .ToList();
            _logger.Info($"Found {libraries.Count} libraries");

            if (options.Libraries == null)
            {
                options.Libraries = libraries;
            }
            else
            {
                options.Libraries.Clear();
                options.Libraries.AddRange(libraries);
            }
        }
    }
}
