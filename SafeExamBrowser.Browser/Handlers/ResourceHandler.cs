﻿/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using CefSharp;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Pages;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser.Filter;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using Request = SafeExamBrowser.Browser.Contracts.Filters.Request;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class ResourceHandler : CefSharp.Handler.ResourceRequestHandler
	{
		private SHA256Managed algorithm;
		private AppConfig appConfig;
		private string browserExamKey;
		private IResourceHandler contentHandler;
		private IRequestFilter filter;
		private HtmlLoader htmlLoader;
		private ILogger logger;
		private IResourceHandler pageHandler;
		private BrowserSettings settings;
		private IText text;

		internal ResourceHandler(AppConfig appConfig, BrowserSettings settings, IRequestFilter filter, ILogger logger, IText text)
		{
			this.appConfig = appConfig;
			this.algorithm = new SHA256Managed();
			this.filter = filter;
			this.htmlLoader = new HtmlLoader(text);
			this.logger = logger;
			this.settings = settings;
			this.text = text;
		}

		protected override IResourceHandler GetResourceHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request)
		{
			if (Block(request))
			{
				return ResourceHandlerFor(request.ResourceType);
			}

			return base.GetResourceHandler(webBrowser, browser, frame, request);
		}

		protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
		{
			if (IsMailtoUrl(request.Url))
			{
				return CefReturnValue.Cancel;
			}

			AppendCustomHeaders(request);
			ReplaceSebScheme(request);

			return base.OnBeforeResourceLoad(webBrowser, browser, frame, request, callback);
		}

		protected override bool OnResourceResponse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
		{
			if (RedirectToDisablePdfToolbar(request, response, out var url))
			{
				webBrowser.Load(url);

				return true;
			}

			return base.OnResourceResponse(webBrowser, browser, frame, request, response);
		}

		private void AppendCustomHeaders(IRequest request)
		{
			var headers = new NameValueCollection(request.Headers);
			var urlWithoutFragment = request.Url.Split('#')[0];

			if (settings.SendConfigurationKey)
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + settings.ConfigurationKey));
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				headers["X-SafeExamBrowser-ConfigKeyHash"] = key;
			}

			if (settings.SendExamKey)
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + (browserExamKey ?? ComputeBrowserExamKey())));
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				headers["X-SafeExamBrowser-RequestHash"] = key;
			}

			request.Headers = headers;
		}

		private bool Block(IRequest request)
		{
			var block = false;

			if (settings.Filter.ProcessContentRequests)
			{
				var result = filter.Process(new Request { Url = request.Url });

				if (result == FilterResult.Block)
				{
					block = true;
					logger.Info($"Blocked content request for '{request.Url}' ({request.ResourceType}, {request.TransitionType}).");
				}
			}

			return block;
		}

		private string ComputeBrowserExamKey()
		{
			var salt = settings.ExamKeySalt;

			if (salt == default(byte[]))
			{
				salt = new byte[0];
				logger.Warn("The current configuration does not contain a salt value for the browser exam key!");
			}

			using (var algorithm = new HMACSHA256(salt))
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(appConfig.CodeSignatureHash + appConfig.ProgramBuildVersion + settings.ConfigurationKey));
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				browserExamKey = key;

				return browserExamKey;
			}
		}

		private bool IsMailtoUrl(string url)
		{
			return url.StartsWith(Uri.UriSchemeMailto);
		}

		private bool RedirectToDisablePdfToolbar(IRequest request, IResponse response, out string url)
		{
			const string DISABLE_PDF_TOOLBAR = "#toolbar=0";
			var isPdf = response.Headers["Content-Type"] == MediaTypeNames.Application.Pdf;
			var isMainFrame = request.ResourceType == ResourceType.MainFrame;
			var hasFragment = request.Url.Contains(DISABLE_PDF_TOOLBAR);
			var redirect = settings.AllowPdfReader && !settings.AllowPdfReaderToolbar && isPdf && isMainFrame && !hasFragment;

			url = request.Url + DISABLE_PDF_TOOLBAR;

			if (redirect)
			{
				logger.Info($"Redirecting to '{url}' to disable PDF reader toolbar.");
			}

			return redirect;
		}

		private void ReplaceSebScheme(IRequest request)
		{
			if (Uri.IsWellFormedUriString(request.Url, UriKind.RelativeOrAbsolute))
			{
				var uri = new Uri(request.Url);

				if (uri.Scheme == appConfig.SebUriScheme)
				{
					request.Url = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttp }.Uri.AbsoluteUri;
				}
				else if (uri.Scheme == appConfig.SebUriSchemeSecure)
				{
					request.Url = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps }.Uri.AbsoluteUri;
				}
			}
		}

		private IResourceHandler ResourceHandlerFor(ResourceType resourceType)
		{
			if (contentHandler == default(IResourceHandler))
			{
				contentHandler = CefSharp.ResourceHandler.FromString(htmlLoader.LoadBlockedContent());
			}

			if (pageHandler == default(IResourceHandler))
			{
				pageHandler = CefSharp.ResourceHandler.FromString(htmlLoader.LoadBlockedPage());
			}

			switch (resourceType)
			{
				case ResourceType.MainFrame:
				case ResourceType.SubFrame:
					return pageHandler;
				default:
					return contentHandler;
			}
		}
	}
}
