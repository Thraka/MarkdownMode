﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using System.Reflection;

namespace MarkdownMode
{
    internal class PreviewWindowBackgroundParser : BackgroundParser
    {
        private static string MarkdownCssPath;

        private readonly MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();
        private readonly string markdownDocumentPath;

        public PreviewWindowBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            ReparseDelay = TimeSpan.FromMilliseconds(500);

            ITextDocument markdownDocument;
            if (textDocumentFactoryService.TryGetTextDocument(textBuffer, out markdownDocument))
            {
                markdownDocumentPath = markdownDocument.FilePath;
            }

            if (MarkdownCssPath == null)
            {
                string installPath = this.GetType().Assembly.GetLocation();
                if (installPath != null)
                {
                    string cssPath = Path.Combine(installPath, "Markdown.css");
                    if (File.Exists(cssPath))
                    {
                        MarkdownCssPath = new Uri(cssPath).ToString();
                    }
                }
            }
        }

        public override string Name
        {
            get
            {
                return "Markdown Preview Window";
            }
        }

        string GetHTMLText(string text, bool extraSpace)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<html><head>")
                .AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">")
                .AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">")
                .AppendFormat("<link rel='stylesheet' href='{0}'/>", MarkdownCssPath).AppendLine()
                .AppendLine("<script>function getVerticalScrollPosition() {return document.body.scrollTop.toString();} function setVerticalScrollPosition(position) {document.body.scrollTop = position;}</script>")
                .AppendLine("</head><body>");

            markdownTransform.SkipIncludes = MarkdownSettings.SkipIncludeProcessing;
            markdownTransform.HideIncludeDivs = MarkdownSettings.HideIncludeDivs;
            markdownTransform.SkipImages = MarkdownSettings.SkipImages;
            markdownTransform.HideImages = MarkdownSettings.HideImages;

            html.AppendLine(markdownTransform.Transform(text, markdownDocumentPath));
            if (extraSpace)
            {
                for (int i = 0; i < 20; i++)
                    html.Append("<br />");
            }

            html.AppendLine("</body></html>");

            return html.ToString();
        }

        protected override void ReParseImpl()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ITextSnapshot snapshot = TextBuffer.CurrentSnapshot;
            string content = GetHTMLText(snapshot.GetText(), true);

            OnParseComplete(new PreviewParseResultEventArgs(content, snapshot, stopwatch.Elapsed));
        }
    }
}
