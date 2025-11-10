using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;

namespace EmbyExtractStrmData
{
    public class ExtractStrmDataTask : IScheduledTask
    {
        public string Name => "Extract Strm Data";
        public string Key => "ExtractStrmData";
        public string Description => "Extracts data from strm file to Emby";
        public string Category => Name;

        private readonly ILogger _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IFileSystem _fileSystem;

        public ExtractStrmDataTask(ILibraryManager libraryManager, IFileSystem fileSystem, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _logger = logManager.GetLogger(nameof(ExtractStrmDataTask));
            _fileSystem = fileSystem;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.Info("Starting strm file scan...");

            List<long> ids = new List<long>();
            foreach (var id in Plugin.Instance.Options.SelectedLibraries.Split(','))
            {
                if (long.TryParse(id, out var idInt))
                {
                    ids.Add(idInt);
                }
            }

            _logger.Info($"Selected {ids.Count} libraries");

            var allItems = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    HasPath = true,
                    Recursive = true,
                    ExcludeItemTypes = new[] { "Folder", "CollectionFolder" },
                    AncestorIds = ids.ToArray(),
                })
                .Where(i =>
                    i.LocationType == LocationType.FileSystem
                    && !string.IsNullOrEmpty(i.Path)
                    && i.Path.EndsWith(".strm", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            _logger.Info($"Found {allItems.Count} strm files in selected libraries");

            var strmItems = allItems
                .Where(i => !BaseItemHelper.HasBothMediaStreams(i))
                .ToList();

            _logger.Info($"{strmItems.Count} strm files pending metadata refresh");

            if (strmItems.Count == 0)
            {
                progress.Report(100);
                _logger.Info("Nothing to process, task completed.");
                return;
            }

            var options = new MetadataRefreshOptions(new DirectoryService(_fileSystem))
            {
                EnableRemoteContentProbe = true,
                ReplaceAllMetadata = true,
                MetadataRefreshMode = MetadataRefreshMode.ValidationOnly,
                EnableThumbnailImageExtraction = false,
                ImageRefreshMode = MetadataRefreshMode.ValidationOnly,
                ReplaceAllImages = false
            };

            int processed = 0;
            double total = strmItems.Count;
            foreach (var item in strmItems)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("Task was cancelled");
                    break;
                }

                try
                {
                    _logger.Info($"Processing '{item.Name}' with path '{item.Path}'");
                    _logger.Debug($"'{item.Name}' has content: '{(string.IsNullOrWhiteSpace(item.Path) ? string.Empty : File.ReadAllText(item.Path))}'");
                    _ = await item.RefreshMetadata(options, cancellationToken);
                    _logger.Info($"{item.Name}: Refresh done");

                    if (!BaseItemHelper.HasBothMediaStreams(item))
                    {
                        _logger.Warn($"{item.Name} may still lack full media info");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error processing {item.Name} ({item.Path}): {ex.Message}");
                }

                progress.Report(++processed / total * 100);

                if (processed < total)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

            progress.Report(100);
            _logger.Info($"Task complete. Fully processed {processed}/{(int)total} strm files.");
        }
    }
}
