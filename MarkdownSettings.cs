using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMode
{
    static class MarkdownSettings
    {
        public static bool HideIncludeDivs { get; set; }
        public static bool SkipIncludeProcessing { get; set; }

        public static bool HideImages { get; set; }
        public static bool SkipImages { get; set; }

        public static PreviewWindowBackgroundParser Parser { get; set; }
    }
}
