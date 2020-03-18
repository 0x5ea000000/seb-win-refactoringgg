﻿/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Browser.Pages;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;

namespace SafeExamBrowser.Browser
{
	internal class BrowserControl : ChromiumWebBrowser, IBrowserControl
	{
		private IContextMenuHandler contextMenuHandler;
		private IDialogHandler dialogHandler;
		private IDisplayHandler displayHandler;
		private IDownloadHandler downloadHandler;
		private string errorPage;
		private HtmlLoader htmlLoader;
		private IKeyboardHandler keyboardHandler;
		private ILifeSpanHandler lifeSpanHandler;
		private ILogger logger;
		private IRequestHandler requestHandler;

		private AddressChangedEventHandler addressChanged;
		private LoadingStateChangedEventHandler loadingStateChanged;
		private TitleChangedEventHandler titleChanged;

		public bool CanNavigateBackwards => GetBrowser().CanGoBack;
		public bool CanNavigateForwards => GetBrowser().CanGoForward;

		event AddressChangedEventHandler IBrowserControl.AddressChanged
		{
			add { addressChanged += value; }
			remove { addressChanged -= value; }
		}

		event LoadingStateChangedEventHandler IBrowserControl.LoadingStateChanged
		{
			add { loadingStateChanged += value; }
			remove { loadingStateChanged -= value; }
		}

		event TitleChangedEventHandler IBrowserControl.TitleChanged
		{
			add { titleChanged += value; }
			remove { titleChanged -= value; }
		}

		public BrowserControl(
			IContextMenuHandler contextMenuHandler,
			IDialogHandler dialogHandler,
			IDisplayHandler displayHandler,
			IDownloadHandler downloadHandler,
			HtmlLoader htmlLoader,
			IKeyboardHandler keyboardHandler,
			ILifeSpanHandler lifeSpanHandler,
			ILogger logger,
			IRequestHandler requestHandler,
			string url) : base(url)
		{
			this.contextMenuHandler = contextMenuHandler;
			this.dialogHandler = dialogHandler;
			this.displayHandler = displayHandler;
			this.downloadHandler = downloadHandler;
			this.htmlLoader = htmlLoader;
			this.keyboardHandler = keyboardHandler;
			this.lifeSpanHandler = lifeSpanHandler;
			this.logger = logger;
			this.requestHandler = requestHandler;
		}

		public void Initialize()
		{
			AddressChanged += (o, args) => addressChanged?.Invoke(args.Address);
			LoadError += BrowserControl_LoadError;
			LoadingStateChanged += (o, args) => loadingStateChanged?.Invoke(args.IsLoading);
			TitleChanged += (o, args) => titleChanged?.Invoke(args.Title);

			DialogHandler = dialogHandler;
			DisplayHandler = displayHandler;
			DownloadHandler = downloadHandler;
			KeyboardHandler = keyboardHandler;
			LifeSpanHandler = lifeSpanHandler;
			MenuHandler = contextMenuHandler;
			RequestHandler = requestHandler;

			errorPage = htmlLoader.LoadErrorPage();
		}

		public void NavigateBackwards()
		{
			GetBrowser().GoBack();
		}

		public void NavigateForwards()
		{
			GetBrowser().GoForward();
		}

		public void NavigateTo(string address)
		{
			Load(address);
		}

		public void ShowDeveloperConsole()
		{
			GetBrowser().ShowDevTools();
		}

		public void Reload()
		{
			GetBrowser().Reload();
		}

		public void Zoom(double level)
		{
			GetBrowser().SetZoomLevel(level);
		}

		private void BrowserControl_LoadError(object sender, LoadErrorEventArgs e)
		{
			if (e.ErrorCode == CefErrorCode.None)
			{
				logger.Info($"Request for '{e.FailedUrl}' was successful.");
			}
			else if (e.ErrorCode == CefErrorCode.Aborted)
			{
				logger.Info($"Request for '{e.FailedUrl}' was aborted.");
			}
			else
			{
				var html = string.Copy(errorPage);

				logger.Warn($"Request for '{e.FailedUrl}' failed: {e.ErrorText} ({e.ErrorCode}).");

				html = html.Replace("%%STATUS%%", $"{e.ErrorText} ({e.ErrorCode})");
				html = html.Replace("%%URL%%", e.FailedUrl);

				e.Frame.LoadHtml(html, true);
			}
		}
	}
}
