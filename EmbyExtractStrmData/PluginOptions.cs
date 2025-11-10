using System.Collections.Generic;
using System.ComponentModel;

using Emby.Web.GenericEdit;
using Emby.Web.GenericEdit.Common;

using MediaBrowser.Model.Attributes;

namespace EmbyExtractStrmData
{
    public class PluginOptions : EditableOptionsBase
    {
        public override string EditorTitle => "Extract Strm Data";


        [Browsable(false)]
        public List<EditorSelectOption> Libraries { get; set; }

        [DisplayName("Libraries")]
        [Description("Select libraries to extract strm data")]
        [EditMultilSelect]
        [SelectItemsSource(nameof(Libraries))]
        public string SelectedLibraries { get; set; } = string.Empty;

        [DisplayName("Monitor all libraries")]
        [Description("Monitor all libraries to extract strm data from new strm file")]
        public bool MonitorLibraries { get; set; }
    }
}
