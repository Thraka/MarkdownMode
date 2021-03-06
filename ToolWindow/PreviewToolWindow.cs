﻿using System;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System.Windows.Controls;

namespace MarkdownMode
{
    [Guid("acd82a5f-9c35-400b-b9d0-f97925f3b312")]
    public class MarkdownPreviewToolWindow : ToolWindowPane
    {
        private WebBrowser browser;
        private Grid parentPanel;
        private MenuItem menuImagesHide = new MenuItem();
        private MenuItem menuImagesSkip = new MenuItem();
        private MenuItem menuIncludesSkip = new MenuItem();
        private MenuItem menuIncludesHide = new MenuItem();

        object source;
        string html;
        string title;
        string path;

        const string EmptyWindowHtml = "Open a markdown file to see a preview.";
        int? scrollBackTo;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public MarkdownPreviewToolWindow() : base(null)
        {
            this.Caption = "Markdown Preview";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            parentPanel = new Grid();
            parentPanel.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            parentPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            Menu menu = new Menu();

            MenuItem menuParentImages = new MenuItem();
            menuParentImages.Header = "Images";

            MenuItem menuParentIncludes = new MenuItem();
            menuParentIncludes.Header = "Includes";

            MenuItem menuCopyHTML = new MenuItem();
            menuCopyHTML.Header = "Copy HTML";
            menuCopyHTML.Click += (sender, e) => { Clipboard.SetText(html); };

            menu.Items.Add(menuParentImages);
            menu.Items.Add(menuParentIncludes);
            menu.Items.Add(menuCopyHTML);

            menuImagesHide.Header = "Hide";
            menuImagesHide.IsCheckable = true;
            menuImagesHide.Checked += menuImagesHide_Checked;
            menuImagesHide.Unchecked += menuImagesHide_Checked;
            menuParentImages.Items.Add(menuImagesHide);

            
            menuImagesSkip.Header = "Skip";
            menuImagesSkip.IsCheckable = true;
            menuImagesSkip.Checked += menuImagesSkip_Checked;
            menuImagesSkip.Unchecked += menuImagesSkip_Checked;
            menuParentImages.Items.Add(menuImagesSkip);

            menuIncludesSkip.Header = "Skip";
            menuIncludesSkip.IsCheckable = true;
            menuIncludesSkip.Checked += menuIncludesSkip_Checked;
            menuIncludesSkip.Unchecked += menuIncludesSkip_Checked;
            menuParentIncludes.Items.Add(menuIncludesSkip);

            menuIncludesHide.Header = "Hide Divs";
            menuIncludesHide.IsCheckable = true;
            menuIncludesHide.Checked += menuIncludesHide_Checked;
            menuIncludesHide.Unchecked += menuIncludesHide_Checked;
            menuParentIncludes.Items.Add(menuIncludesHide);

            browser = new WebBrowser();
            browser.NavigateToString(EmptyWindowHtml);
            browser.LoadCompleted += (sender, args) =>
            {
                if (scrollBackTo.HasValue)
                {
                    var document = browser.Document as mshtml.IHTMLDocument3;

                    if (document != null)
                    {
                        var element = document.documentElement as mshtml.IHTMLElement2;
                        if (element != null)
                        {
                            element.scrollTop = scrollBackTo.Value;
                        }
                    }
                }

                scrollBackTo = null;
            };
            browser.IsVisibleChanged += HandleBrowserIsVisibleChanged;
            browser.Navigating += (sender, args) =>
                {
                    if (this.path == null)
                    {
                        return; // current context unknown
                    }

                    if (args.Uri == null || args.Uri.HostNameType != UriHostNameType.Unknown || string.IsNullOrEmpty(args.Uri.LocalPath))
                    {
                        return; // doesn't look like a relative uri
                    }

                    string documentName =
                        new FileInfo(this.path).ResolveRelativePath(
                            args.Uri.LocalPath.Replace('/', Path.DirectorySeparatorChar));

                    if (documentName == null || !File.Exists(documentName))
                    {
                        return; // relative path could not be resolved, or does not exist
                    }

                    VsShellUtilities.OpenDocument(this, documentName);
                    args.Cancel = true; // open matching document
                };

            Grid.SetRow(menu, 0);
            Grid.SetRow(browser, 1);

            parentPanel.Children.Add(menu);
            parentPanel.Children.Add(browser);

            
        }

        void menuIncludesHide_Checked(object sender, RoutedEventArgs e)
        {
            if (menuIncludesHide.IsChecked)
            {
                menuIncludesSkip.Checked -= menuIncludesSkip_Checked;
                menuIncludesSkip.IsChecked = !menuIncludesHide.IsChecked;
                menuIncludesSkip.Checked += menuIncludesSkip_Checked;
            }

            MarkdownSettings.HideIncludeDivs = menuIncludesHide.IsChecked;
            MarkdownSettings.SkipIncludeProcessing = menuIncludesSkip.IsChecked;
            MarkdownSettings.Parser.RequestParse(true);
        }

        void menuIncludesSkip_Checked(object sender, RoutedEventArgs e)
        {
            if (menuIncludesSkip.IsChecked)
            {
                menuIncludesHide.Checked -= menuIncludesHide_Checked;
                menuIncludesHide.IsChecked = !menuIncludesSkip.IsChecked;
                menuIncludesHide.Checked += menuIncludesHide_Checked;
            }

            MarkdownSettings.HideIncludeDivs = menuIncludesHide.IsChecked;
            MarkdownSettings.SkipIncludeProcessing = menuIncludesSkip.IsChecked;
            MarkdownSettings.Parser.RequestParse(true);
        }

        void menuImagesSkip_Checked(object sender, RoutedEventArgs e)
        {
            if (menuImagesSkip.IsChecked)
            {
                menuImagesHide.Checked -= menuImagesHide_Checked;
                menuImagesHide.IsChecked = !menuImagesSkip.IsChecked;
                menuImagesHide.Checked += menuImagesHide_Checked;
            }

            MarkdownSettings.HideImages = menuImagesHide.IsChecked;
            MarkdownSettings.SkipImages = menuImagesSkip.IsChecked;
            MarkdownSettings.Parser.RequestParse(true);
        }

        void menuImagesHide_Checked(object sender, RoutedEventArgs e)
        {
            if (menuImagesHide.IsChecked)
            {
                menuImagesSkip.Checked -= menuImagesSkip_Checked;
                menuImagesSkip.IsChecked = !menuImagesHide.IsChecked;
                menuImagesSkip.Checked += menuImagesSkip_Checked;
            }

            MarkdownSettings.SkipImages = menuImagesSkip.IsChecked;
            MarkdownSettings.HideImages = menuImagesHide.IsChecked;
            MarkdownSettings.Parser.RequestParse(true);
        }

        public override object Content
        {
            get { return parentPanel; }
        }

        public bool IsVisible
        {
            get { return browser != null && browser.IsVisible; }
        }

        public object CurrentSource
        {
            get
            {
                return source;
            }
        }

        void HandleBrowserIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool visible = (bool)e.NewValue;
            if (visible)
            {
                object source = this.source;
                this.source = null;
                SetPreviewContent(source, this.html, this.title, this.path);
            }
        }

        public void SetPreviewContent(object source, string html, string title, string path)
        {
            if (string.IsNullOrEmpty(html) && string.IsNullOrEmpty(title))
            {
                ClearPreviewContent();
                return;
            }



            bool sameSource = source == this.source;
            this.source = source;
            this.html = html;
            this.title = title;
            this.path = path;

            if (!IsVisible)
                return;

            this.Caption = "Markdown Preview - " + title;

            if (sameSource)
            {
                // If the scroll back to already has a value, it means the current content hasn't finished loading yet,
                // so the current scroll position isn't ready for us to use.  Just use the existing scroll position.
                if (!scrollBackTo.HasValue)
                {
                    var document = browser.Document as mshtml.IHTMLDocument3;
                    if (document != null)
                    {

                        var element = document.documentElement as mshtml.IHTMLElement2;
                        if (element != null)
                        {
                            //var position = browser.InvokeScript("getVerticalScrollPosition");
                            //scrollBackTo = element.scrollTop;
                            scrollBackTo = element.scrollTop;
                        }
                    }
                }
            }
            else
            {
                scrollBackTo = null;
            }

            browser.NavigateToString(html);
        }

        public void ClearPreviewContent()
        {
            this.Caption = "Markdown Preview";
            this.scrollBackTo = null;
            this.source = null;
            this.html = string.Empty;
            this.title = string.Empty;
            this.path = null;


            browser.NavigateToString(EmptyWindowHtml);
        }
    }
}
