using System;
using System.Threading;

using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;

namespace EmbyExtractStrmData
{
    public sealed class ServerEntryPoint : IServerEntryPoint
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger _logger;

        public ServerEntryPoint(ILibraryManager libraryManager, ILogManager logManager, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _libraryManager = libraryManager;
            _logger = logManager.GetLogger(Plugin.Instance.Name);
        }

        public void Dispose()
        {
            _libraryManager.ItemAdded -= LibraryManager_ItemAdded;
        }

        public void Run()
        {
            _libraryManager.ItemAdded += LibraryManager_ItemAdded;
        }

        // ReSharper disable once AsyncVoidMethod
        private async void LibraryManager_ItemAdded(object sender, ItemChangeEventArgs e)
        {
            if (!Plugin.Instance.Options.MonitorLibraries)
            {
                _logger.Info("Monitoring is turned off");
                return;
            }

            try
            {
                var item = e.Item;
                if (item == null
                    || string.IsNullOrEmpty(item.Path)
                    || !item.Path.EndsWith(".strm", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                _logger.Info($"New strm file detected: {item.Name}");

                if (BaseItemHelper.HasBothMediaStreams(item))
                {
                    _logger.Info($"{item.Name} already has complete media info, skipping");
                    return;
                }

                try
                {
                    _logger.Info($"Processing new strm file: {item.Name}");
                    var options = new MetadataRefreshOptions(new DirectoryService(_fileSystem))
                    {
                        EnableRemoteContentProbe = true,
                        ReplaceAllMetadata = true,
                        MetadataRefreshMode = MetadataRefreshMode.ValidationOnly,
                        EnableThumbnailImageExtraction = false,
                        ImageRefreshMode = MetadataRefreshMode.ValidationOnly,
                        ReplaceAllImages = false
                    };

                    _ = await item.RefreshMetadata(options, CancellationToken.None);

                    _logger.Info($"{item.Name}: Real-time processing done.");

                    if (!BaseItemHelper.HasBothMediaStreams(item))
                    {
                        _logger.Warn($"{item.Name} may still lack full media info, will be processed by scheduled task");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error processing new strm file {item.Name} ({item.Path}): {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in item added event handler: {ex.Message}");
            }
        }
    }
}
