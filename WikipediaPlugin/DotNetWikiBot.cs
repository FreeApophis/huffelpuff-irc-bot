// DotNetWikiBot Framework v2.72 - bot framework based on Microsoft .NET Framework 2.0 for wiki projects
// Distributed under the terms of the MIT (X11) license: http://www.opensource.org/licenses/mit-license.php
// Copyright (c) Iaroslav Vassiliev (2006-2009) codedriller@gmail.com

using System;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Web;

namespace DotNetWikiBot
{
	/// <summary>Class defines wiki site object.</summary>
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class Site
	{
		/// <summary>Wiki site URL.</summary>
		public string site;
		/// <summary>User's account to login with.</summary>
		private string userName;
		/// <summary>User's password to login with.</summary>
		private string userPass;
		/// <summary>Default domain for LDAP authentication. Additional information can
		/// be found at http://www.mediawiki.org/wiki/Extension:LDAP_Authentication.</summary>
		private string userDomain = "";
		/// <summary>Site title.</summary>
		public string name;
		/// <summary>MediaWiki version as string.</summary>
		public string generator;
		/// <summary>MediaWiki version as number.</summary>
		public float version;
		/// <summary>MediaWiki version as Version object.</summary>
		public Version ver;
		/// <summary>Rule of page title capitalization.</summary>
		public string capitalization;
		/// <summary>Short relative path to wiki pages (if such alias is set on the server).
		/// See "http://www.mediawiki.org/wiki/Manual:Short URL" for details.</summary>
		public string wikiPath;		// = "/wiki/";
		/// <summary>Relative path to "index.php" file on server.</summary>
		public string indexPath;	// = "/w/";
		/// <summary>User's watchlist. Should be loaded manually with FillFromWatchList function,
		/// if it is necessary.</summary>
		public PageList watchList;
		/// <summary>MediaWiki interface messages. Should be loaded manually with
		/// GetMediaWikiMessages function, if it is necessary.</summary>
		public PageList messages;
		/// <summary>Regular expression to find redirection target.</summary>
		public Regex redirectRE;
		/// <summary>Regular expression to find links to pages in list in HTML source.</summary>
		public Regex linkToPageRE1 = new Regex("<li><a href=\"[^\"]*?\" " +
			"(?:class=\"mw-redirect\" )?title=\"([^\"]+?)\">");
		/// <summary>Alternative regular expression to find links to pages in HTML source.</summary>
		public Regex linkToPageRE2 = new Regex("<a href=\"[^\"]*?\" title=\"([^\"]+?)\">\\1</a>");
		/// <summary>Alternative regular expression to find links to pages (mostly image and file
		/// pages) in HTML source.</summary>
		public Regex linkToPageRE3;
		/// <summary>Regular expression to find links to subcategories in HTML source
		/// of category page on sites that use "CategoryTree" MediaWiki extension.</summary>
		public Regex linkToSubCategoryRE = new Regex(">([^<]+)</a></div>\\s*" +
			"<div class=\"CategoryTreeChildren\"");
		/// <summary>Regular expression to find links to image pages in galleries
		/// in HTML source.</summary>
		public Regex linkToImageRE = new Regex("<div class=\"gallerytext\">\n" +
			"<a href=\"[^\"]*?\" title=\"([^\"]+?)\">");
		/// <summary>Regular expression to find titles in markup.</summary>
		public Regex pageTitleTagRE = new Regex("<title>(.+?)</title>");
		/// <summary>Regular expression to find internal wiki links in markup.</summary>
		public Regex wikiLinkRE = new Regex(@"\[\[(.+?)(\|.+?)?]]");
		/// <summary>Regular expression to find wiki category links.</summary>
		public Regex wikiCategoryRE;
		/// <summary>Regular expression to find wiki templates in markup.</summary>
		public Regex wikiTemplateRE = new Regex(@"(?s)\{\{(.+?)((\|.*?)*?)}}");
		/// <summary>Regular expression to find embedded images and files in wiki markup.</summary>
		public Regex wikiImageRE;
		/// <summary>Regular expression to find links to sister wiki projects in markup.</summary>
		public Regex sisterWikiLinkRE;
		/// <summary>Regular expression to find interwiki links in wiki markup.</summary>
		public Regex iwikiLinkRE;
		/// <summary>Regular expression to find displayed interwiki links in wiki markup,
		/// like "[[:de:...]]".</summary>
		public Regex iwikiDispLinkRE;
		/// <summary>Regular expression to find external web links in wiki markup.</summary>
		public Regex webLinkRE = new Regex("(https?|t?ftp|news|nntp|telnet|irc|gopher)" +
			"://([^\\s'\"<>]+)");
		/// <summary>Regular expression to find sections of text, that are explicitly
		/// marked as non-wiki with special tag.</summary>
		public Regex noWikiMarkupRE = new Regex("(?is)<nowiki>(.*?)</nowiki>");
		/// <summary>A template for disambiguation page. If some unusual template is used in your
		/// wiki for disambiguation, then it must be set in this variable. Use "|" as a delimiter
		/// when enumerating several templates here.</summary>
		public string disambigStr;
		/// <summary>Regular expression to extract language code from site URL.</summary>
		public Regex siteLangRE = new Regex(@"http://(.*?)\.(.+?\..+)");
		/// <summary>Regular expression to extract edit session time attribute.</summary>
		public Regex editSessionTimeRE1 =
			new Regex("value=\"([^\"]*?)\" name=['\"]wpEdittime['\"]");
		/// <summary>Regular expression to extract edit session time attribute.</summary>
		public Regex editSessionTimeRE3 = new Regex(" touched=\"(.+?)\"");
		/// <summary>Regular expression to extract edit session token attribute.</summary>
		public Regex editSessionTokenRE1 =
			new Regex("value=\"([^\"]*?)\" name=['\"]wpEditToken['\"]");
		/// <summary>Regular expression to extract edit session token attribute.</summary>
		public Regex editSessionTokenRE2 = new Regex("name=['\"]wpEditToken['\"]" +
			"(?: type=\"hidden\")? value=\"([^\"]*?)\"");
		/// <summary>Regular expression to extract edit session token attribute.</summary>
		public Regex editSessionTokenRE3 = new Regex(" edittoken=\"(.+?)\"");
		/// <summary>Site cookies.</summary>
		public CookieContainer cookies = new CookieContainer();
		/// <summary>XML name table for parsing XHTML documents from wiki site.</summary>
		public NameTable xhtmlNameTable = new NameTable();
		/// <summary>XML namespace URI of wiki site's XHTML version.</summary>
		public string xhtmlNSUri = "http://www.w3.org/1999/xhtml";
		/// <summary>XML namespace manager for parsing XHTML documents from wiki site.</summary>
		public XmlNamespaceManager xmlNS;
		/// <summary>Local namespaces.</summary>
		public Hashtable namespaces = new Hashtable();
		/// <summary>Default namespaces.</summary>
		public static Hashtable wikiNSpaces = new Hashtable();
		/// <summary>List of Wikimedia Foundation sites and according prefixes.</summary>
		public static Hashtable WMSites = new Hashtable();
		/// <summary>Built-in variables of MediaWiki software
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
		public static string[] mediaWikiVars;
		/// <summary>Built-in parser functions of MediaWiki software
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
		public static string[] parserFunctions;
		/// <summary>Built-in template modifiers of MediaWiki software
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
		public static string[] templateModifiers;
		/// <summary>Interwiki links sorting order, based on local language by first word.
		/// See http://meta.wikimedia.org/wiki/Interwiki_sorting_order for details.</summary>
		public static string[] iwikiLinksOrder1;
		/// <summary>Interwiki links sorting order, based on local language.</summary>
		public static string[] iwikiLinksOrder2;
		/// <summary>Wikimedia Foundation sites and prefixes in one regex-escaped string
		/// with "|" as separator.</summary>
		public static string WMSitesStr;
		/// <summary>ISO 639-1 language codes, used as prefixes to identify Wikimedia
		/// Foundation sites, gathered in one regex-escaped string with "|" as separator.</summary>
		public static string WMLangsStr;
		/// <summary>Availability of "api.php" MediaWiki extension (bot interface).</summary>
		public bool botQuery;
		/// <summary>Versions of "api.php" MediaWiki extension (bot interface) modules.</summary>
		public Hashtable botQueryVersions = new Hashtable();
		/// <summary>Set of lists of pages, produced by bot interface.</summary>
		public static Hashtable botQueryLists = new Hashtable();
		/// <summary>Set of lists of parsed data, produced by bot interface.</summary>
		public static Hashtable botQueryProps = new Hashtable();
		/// <summary>Site language.</summary>
		public string language;
		/// <summary>Site language text direction.</summary>
		public string langDirection;
		/// <summary>Site's neutral (language) culture.</summary>
		public CultureInfo langCulture;
		/// <summary>Randomly chosen regional (non-neutral) culture for site's language.</summary>
		public CultureInfo regCulture;
		/// <summary>Site encoding.</summary>
		public Encoding encoding = Encoding.UTF8;

		/// <summary>This constructor is used to generate most Site objects.</summary>
		/// <param name="site">Wiki site's URI. It must point to the main page of the wiki, e.g.
		/// "http://en.wikipedia.org" or "http://127.0.0.1:80/w/index.php?title=Main_page".</param>
		/// <param name="userName">User name to log in.</param>
		/// <param name="userPass">Password.</param>
		/// <returns>Returns Site object.</returns>
		public Site(string site, string userName, string userPass)
			: this(site, userName, userPass, "") {}

		/// <summary>This constructor is used for LDAP authentication. Additional information can
		/// be found at "http://www.mediawiki.org/wiki/Extension:LDAP_Authentication".</summary>
		/// <param name="site">Wiki site's URI. It must point to the main page of the wiki, e.g.
		/// "http://en.wikipedia.org" or "http://127.0.0.1:80/w/index.php?title=Main_page".</param>
		/// <param name="userName">User name to log in.</param>
		/// <param name="userPass">Password.</param>
		/// <param name="userDomain">Domain for LDAP authentication.</param>
		/// <returns>Returns Site object.</returns>
		public Site(string site, string userName, string userPass, string userDomain)
		{
			this.site = site;
			this.userName = userName;
			this.userPass = userPass;
			this.userDomain = userDomain;
			Initialize();
		}

		/// <summary>This constructor uses default site, userName and password. The site URL and
		/// account data can be stored in UTF8-encoded "Defaults.dat" file in bot's "Cache"
		/// subdirectory.</summary>
		/// <returns>Returns Site object.</returns>
		public Site()
		{
			if (File.Exists("Cache" + Path.DirectorySeparatorChar + "Defaults.dat") == true) {
				string[] lines = File.ReadAllLines(
					"Cache" + Path.DirectorySeparatorChar + "Defaults.dat", Encoding.UTF8);
				if (lines.GetUpperBound(0) >= 2) {
					this.site = lines[0];
					this.userName = lines[1];
					this.userPass = lines[2];
					if (lines.GetUpperBound(0) >= 3)
						this.userDomain = lines[3];
					else
						this.userDomain = "";
				}
				else
					throw new WikiBotException(
						Bot.Msg("\"\\Cache\\Defaults.dat\" file is invalid."));
			}
			else
				throw new WikiBotException(Bot.Msg("\"\\Cache\\Defaults.dat\" file not found."));
			Initialize();
		}

		/// <summary>This internal function establishes connection to site and loads general site
		/// info by the use of other functions. Function is called from the constructors.</summary>
		public void Initialize()
		{
			xmlNS = new XmlNamespaceManager(xhtmlNameTable);
			GetPaths();
			xmlNS.AddNamespace("ns", xhtmlNSUri);
			LoadDefaults();
			LogIn();
			GetInfo();
			//GetMediaWikiMessagesEx(false);
		}

		/// <summary>Gets path to "index.php", short path to pages (if present), and then
		/// saves paths to file.</summary>
		public void GetPaths()
		{
			if (!site.StartsWith("http"))
				site = "http://" + site;
			if (Bot.CountMatches(site, "/", false) == 3 && site.EndsWith("/"))
				site = site.Substring(0, site.Length - 1);
			string filePathName = "Cache" + Path.DirectorySeparatorChar +
				HttpUtility.UrlEncode(site.Replace("://", ".").Replace("/", ".")) + ".dat";
			if (File.Exists(filePathName) == true) {
				string[] lines = File.ReadAllLines(filePathName, Encoding.UTF8);
				if (lines.GetUpperBound(0) >= 4) {
					wikiPath = lines[0];
					indexPath = lines[1];
					xhtmlNSUri = lines[2];
					language = lines[3];
					langDirection = lines[4];
					if (lines.GetUpperBound(0) >= 5)
						site = lines[5];
					return;
				}
			}
			Console.WriteLine(Bot.Msg("Logging in..."));
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site);
			webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
			webReq.UseDefaultCredentials = true;
			webReq.ContentType = Bot.webContentType;
			webReq.UserAgent = Bot.botVer;
			if (Bot.unsafeHttpHeaderParsingUsed == 0) {
				webReq.ProtocolVersion = HttpVersion.Version10;
				webReq.KeepAlive = false;
			}
			HttpWebResponse webResp = null;
			for (int errorCounter = 0; errorCounter <= Bot.retryTimes; errorCounter++) {
				try {
					webResp = (HttpWebResponse)webReq.GetResponse();
					break;
				}
				catch (WebException e) {
					string message = Bot.isRunningOnMono ? e.InnerException.Message : e.Message;
					if (Regex.IsMatch(message, ": \\(50[02349]\\) ")) {		// Remote problem
						if (errorCounter > Bot.retryTimes)
							throw;
						Console.Error.WriteLine(Bot.Msg("{0} Retrying in 60 seconds."), message);
						Thread.Sleep(60000);
					}
					else if (message.Contains("Section=ResponseStatusLine")) {	// Squid problem
						Bot.SwitchUnsafeHttpHeaderParsing(true);
						GetPaths();
						return;
					}
					else {
						Console.Error.WriteLine(Bot.Msg("Can't access the site.") + " " + message);
						throw;
					}
				}
			}
			site = webResp.ResponseUri.Scheme + "://" + webResp.ResponseUri.Authority;
			Regex wikiPathRE = new Regex("(?i)" + Regex.Escape(site) + "(/.+?/).+");
			Regex indexPathRE1 = new Regex("(?i)" + Regex.Escape(site) +
				"(/.+?/)index\\.php(\\?|/)");
			Regex indexPathRE2 = new Regex("(?i)href=\"(/[^\"\\s<>?]*?)index\\.php(\\?|/)");
			Regex xhtmlOptionsRE = new Regex("(?i)<html xmlns=\"([^\"]+)\" " +
				"xml:lang=\"([^\"]+)\" lang=\"([^\"]+)\" dir=\"([^\"]+)\">");
			string mainPageUri = webResp.ResponseUri.ToString();
			if (mainPageUri.Contains("/index.php?"))
				indexPath = indexPathRE1.Match(mainPageUri).Groups[1].ToString();
			else
				wikiPath = wikiPathRE.Match(mainPageUri).Groups[1].ToString();
			if (string.IsNullOrEmpty(indexPath) && string.IsNullOrEmpty(wikiPath) &&
				mainPageUri[mainPageUri.Length-1] != '/' &&
				Bot.CountMatches(mainPageUri, "/", false) == 3)
					wikiPath = "/";
			Stream respStream = webResp.GetResponseStream();
			if (webResp.ContentEncoding.ToLower().Contains("gzip"))
				respStream = new GZipStream(respStream, CompressionMode.Decompress);
			else if (webResp.ContentEncoding.ToLower().Contains("deflate"))
				respStream = new DeflateStream(respStream, CompressionMode.Decompress);
			StreamReader strmReader = new StreamReader(respStream, encoding);
			string src = strmReader.ReadToEnd();
			webResp.Close();
			indexPath = indexPathRE2.Match(src).Groups[1].ToString();
			xhtmlNSUri = xhtmlOptionsRE.Match(src).Groups[1].ToString();
			language = xhtmlOptionsRE.Match(src).Groups[2].ToString();
			langDirection = xhtmlOptionsRE.Match(src).Groups[4].ToString();
			Directory.CreateDirectory("Cache");
			File.WriteAllText(filePathName, wikiPath + "\r\n" + indexPath + "\r\n" + xhtmlNSUri +
				"\r\n" + language + "\r\n" + langDirection + "\r\n" + site, Encoding.UTF8);
		}

		/// <summary>Gets all MediaWiki messages and dumps them to XML file. This function is
		/// obsolete, it won't work with current versions of MediaWiki software.</summary>
		/// <param name="forceLoad">If true, the messages are updated unconditionally. Otherwise
		/// the messages are updated only if they are outdated.</param>
		public void GetMediaWikiMessages(bool forceLoad)
		{
			if (messages == null)
				messages = new PageList(this);
			string filePathName = "Cache" + Path.DirectorySeparatorChar +
				HttpUtility.UrlEncode(site.Replace("://", ".")) + ".xml";
			if (forceLoad == false && File.Exists(filePathName) &&
				(DateTime.Now - File.GetLastWriteTime(filePathName)).Days <= 90) {
				messages.FillAndLoadFromXMLDump(filePathName);
				return;
			}
			Console.WriteLine(Bot.Msg("Updating MediaWiki messages dump. Please, wait..."));
			PageList pl = new PageList(this);
			pl.FillFromAllPages("!", 8, false, 100000);
			File.Delete(filePathName);
			pl.SaveXMLDumpToFile(filePathName);
			messages.FillAndLoadFromXMLDump(filePathName);
			Console.WriteLine(Bot.Msg("MediaWiki messages dump updated successfully."));
		}

		/// <summary>Gets all MediaWiki messages from "Special:Allmessages" page and dumps
		/// them to HTM file.</summary>
		/// <param name="forceLoad">If true, the messages are updated unconditionally. Otherwise
		/// the messages are updated only if they are outdated.</param>
		public void GetMediaWikiMessagesEx(bool forceLoad)
		{
			if (messages == null)
				messages = new PageList(this);
			string filePathName = "Cache" + Path.DirectorySeparatorChar +
				HttpUtility.UrlEncode(site.Replace("://", ".")) + ".messages.htm";
			if (forceLoad == true || !File.Exists(filePathName) ||
				(DateTime.Now - File.GetLastWriteTime(filePathName)).Days  > 90) {
				Console.WriteLine(Bot.Msg("Updating MediaWiki messages dump. Please, wait..."));
				string res = site + indexPath + "index.php?title=Special:Allmessages";
				File.WriteAllText(filePathName, GetPageHTM(res), Encoding.UTF8);
				Console.WriteLine(Bot.Msg("MediaWiki messages dump updated successfully."));
			}
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(File.ReadAllText(filePathName, Encoding.UTF8));
			XmlNodeList nl1 = doc.DocumentElement.SelectNodes(
				".//ns:table[@id = 'allmessagestable']/ns:tr[@class = 'def' or @class = 'orig']/" +
				"ns:td/ns:a/ns:span", xmlNS);
			XmlNodeList nl2 = doc.DocumentElement.SelectNodes(
				".//ns:table[@id = 'allmessagestable']/ns:tr[@class = 'def' or @class = 'new']/" +
				"ns:td[last()]", xmlNS);
			if (nl1.Count == 0 || nl1.Count != nl2.Count)
				throw new WikiBotException(Bot.Msg("MediaWiki messages parsing failed."));
			for (int i = 0; i < nl1.Count; i++) {
				messages.Add(new Page(this, HttpUtility.HtmlDecode(nl1[i].InnerXml)));
				messages[messages.Count()-1].text = HttpUtility.HtmlDecode(nl2[i].InnerXml.Trim());
			}
		}

		/// <summary>Retrieves metadata and local namespace names from site.</summary>
		public void GetInfo()
		{
			try {
				langCulture = new CultureInfo(language, false);
			}
			catch (Exception) {
				langCulture = new CultureInfo("");
			}
			if (langCulture.Equals(CultureInfo.CurrentUICulture.Parent))
				regCulture = CultureInfo.CurrentUICulture;
			else {
				try {
					regCulture = CultureInfo.CreateSpecificCulture(language);
				}
				catch (Exception) {
					foreach (CultureInfo ci in
						CultureInfo.GetCultures(CultureTypes.SpecificCultures)) {
							if (langCulture.Equals(ci.Parent)) {
								regCulture = ci;
								break;
							}
					}
					if (regCulture == null)
						regCulture = CultureInfo.InvariantCulture;
				}
			}

			string src = GetPageHTM(site + indexPath + "index.php?title=Special:Export/" +
				DateTime.Now.Ticks.ToString("x"));
			XmlTextReader reader = new XmlTextReader(new StringReader(src));
			reader.WhitespaceHandling = WhitespaceHandling.None;
			reader.ReadToFollowing("sitename");
			name = reader.ReadString();
			reader.ReadToFollowing("generator");
			generator = reader.ReadString();
			ver = new Version(Regex.Replace(generator, @"[^\d\.]", ""));
			float.TryParse(ver.ToString(), NumberStyles.AllowDecimalPoint,
				new CultureInfo("en-US"), out version);
			reader.ReadToFollowing("case");
			capitalization = reader.ReadString();
			namespaces.Clear();
			while (reader.ReadToFollowing("namespace"))
				namespaces.Add(reader.GetAttribute("key"),
					HttpUtility.HtmlDecode(reader.ReadString()));
			reader.Close();
			namespaces.Remove("0");
			foreach (DictionaryEntry ns in namespaces) {
				if (!wikiNSpaces.ContainsKey(ns.Key) ||
					ns.Key.ToString() == "4" || ns.Key.ToString() == "5")
						wikiNSpaces[ns.Key] = ns.Value;
			}
			if (ver >= new Version(1,14)) {
				wikiNSpaces["6"] = "File";
				wikiNSpaces["7"] = "File talk";
			}
			wikiCategoryRE = new Regex(@"\[\[(?i)(((" + Regex.Escape(wikiNSpaces["14"].ToString()) +
				"|" + Regex.Escape(namespaces["14"].ToString()) + @"):(.+?))(\|(.+?))?)]]");
			wikiImageRE = new Regex(@"\[\[(?i)((File|Image" +
				"|" + Regex.Escape(namespaces["6"].ToString()) + @"):(.+?))(\|(.+?))*?]]");
			string namespacesStr = "";
			foreach (DictionaryEntry ns in namespaces)
				namespacesStr += Regex.Escape(ns.Value.ToString()) + "|";
			namespacesStr = namespacesStr.Replace("||", "|").Trim("|".ToCharArray());
			linkToPageRE3 = new Regex("<a href=\"[^\"]*?\" title=\"(" +
				Regex.Escape(namespaces["6"].ToString()) + ":[^\"]+?)\">");
			string redirectTag = "REDIRECT";
			switch(language) {		// Revised 2009-02-23
				case "ar": redirectTag += "|تحويل"; break;
				case "be": redirectTag += "|перанакіраваньне"; break;
				case "bg": redirectTag += "|виж"; break;
				case "bs": redirectTag += "|preusmjeri"; break;
				case "cy": redirectTag += "|ail-cyfeirio"; break;
				case "de": redirectTag += "|weiterleitung"; break;
				case "es": redirectTag += "|redireccíon"; break;
				case "et": redirectTag += "|suuna"; break;
				case "eu": redirectTag += "|bidali"; break;
				case "fi": redirectTag += "|uudelleenohjaus|ohjaus"; break;
				case "fr": redirectTag += "|redirection"; break;
				case "ga": redirectTag += "|athsheoladh"; break;
				case "he": redirectTag += "|הפניה"; break;
				case "hu": redirectTag += "|ÁTIRÁNYÍTÁS"; break;
				case "id": redirectTag += "|redirected|alih"; break;
				case "is": redirectTag += "|tilvísun"; break;
				case "it": redirectTag += "|redirezione"; break;
				case "ja": redirectTag += "|転送|リダイレクト|転送|リダイレクト"; break;
				case "ka": redirectTag += "|გადამისამართება"; break;
				case "nl": redirectTag += "|doorverwijzing"; break;
				case "nn": redirectTag += "|omdiriger"; break;
				case "pl": redirectTag += "|patrz|przekieruj|tam"; break;
				case "ru": redirectTag += "|перенаправление|перенапр"; break;
				case "sk": redirectTag += "|presmeruj"; break;
				case "sl": redirectTag += "|preusmeritev"; break;
				case "sr": redirectTag += "|преусмери"; break;
				case "sv": redirectTag += "|omdirigering"; break;
				case "tt": redirectTag += "|yünältü"; break;
				case "yi": redirectTag += "|ווייטערפירן"; break;
				default: redirectTag = "REDIRECT"; break;
			}
			redirectRE = new Regex(@"(?i)^#(?:" + redirectTag + @")\s*:?\s*\[\[(.+?)(\|.+)?]]",
				RegexOptions.Compiled);
			Console.WriteLine(Bot.Msg("Site: {0} ({1})"), name, generator);
			string botQueryUriStr = site + indexPath + "api.php?version";
			string respStr;
			try {
				respStr = GetPageHTM(botQueryUriStr);
				if (respStr.Contains("<title>MediaWiki API</title>")) {
					botQuery = true;
					Regex botQueryVersionsRE = new Regex(@"(?i)<b><i>\$" +
						@"Id: (\S+) (\d+) (.+?) \$</i></b>");
					foreach (Match m in botQueryVersionsRE.Matches(respStr))
						botQueryVersions[m.Groups[1].ToString()] = m.Groups[2].ToString();
				}
			}
			catch (WebException) {
				botQuery = false;
			}
			if (botQuery == false || !botQueryVersions.ContainsKey("ApiQueryCategoryMembers.php")) {
				botQueryUriStr = site + indexPath + "query.php";
				try {
					respStr = GetPageHTM(botQueryUriStr);
					if (respStr.Contains("<title>MediaWiki Query Interface</title>")) {
						botQuery = true;
						botQueryVersions["query.php"] = "Unknown";
					}
				}
				catch (WebException) {
					return;
				}
			}
		}

		/// <summary>Loads default English namespace names for site.</summary>
		public void LoadDefaults()
		{
			if (wikiNSpaces.Count != 0 && WMSites.Count != 0)
				return;

			string[] wikiNSNames = { "Media", "Special", "", "Talk", "User", "User talk", name,
				name + " talk", "Image", "Image talk", "MediaWiki", "MediaWiki talk", "Template",
				"Template talk", "Help", "Help talk", "Category", "Category talk" };
			for (int i=-2, j=0; i < 16; i++, j++)
				wikiNSpaces.Add(i.ToString(), wikiNSNames[j]);
			wikiNSpaces.Remove("0");

			WMSites.Add("w", "wikipedia");					WMSites.Add("wikt", "wiktionary");
			WMSites.Add("b", "wikibooks");					WMSites.Add("n", "wikinews");
			WMSites.Add("q", "wikiquote");					WMSites.Add("s", "wikisource");
			foreach (DictionaryEntry s in WMSites)
				WMSitesStr += s.Key + "|" + s.Value + "|";

			mediaWikiVars = new string[] { "currentmonth","currentmonthname","currentmonthnamegen",
				"currentmonthabbrev","currentday2","currentdayname","currentyear","currenttime",
				"currenthour","localmonth","localmonthname","localmonthnamegen","localmonthabbrev",
				"localday","localday2","localdayname","localyear","localtime","localhour",
				"numberofarticles","numberoffiles","sitename","server","servername","scriptpath",
				"pagename","pagenamee","fullpagename","fullpagenamee","namespace","namespacee",
				"currentweek","currentdow","localweek","localdow","revisionid","revisionday",
				"revisionday2","revisionmonth","revisionyear","revisiontimestamp","subpagename",
				"subpagenamee","talkspace","talkspacee","subjectspace","dirmark","directionmark",
				"subjectspacee","talkpagename","talkpagenamee","subjectpagename","subjectpagenamee",
				"numberofusers","rawsuffix","newsectionlink","numberofpages","currentversion",
				"basepagename","basepagenamee","urlencode","currenttimestamp","localtimestamp",
				"directionmark","language","contentlanguage","pagesinnamespace","numberofadmins",
				"currentday","pagesinns:ns","pagesinns:ns:r","numberofarticles:r","numberofpages:r",
				"numberoffiles:r", "numberofusers:r", "numberofadmins:r" };
			parserFunctions = new string[] { "ns:", "localurl:", "localurle:", "urlencode:",
				"anchorencode:", "fullurl:", "fullurle:",  "grammar:", "plural:", "lc:", "lcfirst:",
				"uc:", "ucfirst:", "formatnum:", "padleft:", "padright:", "#language:",
				"displaytitle:", "defaultsort:", "#if:", "#if:", "#switch:", "#ifexpr:" };
			templateModifiers = new string[] { ":", "int:", "msg:", "msgnw:", "raw:", "subst:" };
			iwikiLinksOrder1 = new string[] { "aa","af","ak","als","am","ang","ab","ar","an","arc",
				"roa-rup","frp","as","ast","gn","av","ay","az","bm","bn","zh-min-nan","map-bms","ba",
				"be","be-x-old","bh","bi","bar","bo","bs","br","bg","bxr","ca","cv","ceb","cs","ch",
				"ny","sn","tum","cho","co","za","cy","da","pdc","de","dv","nv","dz","mh","et","el",
				"eml","en","es","eo","eu","ee","fa","fo","fr","fy","ff","fur","ga","gv","gd","gl","ki",
				"glk","gu","got","zh-classical","hak","xal","ko","ha","haw","hy","hi","ho","hsb","hr",
				"io","ig","ilo","bpy","id","ia","ie","iu","ik","os","xh","zu","is","it","he","jv","kl",
				"kn","pam","ka","ks","csb","kk","kw","rw","ky","rn","sw","kv","kg","ht","kj","ku","lad",
				"lbe","lo","la","lv","lb","lt","lij","li","ln","jbo","lg","lmo","hu","mk","mg","ml",
				"mt","mi","mr","mzn","ms","cdo","mo","mn","mus","my","nah","na","fj","nl","nds-nl","cr",
				"ne","new","ja","nap","ce","pih","no","nn","nrm","nov","oc","or","om","ng","hz","ug",
				"uz","pa","pi","pag","pap","ps","km","pms","nds","pl","pt","ty","ksh","ro","rmy","rm",
				"qu","ru","se","sm","sa","sg","sc","sco","st","tn","sq","ru-sib","scn","si","simple",
				"sd","ss","sk","cu","sl","so","sr","sh","su","fi","sv","tl","ta","kab","roa-tara","tt",
				"te","tet","th","vi","ti","tg","tpi","to","chr","chy","ve","tr","tk","tw","udm","bug",
				"uk","ur","vec","vo","fiu-vro","wa","vls","war","wo","wuu","ts","ii","yi","yo","zh-yue",
				"cbk-zam","diq","zea","bat-smg","zh","zh-tw","zh-cn" };
			iwikiLinksOrder2 = new string[] { "aa","af","ak","als","am","ang","ab","ar","an","arc",
				"roa-rup","frp","as","ast","gn","av","ay","az","id","ms","bm","bn","zh-min-nan",
				"map-bms","jv","su","ban","ba","be","bh","bi","bo","bs","br","bug","bg","bxr","ca",
				"ceb","cv","cs","ch","ny","sn","tum","cho","co","za","cy","da","pdc","de","dv","nv",
				"dz","mh","et","na","el","eml","en","es","eo","eu","ee","to","fab","fa","fo","fr","fy",
				"ff","fur","ga","gv","sm","gd","gl","gay","ki","glk","gu","got","zh-classical","xal",
				"ko","ha","haw","hy","hi","ho","hsb","hr","io","ig","ilo","bpy","ia","ie","iu","ik",
				"os","xh","zu","is","it","he","kl","pam","kn","kr","ka","ks","csb","kk","kk-cn","kk-kz",
				"kw","rw","ky","rn","sw","kv","kg","ht","kj","ku","lad","lbe","lo","ltg","la","lv","lb",
				"lij","lt","li","ln","jbo","lg","lmo","hu","mk","mg","ml","mt","mi","mr","mzn","chm",
				"cdo","mo","mn","mus","my","nah","fj","nl","nds-nl","cr","ne","new","ja","nap","ce",
				"pih","no","nn","nrm","nov","oc","or","om","ng","hz","ug","uz","pa","pi","pag","pap",
				"ps","km","pms","nds","pl","pt","kk-tr","ty","ksh","ro","rmy","rm","qu","ru","se","sa",
				"sg","sc","sco","st","tn","sq","ru-sib","scn","si","simple","sd","ss","sk","sl","cu",
				"so","sr","sh","fi","sv","tl","ta","roa-tara","tt","te","tet","th","vi","ti","tlh","tg",
				"tpi","chr","chy","ve","tr","tk","tw","udm","uk","ur","vec","vo","fiu-vro","wa","vls",
				"war","wo","wuu","ts","ii","yi","yo","zh-yue","cbk-zam","diq","zea","bat-smg","zh",
				"zh-tw","zh-cn" };

			botQueryLists.Add("allpages", "ap");			botQueryLists.Add("alllinks", "al");
			botQueryLists.Add("allusers", "au");			botQueryLists.Add("backlinks", "bl");
			botQueryLists.Add("categorymembers", "cm");		botQueryLists.Add("embeddedin", "ei");
			botQueryLists.Add("imageusage", "iu");			botQueryLists.Add("logevents", "le");
			botQueryLists.Add("recentchanges", "rc");		botQueryLists.Add("usercontribs", "uc");
			botQueryLists.Add("watchlist", "wl");			botQueryLists.Add("exturlusage", "eu");
			botQueryProps.Add("info", "in");				botQueryProps.Add("revisions", "rv");
			botQueryProps.Add("links", "pl");				botQueryProps.Add("langlinks", "ll");
			botQueryProps.Add("images", "im");				botQueryProps.Add("imageinfo", "ii");
			botQueryProps.Add("templates", "tl");			botQueryProps.Add("categories", "cl");
			botQueryProps.Add("extlinks", "el");
		}

		/// <summary>Logs in and retrieves cookies.</summary>
		private void LogIn()
		{
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site + indexPath +
				"index.php?title=Special:Userlogin&action=submitlogin&type=login");
			string postData = string.Format("wpName={0}&wpPassword={1}&wpDomain={2}" +
				"&wpRemember=1&wpLoginattempt=Log+in", HttpUtility.UrlEncode(userName),
				HttpUtility.UrlEncode(userPass), HttpUtility.UrlEncode(userDomain));
			webReq.Method = "POST";
			webReq.ContentType = Bot.webContentType;
			webReq.UserAgent = Bot.botVer;
			webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
			webReq.UseDefaultCredentials = true;
			webReq.CookieContainer = new CookieContainer();
			webReq.AllowAutoRedirect = false;
			if (Bot.unsafeHttpHeaderParsingUsed == 0) {
				webReq.ProtocolVersion = HttpVersion.Version10;
				webReq.KeepAlive = false;
			}
			byte[] postBytes = encoding.GetBytes(postData);
			webReq.ContentLength = postBytes.Length;
			Stream reqStrm = webReq.GetRequestStream();
			reqStrm.Write(postBytes, 0, postBytes.Length);
			reqStrm.Close();
			HttpWebResponse webResp = null;
			for (int errorCounter = 0; errorCounter <= Bot.retryTimes; errorCounter++) {
				try {
					webResp = (HttpWebResponse)webReq.GetResponse();
					break;
				}
				catch (WebException e) {
					string message = Bot.isRunningOnMono ? e.InnerException.Message : e.Message;
					if (Regex.IsMatch(message, ": \\(50[02349]\\) ")) {		// Remote problem
						if (errorCounter > Bot.retryTimes)
							throw;
						Console.Error.WriteLine(Bot.Msg("{0} Retrying in 60 seconds."), message);
						Thread.Sleep(60000);
					}
					else if (message.Contains("Section=ResponseStatusLine")) {	// Squid problem
						Bot.SwitchUnsafeHttpHeaderParsing(true);
						LogIn();
						return;
					}
					else
						throw;
				}
			}
			foreach (Cookie cookie in webResp.Cookies)
				cookies.Add(cookie);
			StreamReader strmReader = new StreamReader(webResp.GetResponseStream());
			string respStr = strmReader.ReadToEnd();
			if (respStr.Contains("<div class=\"errorbox\">"))
				throw new WikiBotException(
					"\n\n" + Bot.Msg("Login failed. Check your username and password.") + "\n");
			strmReader.Close();
			webResp.Close();
			Console.WriteLine(Bot.Msg("Logged in as {0}."), userName);
		}

		/// <summary>Gets the list of Wikimedia Foundation wiki sites and ISO 639-1
		/// language codes, used as prefixes.</summary>
		public void GetWikimediaWikisList()
		{
			Uri wikimediaMeta = new Uri("http://meta.wikimedia.org/wiki/Special:SiteMatrix");
			string respStr = Bot.GetWebResource(wikimediaMeta, "");
			Regex langCodeRE = new Regex("<a id=\"([^\"]+?)\"");
			Regex siteCodeRE = new Regex("<li><a href=\"[^\"]+?\">([^\\s]+?)<");
			MatchCollection langMatches = langCodeRE.Matches(respStr);
			MatchCollection siteMatches = siteCodeRE.Matches(respStr);
			foreach(Match m in langMatches)
				WMLangsStr += Regex.Escape(HttpUtility.HtmlDecode(m.Groups[1].ToString())) + "|";
			WMLangsStr = WMLangsStr.Remove(WMLangsStr.Length - 1);
			foreach(Match m in siteMatches)
				WMSitesStr += Regex.Escape(HttpUtility.HtmlDecode(m.Groups[1].ToString())) + "|";
			WMSitesStr += "m";
			iwikiLinkRE = new Regex(@"(?i)\[\[((" + WMLangsStr + "):(.+?))]]\r?\n?");
			iwikiDispLinkRE = new Regex(@"(?i)\[\[:((" + WMLangsStr + "):(.+?))]]");
			sisterWikiLinkRE = new Regex(@"(?i)\[\[((" + WMSitesStr + "):(.+?))]]");
		}

		/// <summary>This internal function gets the hypertext markup (HTM) of wiki-page.</summary>
		/// <param name="pageURL">Absolute or relative URL of page to get.</param>
		/// <returns>Returns HTM source code.</returns>
		public string GetPageHTM(string pageURL)
		{
			return PostDataAndGetResultHTM(pageURL, "");
		}

		/// <summary>This internal function posts specified string to requested resource
		/// and gets the result hypertext markup (HTM).</summary>
		/// <param name="pageURL">Absolute or relative URL of page to get.</param>
		/// <param name="postData">String to post to site with web request.</param>
		/// <returns>Returns code of hypertext markup (HTM).</returns>
		public string PostDataAndGetResultHTM(string pageURL, string postData)
		{
			if (string.IsNullOrEmpty(pageURL))
				throw new WikiBotException(Bot.Msg("No URL specified."));
			if (!pageURL.StartsWith(site))
				pageURL = site + pageURL;
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(pageURL);
			webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
			webReq.UseDefaultCredentials = true;
			webReq.ContentType = Bot.webContentType;
			webReq.UserAgent = Bot.botVer;
			webReq.CookieContainer = cookies;
			if (Bot.unsafeHttpHeaderParsingUsed == 0) {
				webReq.ProtocolVersion = HttpVersion.Version10;
				webReq.KeepAlive = false;
			}
			webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
			if (!string.IsNullOrEmpty(postData)) {
				webReq.Method = "POST";
				//webReq.Timeout = 180000;
				byte[] postBytes = Encoding.UTF8.GetBytes(postData);
				webReq.ContentLength = postBytes.Length;
				Stream reqStrm = webReq.GetRequestStream();
				reqStrm.Write(postBytes, 0, postBytes.Length);
				reqStrm.Close();
			}
			HttpWebResponse webResp = null;
			for (int errorCounter = 0; errorCounter <= Bot.retryTimes; errorCounter++) {
				try {
					webResp = (HttpWebResponse)webReq.GetResponse();
					break;
				}
				catch (WebException e) {
					string message = Bot.isRunningOnMono ? e.InnerException.Message : e.Message;
					if (Regex.IsMatch(message, ": \\(50[02349]\\) ")) {		// Remote problem
						if (errorCounter > Bot.retryTimes)
							throw;
						Console.Error.WriteLine(Bot.Msg("{0} Retrying in 60 seconds."), message);
						Thread.Sleep(60000);
					}
					else if (message.Contains("Section=ResponseStatusLine")) {	// Squid problem
						Bot.SwitchUnsafeHttpHeaderParsing(true);
						//Console.Write("|");
						return PostDataAndGetResultHTM(pageURL, postData);
					}
					else
						throw;
				}
			}
			Stream respStream = webResp.GetResponseStream();
			if (webResp.ContentEncoding.ToLower().Contains("gzip"))
				respStream = new GZipStream(respStream, CompressionMode.Decompress);
			else if (webResp.ContentEncoding.ToLower().Contains("deflate"))
				respStream = new DeflateStream(respStream, CompressionMode.Decompress);
			StreamReader strmReader = new StreamReader(respStream, encoding);
			string respStr = strmReader.ReadToEnd();
			strmReader.Close();
			webResp.Close();
			return respStr;
		}

		/// <summary>This internal loads XML from specified URI, makes XPath query and returns
		/// XPathNodeIterator for selected nodes.</summary>
		/// <param name="uri">URI of xml file.</param>
		/// <param name="postData">String to post to URI using HTTP POST method.</param>
		/// <param name="xpathQuery">XPath expression.</param>
		/// <returns>XPathNodeIterator object.</returns>
		public XPathNodeIterator GetXMLNodes(string uri, string postData, string xpathQuery)
		{
			string src = PostDataAndGetResultHTM(uri, postData);
			StringReader strReader = new StringReader(src);
			XPathDocument doc = new XPathDocument(strReader);
			strReader.Close();
			XPathNavigator nav = doc.CreateNavigator();
			return nav.Select(xpathQuery, xmlNS);
		}

		/// <summary>This internal function removes the namespace prefix from page title.</summary>
		/// <param name="pageTitle">Page title to remove prefix from.</param>
		/// <param name="nsIndex">Index of namespace to remove. If this parameter is 0,
		/// any found namespace prefix is removed.</param>
		/// <returns>Page title without prefix.</returns>
		public string RemoveNSPrefix(string pageTitle, int nsIndex)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			if (nsIndex != 0) {
				if (wikiNSpaces[nsIndex.ToString()] != null)
					pageTitle = Regex.Replace(pageTitle, "(?i)^" +
						Regex.Escape(wikiNSpaces[nsIndex.ToString()].ToString()) + ":", "");
				if (namespaces[nsIndex.ToString()] != null)
					pageTitle = Regex.Replace(pageTitle, "(?i)^" +
						Regex.Escape(namespaces[nsIndex.ToString()].ToString()) + ":", "");
				return pageTitle;
			}
			foreach (DictionaryEntry ns in wikiNSpaces) {
				if (ns.Value == null)
					continue;
				pageTitle = Regex.Replace(pageTitle, "(?i)^" +
					Regex.Escape(ns.Value.ToString()) + ":", "");
			}
			foreach (DictionaryEntry ns in namespaces) {
				if (ns.Value == null)
					continue;
				pageTitle = Regex.Replace(pageTitle, "(?i)^" +
					Regex.Escape(ns.Value.ToString()) + ":", "");
			}
			return pageTitle;
		}

		/// <summary>Function changes default English namespace prefixes to correct local prefixes
		/// (e.g. for German wiki-sites it changes "Category:..." to "Kategorie:...").</summary>
		/// <param name="pageTitle">Page title to correct prefix in.</param>
		/// <returns>Page title with corrected prefix.</returns>
		public string CorrectNSPrefix(string pageTitle)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			foreach (DictionaryEntry ns in wikiNSpaces) {
				if (ns.Value == null)
					continue;
				if (Regex.IsMatch(pageTitle, "(?i)" + Regex.Escape(ns.Value.ToString()) + ":"))
					pageTitle = namespaces[ns.Key] + pageTitle.Substring(pageTitle.IndexOf(":"));
			}
			return pageTitle;
		}

		/// <summary>Shows names and integer keys of local and default namespaces.</summary>
		public void ShowNamespaces()
		{
			foreach (DictionaryEntry ns in namespaces) {
				Console.WriteLine(ns.Key.ToString() + "\t" + ns.Value.ToString().PadRight(20) +
					"\t" + wikiNSpaces[ns.Key.ToString()]);
			}
		}
	}

	/// <summary>Class defines wiki page object.</summary>
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class Page
	{
		/// <summary>Page title.</summary>
		public string title;
		/// <summary>Page text.</summary>
		public string text;
		/// <summary>Page ID in internal MediaWiki database.</summary>
		public string pageID;
		/// <summary>Username or IP-address of last page contributor.</summary>
		public string lastUser;
		/// <summary>Last contributor ID in internal MediaWiki database.</summary>
		public string lastUserID;
		/// <summary>Page revision ID in the internal MediaWiki database.</summary>
		public string lastRevisionID;
		/// <summary>True, if last edit was minor edit.</summary>
		public bool lastMinorEdit;
		/// <summary>Amount of bytes, modified during last edit.</summary>
		public int lastBytesModified;
		/// <summary>Last edit comment.</summary>
		public string comment;
		/// <summary>Date and time of last edit expressed in UTC (Coordinated Universal Time).
		/// Call "timestamp.ToLocalTime()" to convert to local time if it is necessary.</summary>
		public DateTime timestamp;
		/// <summary>True, if this page is in bot account's watchlist. Call GetEditSessionData
		/// function to get the actual state of this property.</summary>
		public bool watched;
		/// <summary>This edit session time attribute is used to edit pages.</summary>
		public string editSessionTime;
		/// <summary>This edit session token attribute is used to edit pages.</summary>
		public string editSessionToken;
		/// <summary>Site, on which the page is.</summary>
		public Site site;

		/// <summary>This constructor creates Page object with specified title and specified
		/// Site object. This is preferable constructor. When constructed, new Page object doesn't
		/// contain text. Use Load() method to get text from live wiki. Or use LoadEx() to get
		/// both text and metadata via XML export interface.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <param name="title">Page title as string.</param>
		/// <returns>Returns Page object.</returns>
		public Page(Site site, string title)
		{
			this.title = title;
			this.site = site;
		}

		/// <summary>This constructor creates empty Page object with specified Site object,
		/// but without title. Avoid using this constructor needlessly.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <returns>Returns Page object.</returns>
		public Page(Site site)
		{
			this.site = site;
		}

		/// <summary>This constructor creates Page object with specified title. Site object
		/// with default properties is created internally and logged in. Constructing
		/// new Site object is too slow, don't use this constructor needlessly.</summary>
		/// <param name="title">Page title as string.</param>
		/// <returns>Returns Page object.</returns>
		public Page(string title)
		{
			this.site = new Site();
			this.title = title;
		}

		/// <summary>This constructor creates Page object with specified page's numeric revision ID
		/// (also called "oldid"). Page title is retrieved automatically
		/// in this constructor.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <param name="revisionID">Page's numeric revision ID (also called "oldid").</param>
		/// <returns>Returns Page object.</returns>
		public Page(Site site, Int64 revisionID)
		{
			if (revisionID <= 0)
				throw new ArgumentOutOfRangeException("revisionID",
					Bot.Msg("Revision ID must be positive."));
			this.site = site;
			lastRevisionID = revisionID.ToString();
			GetTitle();
		}

		/// <summary>This constructor creates Page object with specified page's numeric revision ID
		/// (also called "oldid"). Page title is retrieved automatically in this constructor.
		/// Site object with default properties is created internally and logged in. Constructing
		/// new Site object is too slow, don't use this constructor needlessly.</summary>
		/// <param name="revisionID">Page's numeric revision ID (also called "oldid").</param>
		/// <returns>Returns Page object.</returns>
		public Page(Int64 revisionID)
		{
			if (revisionID <= 0)
				throw new ArgumentOutOfRangeException("revisionID",
					Bot.Msg("Revision ID must be positive."));
			this.site = new Site();
			lastRevisionID = revisionID.ToString();
			GetTitle();
		}

		/// <summary>This constructor creates empty Page object without title. Site object with
		/// default properties is created internally and logged in. Constructing new Site object
		/// is too slow, avoid using this constructor needlessly.</summary>
		/// <returns>Returns Page object.</returns>
		public Page()
		{
			this.site = new Site();
		}

		/// <summary>Loads actual page text for live wiki site via raw web interface.
		/// If Page.lastRevisionID is specified, the function gets that specified
		/// revision.</summary>
		public void Load()
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to load."));
			string res = site.site + site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(title) +
				(string.IsNullOrEmpty(lastRevisionID) ? "" : "&oldid=" + lastRevisionID) +
				"&redirect=no&action=raw&ctype=text/plain&dontcountme=s";
			try {
				text = site.GetPageHTM(res);
			}
			catch (WebException e) {
				string message = Bot.isRunningOnMono ? e.InnerException.Message : e.Message;
				if (message.Contains(": (404) ")) {		// Not Found
					Console.Error.WriteLine(Bot.Msg("Page \"{0}\" doesn't exist."), title);
					text = "";
					return;
				}
				else
					throw;
			}
			Console.WriteLine(Bot.Msg("Page \"{0}\" loaded successfully."), title);
		}

		/// <summary>Loads page text and metadata via XML export interface. It is slower,
		/// than Load(), don't use it if you don't need page metadata (page id, timestamp,
		/// comment, last contributor, minor edit mark).</summary>
		public void LoadEx()
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to load."));
			string res = site.site + site.indexPath + "index.php?title=Special:Export/" +
				HttpUtility.UrlEncode(title);
			string src = site.GetPageHTM(res);
			ParsePageXML(src);
		}

		/// <summary>This internal function parses XML export source
		/// to get page text and metadata.</summary>
		/// <param name="xmlSrc">XML export source code.</param>
		public void ParsePageXML(string xmlSrc)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlSrc);
			if (doc.GetElementsByTagName("page").Count == 0) {
				Console.Error.WriteLine(Bot.Msg("Page \"{0}\" doesn't exist."), title);
				return;
			}
			text = doc.GetElementsByTagName("text")[0].InnerText;
			pageID = doc.GetElementsByTagName("id")[0].InnerText;
			if (doc.GetElementsByTagName("username").Count != 0) {
				lastUser = doc.GetElementsByTagName("username")[0].InnerText;
				lastUserID = doc.GetElementsByTagName("id")[2].InnerText;
			}
			else
				lastUser = doc.GetElementsByTagName("ip")[0].InnerText;
			lastRevisionID = doc.GetElementsByTagName("id")[1].InnerText;
			if (doc.GetElementsByTagName("comment").Count != 0)
				comment = doc.GetElementsByTagName("comment")[0].InnerText;
			timestamp = DateTime.Parse(doc.GetElementsByTagName("timestamp")[0].InnerText);
			timestamp = timestamp.ToUniversalTime();
			lastMinorEdit = (doc.GetElementsByTagName("minor").Count != 0) ? true : false;
			if (string.IsNullOrEmpty(title))
				title = doc.GetElementsByTagName("title")[0].InnerText;
			else
				Console.WriteLine(Bot.Msg("Page \"{0}\" loaded successfully."), title);
		}

		/// <summary>Loads page text from the specified UTF8-encoded file.</summary>
		/// <param name="filePathName">Path and name of the file.</param>
		public void LoadFromFile(string filePathName)
		{
			StreamReader strmReader = new StreamReader(filePathName);
			text = strmReader.ReadToEnd();
			strmReader.Close();
			Console.WriteLine(
				Bot.Msg("Text for page \"{0}\" successfully loaded from \"{1}\" file."),
				title, filePathName);
		}

		/// <summary>This function is used internally to gain rights to edit page
		/// on a live wiki site.</summary>
		public void GetEditSessionData()
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(
					Bot.Msg("No title specified for page to get edit session data."));
			string src = site.GetPageHTM(site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(title) + "&action=edit");
			editSessionTime = site.editSessionTimeRE1.Match(src).Groups[1].ToString();
			editSessionToken = site.editSessionTokenRE1.Match(src).Groups[1].ToString();
			if (string.IsNullOrEmpty(editSessionToken))
				editSessionToken = site.editSessionTokenRE2.Match(src).Groups[1].ToString();
			watched = Regex.IsMatch(src, "<a href=\"[^\"]+&(amp;)?action=unwatch\"");
		}

		/// <summary>This function is used internally to gain rights to edit page on a live wiki
		/// site. The function queries rights, using bot interface, thus saving traffic.</summary>
		public void GetEditSessionDataEx()
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(
					Bot.Msg("No title specified for page to get edit session data."));
			string src = site.GetPageHTM(site.indexPath + "api.php?action=query&prop=info" +
				"&format=xml&intoken=edit&titles=" + HttpUtility.UrlEncode(title));
			editSessionToken = site.editSessionTokenRE3.Match(src).Groups[1].ToString();
			if (editSessionToken == "+\\")
				editSessionToken = "";
			editSessionTime = site.editSessionTimeRE3.Match(src).Groups[1].ToString();
			if (!string.IsNullOrEmpty(editSessionTime))
				editSessionTime = Regex.Replace(editSessionTime, "\\D", "");
			if (string.IsNullOrEmpty(editSessionTime) && !string.IsNullOrEmpty(editSessionToken))
				editSessionTime = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss");
			if (site.watchList == null) {
				site.watchList = new PageList(site);
				site.watchList.FillFromWatchList();
			}
			watched = site.watchList.Contains(title);
		}

		/// <summary>Retrieves the title for this Page object using page's numeric revision ID
		/// (also called "oldid"), stored in "lastRevisionID" object's property. Make sure that
		/// "lastRevisionID" property is set before calling this function. Use this function
		/// when working with old revisions to detect if the page was renamed at some
		/// moment.</summary>
		public void GetTitle()
		{
			if (string.IsNullOrEmpty(lastRevisionID))
				throw new WikiBotException(
					Bot.Msg("No revision ID specified for page to get title for."));
			string src = site.GetPageHTM(site.site + site.indexPath +
				"index.php?oldid=" + lastRevisionID);
			title = Regex.Match(src, "<h1 (?:id=\"firstHeading\" )?class=\"firstHeading\">" +
				"(.+?)</h1>").Groups[1].ToString();
		}

		/// <summary>Saves current contents of page.text on live wiki site. Uses default bot
		/// edit comment and default minor edit mark setting ("true" in most cases)/</summary>
		public void Save()
		{
			Save(text, Bot.editComment, Bot.isMinorEdit);
		}

		/// <summary>Saves specified text in page on live wiki. Uses default bot
		/// edit comment and default minor edit mark setting ("true" in most cases).</summary>
		/// <param name="newText">New text for this page.</param>
		public void Save(string newText)
		{
			Save(newText, Bot.editComment, Bot.isMinorEdit);
		}

		/// <summary>Saves current page.text contents on live wiki site.</summary>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>
		public void Save(string comment, bool isMinorEdit)
		{
			Save(text, comment, isMinorEdit);
		}

		/// <summary>Saves specified text in page on live wiki.</summary>
		/// <param name="newText">New text for this page.</param>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>
		public void Save(string newText, string comment, bool isMinorEdit)
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to save text to."));
			if (string.IsNullOrEmpty(newText) && string.IsNullOrEmpty(text))
				throw new WikiBotException(Bot.Msg("No text specified for page to save."));
			if (site.botQuery == true &&
				(site.ver.Major > 1 || (site.ver.Major == 1 && site.ver.Minor >= 15)))
					GetEditSessionDataEx();
			else
				GetEditSessionData();
			if (string.IsNullOrEmpty(editSessionTime) || string.IsNullOrEmpty(editSessionToken))
				throw new WikiBotException(
					string.Format(Bot.Msg("Insufficient rights to edit page \"{0}\"."), title));
			string postData = string.Format("wpSection=&wpStarttime={0}&wpEdittime={1}" +
				"&wpScrolltop=&wpTextbox1={2}&wpSummary={3}&wpSave=Save%20Page" +
				"&wpEditToken={4}{5}{6}",
				DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss"),
				HttpUtility.UrlEncode(editSessionTime),
				HttpUtility.UrlEncode(newText),
				HttpUtility.UrlEncode(comment),
				HttpUtility.UrlEncode(editSessionToken),
				watched ? "&wpWatchthis=1" : "",
				isMinorEdit ? "&wpMinoredit=1" : "");
			if (Bot.askConfirm) {
				Console.Write("\n\n" +
					Bot.Msg("The following text is going to be saved on page \"{0}\":"), title);
				Console.Write("\n\n" + text + "\n\n");
				if(!Bot.UserConfirms())
					return;
			}
			string respStr = site.PostDataAndGetResultHTM(site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(title) + "&action=submit", postData);
			if (respStr.Contains(" name=\"wpTextbox2\""))
				throw new WikiBotException(string.Format(
					Bot.Msg("Edit conflict occurred while trying to savе page \"{0}\"."), title));
			if (respStr.Contains("<div class=\"permissions-errors\">"))
				throw new WikiBotException(
					string.Format(Bot.Msg("Insufficient rights to edit page \"{0}\"."), title));
			if (respStr.Contains("input name=\"wpCaptchaWord\" id=\"wpCaptchaWord\""))
				throw new WikiBotException(
					string.Format(Bot.Msg("Error occurred when saving page \"{0}\": " +
					"Bot operation is not allowed for this account at \"{1}\" site."),
					title, site.site));
			if (site.editSessionTokenRE1.IsMatch(respStr)) {
				Thread.Sleep(5000);
				Save(newText, comment, isMinorEdit);
			}
			else {
				Console.WriteLine(Bot.Msg("Page \"{0}\" saved successfully."), title);
				text = newText;
			}
		}

		/// <summary>Undoes the last edit, so page text reverts to previous contents.
		/// The function doesn't affect other actions like renaming.</summary>
		/// <param name="comment">Revert comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (pass true for minor edit).</param>
		public void Revert(string comment, bool isMinorEdit)
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to revert."));
			PageList pl = new PageList(site);
			if (Bot.useBotQuery == true && site.botQuery == true &&
				site.botQueryVersions.ContainsKey("ApiQueryRevisions.php"))
					pl.FillFromPageHistoryEx(title, 2, false);
			else
				pl.FillFromPageHistory(title, 2);
			if (pl.Count() != 2) {
				Console.Error.WriteLine(Bot.Msg("Can't revert page \"{0}\"."), title);
				return;
			}
			pl[1].Load();
			Save(pl[1].text, comment, isMinorEdit);
			Console.WriteLine(Bot.Msg("Page \"{0}\" was reverted."), title);
		}

		/// <summary>Undoes all last edits of last page contributor, so page text reverts to
		///  previous contents. The function doesn't affect other operations
		/// like renaming or protecting.</summary>
		/// <param name="comment">Revert comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (pass true for minor edit).</param>
		public void UndoLastEdits(string comment, bool isMinorEdit)
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to revert."));
			PageList pl = new PageList(site);
			string lastEditor = "";
			for (int i = 50; i <= 5000; i *= 10) {
				if (Bot.useBotQuery == true && site.botQuery == true &&
					site.botQueryVersions.ContainsKey("ApiQueryRevisions.php"))
						pl.FillFromPageHistoryEx(title, i, false);
				else
					pl.FillFromPageHistory(title, i);
				lastEditor = pl[0].lastUser;
				foreach (Page p in pl)
					if (p.lastUser != lastEditor) {
						p.Load();
						Save(p.text, comment, isMinorEdit);
						Console.WriteLine(
							Bot.Msg("Last edits of page \"{0}\" by user {1} were undone."),
							title, lastEditor);
						return;
					}
				pl.Clear();
			}
			Console.Error.WriteLine(Bot.Msg("Can't undo last edits of page \"{0}\" by user {1}."),
				title, lastEditor);
		}

		/// <summary>Protects or unprotects the page, so only authorized group of users can edit or
		/// rename it. Changing page protection mode requires administrator (sysop)
		/// rights.</summary>
		/// <param name="editMode">Protection mode for editing this page (0 = everyone allowed
		/// to edit, 1 = only registered users are allowed, 2 = only administrators are allowed
		/// to edit).</param>
		/// <param name="renameMode">Protection mode for renaming this page (0 = everyone allowed to
		/// rename, 1 = only registered users are allowed, 2 = only administrators
		/// are allowed).</param>
		/// <param name="cascadeMode">In cascading mode, all the pages, included into this page
		/// (e.g., templates or images) are also automatically protected.</param>
		/// <param name="expiryDate">Date and time, expressed in UTC, when protection expires
		/// and page becomes unprotected. Use DateTime.ToUniversalTime() method to convert local
		/// time to UTC, if necessary. Pass DateTime.MinValue to make protection indefinite.</param>
		/// <param name="reason">Reason for protecting this page.</param>
		public void Protect(int editMode, int renameMode, bool cascadeMode,
			DateTime expiryDate, string reason)
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to protect."));
			string errorMsg =
				Bot.Msg("Only values 0, 1 and 2 are accepted. Please, consult documentation.");
			if (editMode > 2 || editMode < 0)
				throw new ArgumentOutOfRangeException("editMode", errorMsg);
			if (renameMode > 2 || renameMode < 0)
				throw new ArgumentOutOfRangeException("renameMode", errorMsg);
			if (expiryDate != DateTime.MinValue	&& expiryDate < DateTime.Now)
				throw new ArgumentOutOfRangeException("expiryDate",
					Bot.Msg("Protection expiry date must be hereafter."));
			string res = site.site + site.indexPath +
				"index.php?title=" + HttpUtility.UrlEncode(title) + "&action=protect";
			string src = site.GetPageHTM(res);
			editSessionTime = site.editSessionTimeRE1.Match(src).Groups[1].ToString();
			editSessionToken = site.editSessionTokenRE1.Match(src).Groups[1].ToString();
			if (string.IsNullOrEmpty(editSessionToken))
				editSessionToken = site.editSessionTokenRE2.Match(src).Groups[1].ToString();
			if (string.IsNullOrEmpty(editSessionToken)) {
				Console.Error.WriteLine(
					Bot.Msg("Unable to change protection mode for page \"{0}\"."), title);
				return;
			}
			string postData = string.Format("mwProtect-level-edit={0}&mwProtect-level-move={1}" +
				"&mwProtect-reason={2}&wpEditToken={3}&mwProtect-expiry={4}{5}",
				HttpUtility.UrlEncode(
					editMode == 2 ? "sysop" : editMode == 1 ? "autoconfirmed" : ""),
				HttpUtility.UrlEncode(
					renameMode == 2 ? "sysop" : renameMode == 1 ? "autoconfirmed" : ""),
				HttpUtility.UrlEncode(reason),
				HttpUtility.UrlEncode(editSessionToken),
				expiryDate == DateTime.MinValue ? "" : expiryDate.ToString("u"),
				cascadeMode == true ? "&mwProtect-cascade=1" : "");
			string respStr = site.PostDataAndGetResultHTM(site.indexPath +
				"index.php?title=" + HttpUtility.UrlEncode(title) + "&action=protect", postData);
			if (string.IsNullOrEmpty(respStr)) {
				Console.Error.WriteLine(
					Bot.Msg("Unable to change protection mode for page \"{0}\"."), title);
				return;
			}
			Console.WriteLine(
				Bot.Msg("Protection mode for page \"{0}\" changed successfully."), title);
		}

		/// <summary>Adds page to bot account's watchlist.</summary>
		public void Watch()
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to watch."));
			string res = site.site + site.indexPath +
				"index.php?title=" + HttpUtility.UrlEncode(title) + "&action=watch";
			site.GetPageHTM(res);
			watched = true;
			Console.WriteLine(Bot.Msg("Page \"{0}\" added to watchlist."), title);
		}

		/// <summary>Removes page from bot account's watchlist.</summary>
		public void Unwatch()
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to unwatch."));
			string res = site.site + site.indexPath +
				"index.php?title=" + HttpUtility.UrlEncode(title) + "&action=unwatch";
			site.GetPageHTM(res);
			watched = false;
			Console.WriteLine(Bot.Msg("Page \"{0}\" was removed from watchlist."), title);
		}

		/// <summary>This function opens page text in Microsoft Word for editing.
		/// Just close Word after editing, and the revised text will appear in
		/// Page.text variable.</summary>
		/// <remarks>Appropriate PIAs (Primary Interop Assemblies) for available MS Office
		/// version must be installed and referenced in order to use this function. Follow
		/// instructions in "Compile and Run.bat" file to reference PIAs properly in compilation
		/// command, and then recompile the framework. Redistributable PIAs can be downloaded from
		/// http://www.microsoft.com/downloads/results.aspx?freetext=Office%20PIA</remarks>
		public void ReviseInMSWord()
		{
		  #if MS_WORD_INTEROP
			if (string.IsNullOrEmpty(text))
				throw new WikiBotException(Bot.Msg("No text on page to revise in Microsoft Word."));
			Microsoft.Office.Interop.Word.Application app =
				new Microsoft.Office.Interop.Word.Application();
			app.Visible = true;
			object mv = System.Reflection.Missing.Value;
			object template = mv;
			object newTemplate = mv;
			object documentType = Microsoft.Office.Interop.Word.WdDocumentType.wdTypeDocument;
			object visible = true;
			Microsoft.Office.Interop.Word.Document doc =
				app.Documents.Add(ref template, ref newTemplate, ref documentType, ref visible);
			doc.Words.First.InsertBefore(text);
			text = null;
			Microsoft.Office.Interop.Word.DocumentEvents_Event docEvents =
				(Microsoft.Office.Interop.Word.DocumentEvents_Event) doc;
			docEvents.Close +=
				new Microsoft.Office.Interop.Word.DocumentEvents_CloseEventHandler(
					delegate { text = doc.Range(ref mv, ref mv).Text; doc.Saved = true; } );
			app.Activate();
			while (text == null);
			text = Regex.Replace(text, "\r(?!\n)", "\r\n");
			app = null;
			doc = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Console.WriteLine(
				Bot.Msg("Text of \"{0}\" page was revised in Microsoft Word."), title);
		  #else
			throw new WikiBotException(Bot.Msg("Page.ReviseInMSWord() function requires MS " +
				"Office PIAs to be installed and referenced. Please, see remarks in function's " +
				"documentation in \"Documentation.chm\" file for additional instructions.\n"));
		  #endif
		}

		/// <summary>Uploads local image to wiki site. Function also works with non-image files.
		/// Note: uploaded image title (wiki page title) will be the same as title of this Page
		/// object, not the title of source file.</summary>
		/// <param name="filePathName">Path and name of local file.</param>
		/// <param name="description">File (image) description.</param>
		/// <param name="license">File license type (may be template title). Used only on
		/// some wiki sites. Pass empty string, if the wiki site doesn't require it.</param>
		/// <param name="copyStatus">File (image) copy status. Used only on some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>
		/// <param name="source">File (image) source. Used only on some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>
		public void UploadImage(string filePathName, string description,
			string license, string copyStatus, string source)
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for image to upload."));
			if (!File.Exists(filePathName))
				throw new WikiBotException(
					string.Format(Bot.Msg("Image file \"{0}\" doesn't exist."), filePathName));
			if (Path.GetFileNameWithoutExtension(filePathName).Length < 3)
				throw new WikiBotException(string.Format(Bot.Msg("Name of file \"{0}\" must " +
					"contain at least 3 characters (excluding extension) for successful upload."),
					filePathName));
			Console.WriteLine(Bot.Msg("Uploading image \"{0}\"..."), title);
			string targetName = site.RemoveNSPrefix(title, 6);
			targetName = Bot.Capitalize(targetName);
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site.site +
				site.indexPath + "index.php?title=" + site.namespaces["-1"].ToString() + ":Upload");
			webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
			webReq.UseDefaultCredentials = true;
			webReq.Method = "POST";
			string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
			webReq.ContentType = "multipart/form-data; boundary=" + boundary;
			webReq.UserAgent = Bot.botVer;
			webReq.CookieContainer = site.cookies;
			if (Bot.unsafeHttpHeaderParsingUsed == 0) {
				webReq.ProtocolVersion = HttpVersion.Version10;
				webReq.KeepAlive = false;
			}
			webReq.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(
				System.Net.Cache.HttpRequestCacheLevel.Refresh);
			StringBuilder sb = new StringBuilder();
			string ph = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"";
			sb.Append(ph + "wpIgnoreWarning\"\r\n\r\n1\r\n");
			sb.Append(ph + "wpDestFile\"\r\n\r\n" + targetName + "\r\n");
			sb.Append(ph + "wpUploadAffirm\"\r\n\r\n1\r\n");
			sb.Append(ph + "wpWatchthis\"\r\n\r\n0\r\n");
			sb.Append(ph + "wpUploadCopyStatus\"\r\n\r\n" + copyStatus + "\r\n");
			sb.Append(ph + "wpUploadSource\"\r\n\r\n" + source + "\r\n");
			sb.Append(ph + "wpUpload\"\r\n\r\n" + "upload bestand" + "\r\n");
			sb.Append(ph + "wpLicense\"\r\n\r\n" + license + "\r\n");
			sb.Append(ph + "wpUploadDescription\"\r\n\r\n" + description + "\r\n");
			sb.Append(ph + "wpUploadFile\"; filename=\"" +
				HttpUtility.UrlEncode(Path.GetFileName(filePathName)) + "\"\r\n" +
				"Content-Type: application/octet-stream\r\n\r\n");
			byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sb.ToString());
			byte[] fileBytes = File.ReadAllBytes(filePathName);
			byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
			webReq.ContentLength = postHeaderBytes.Length + fileBytes.Length + boundaryBytes.Length;
			Stream reqStream = webReq.GetRequestStream();
			reqStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
			reqStream.Write(fileBytes, 0, fileBytes.Length);
			reqStream.Write(boundaryBytes, 0, boundaryBytes.Length);
			WebResponse webResp = null;
			for (int errorCounter = 0; errorCounter <= Bot.retryTimes; errorCounter++) {
				try {
					webResp = (HttpWebResponse)webReq.GetResponse();
					break;
				}
				catch (WebException e) {
					string message = Bot.isRunningOnMono ? e.InnerException.Message : e.Message;
					if (Regex.IsMatch(message, ": \\(50[02349]\\) ")) {		// Remote problem
						if (errorCounter > Bot.retryTimes)
							throw;
						Console.Error.WriteLine(message + " " + Bot.Msg("Retrying in 60 seconds."));
						Thread.Sleep(60000);
					}
					else if (message.Contains("Section=ResponseStatusLine")) {	// Squid problem
						Bot.SwitchUnsafeHttpHeaderParsing(true);
						UploadImage(filePathName, description, license, copyStatus, source);
						return;
					}
					else
						throw;
				}
			}
			StreamReader strmReader = new StreamReader(webResp.GetResponseStream());
			string respStr = strmReader.ReadToEnd();
			strmReader.Close();
			webResp.Close();
			if (site.messages == null || !site.messages.Contains("uploadcorrupt"))
				site.GetMediaWikiMessagesEx(false);
			if (!respStr.Contains(HttpUtility.HtmlEncode(targetName)) ||
				respStr.Contains(HttpUtility.HtmlEncode(site.messages["uploadcorrupt"].text)))
					throw new WikiBotException(string.Format(
						Bot.Msg("Error occurred when uploading image \"{0}\"."), title));
			else {
				title = site.namespaces["6"] + ":" + targetName;
				text = description;
				Console.WriteLine(Bot.Msg("Image \"{0}\" uploaded successfully."), title);
			}
		}

		/// <summary>Uploads web image to wiki site.</summary>
		/// <param name="imageFileUrl">Full URL of image file on the web.</param>
		/// <param name="description">Image description.</param>
		/// <param name="license">Image license type. Used only in some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>
		/// <param name="copyStatus">Image copy status. Used only in some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>
		public void UploadImageFromWeb(string imageFileUrl, string description,
			string license, string copyStatus)
		{
			if (string.IsNullOrEmpty(imageFileUrl))
				throw new WikiBotException(Bot.Msg("No URL specified of image to upload."));
			Uri res = new Uri(imageFileUrl);
			Bot.InitWebClient();
			string imageFileName = imageFileUrl.Substring(imageFileUrl.LastIndexOf("/") + 1);
			try {
				Bot.wc.DownloadFile(res, "Cache" + Path.DirectorySeparatorChar + imageFileName);
			}
			catch (System.Net.WebException) {
				throw new WikiBotException(string.Format(
					Bot.Msg("Can't access image \"{0}\"."), imageFileUrl));
			}
			if (!File.Exists("Cache" + Path.DirectorySeparatorChar + imageFileName))
				throw new WikiBotException(string.Format(
					Bot.Msg("Error occurred when downloading image \"{0}\"."), imageFileUrl));
			UploadImage("Cache" + Path.DirectorySeparatorChar + imageFileName,
				description, license, copyStatus, imageFileUrl);
			File.Delete("Cache" + Path.DirectorySeparatorChar + imageFileName);
		}

		/// <summary>Downloads image, audio or video file, pointed by this page title,
		/// from the wiki site. Redirection is resolved automatically.</summary>
		/// <param name="filePathName">Path and name of local file to save image to.</param>
		public void DownloadImage(string filePathName)
		{
			string res = site.site + site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(title);
			string src = "";
			try {
				src = site.GetPageHTM(res);
			}
			catch (WebException e) {
				string message = Bot.isRunningOnMono ? e.InnerException.Message : e.Message;
				if (message.Contains(": (404) ")) {		// Not Found
					Console.Error.WriteLine(Bot.Msg("Page \"{0}\" doesn't exist."), title);
					text = "";
					return;
				}
				else
					throw;
			}
			Regex fileLinkRE1 = new Regex("<a href=\"([^\"]+?)\" class=\"internal\"");
			Regex fileLinkRE2 =
				new Regex("<div class=\"fullImageLink\" id=\"file\"><a href=\"([^\"]+?)\"");
			string fileLink = "";
			if (fileLinkRE1.IsMatch(src))
				fileLink = fileLinkRE1.Match(src).Groups[1].ToString();
			else if (fileLinkRE2.IsMatch(src))
				fileLink = fileLinkRE2.Match(src).Groups[1].ToString();
			else
				throw new WikiBotException(string.Format(
					Bot.Msg("Image \"{0}\" doesn't exist."), title));
			if (!fileLink.StartsWith("http"))
				fileLink = site.site + fileLink;
			Bot.InitWebClient();
			Console.WriteLine(Bot.Msg("Downloading image \"{0}\"..."), title);
			Bot.wc.DownloadFile(fileLink, filePathName);
			Console.WriteLine(Bot.Msg("Image \"{0}\" downloaded successfully."), title);
		}

		/// <summary>Saves page text to the specified file. If the target file already exists,
		/// it is overwritten.</summary>
		/// <param name="filePathName">Path and name of the file.</param>
		public void SaveToFile(string filePathName)
		{
			if (IsEmpty()) {
				Console.Error.WriteLine(Bot.Msg("Page \"{0}\" contains no text to save."), title);
				return;
			}
			File.WriteAllText(filePathName, text, Encoding.UTF8);
			Console.WriteLine(Bot.Msg("Text of \"{0}\" page successfully saved in \"{1}\" file."),
				title, filePathName);
		}

		/// <summary>Saves page text to the ".txt" file in current directory.
		/// Use Directory.SetCurrentDirectory function to change the current directory (but don't
		/// forget to change it back after saving file). The name of the file is constructed
		/// from the title of the article. Forbidden characters in filenames are replaced
		/// with their Unicode numeric codes (also known as numeric character references
		/// or NCRs).</summary>
		public void SaveToFile()
		{
			string fileTitle = title;
			//Path.GetInvalidFileNameChars();
			fileTitle = fileTitle.Replace("\"", "&#x22;");
			fileTitle = fileTitle.Replace("<", "&#x3c;");
			fileTitle = fileTitle.Replace(">", "&#x3e;");
			fileTitle = fileTitle.Replace("?", "&#x3f;");
			fileTitle = fileTitle.Replace(":", "&#x3a;");
			fileTitle = fileTitle.Replace("\\", "&#x5c;");
			fileTitle = fileTitle.Replace("/", "&#x2f;");
			fileTitle = fileTitle.Replace("*", "&#x2a;");
			fileTitle = fileTitle.Replace("|", "&#x7c;");
			SaveToFile(fileTitle + ".txt");
		}

		/// <summary>Returns true, if page.text field is empty. Don't forget to call
		/// page.Load() before using this function.</summary>
		/// <returns>Returns bool value.</returns>
		public bool IsEmpty()
		{
			return string.IsNullOrEmpty(text);
		}

		/// <summary>Returns true, if page.text field is not empty. Don't forget to call
		/// Load() or LoadEx() before using this function.</summary>
		/// <returns>Returns bool value.</returns>
		public bool Exists()
		{
			return (string.IsNullOrEmpty(text) == true) ? false : true;
		}

		/// <summary>Returns true, if page redirects to another page. Don't forget to load
		/// actual page contents from live wiki "Page.Load()" before using this function.</summary>
		/// <returns>Returns bool value.</returns>
		public bool IsRedirect()
		{
			if (!Exists())
				return false;
			return site.redirectRE.IsMatch(text);
		}

		/// <summary>Returns redirection target. Don't forget to load
		/// actual page contents from live wiki "Page.Load()" before using this function.</summary>
		/// <returns>Returns redirection target page title as string. Or empty string, if this
		/// Page object does not redirect anywhere.</returns>
		public string RedirectsTo()
		{
			if (IsRedirect())
				return site.redirectRE.Match(text).Groups[1].ToString().Trim();
			else
				return string.Empty;
		}

		/// <summary>If this page is a redirection, this function loads the title and text
		/// of redirected-to page into this Page object.</summary>
		public void ResolveRedirect()
		{
			if (IsRedirect()) {
				title = RedirectsTo();
				Load();
			}
		}

		/// <summary>Returns true, if this page is a disambiguation page. Don't forget to load
		/// actual page contents from live wiki  before using this function. Local redirect
		/// templates of Wikimedia sites are also recognized, but if this extended functionality
		/// is undesirable, then just set appropriate disambiguation template's title in
		/// "disambigStr" variable of Site object. Use "|" as a delimiter when enumerating
		/// several templates in "disambigStr" variable.</summary>
		/// <returns>Returns bool value.</returns>
		public bool IsDisambig()
		{
			if (string.IsNullOrEmpty(text))
				return false;
			if (!string.IsNullOrEmpty(site.disambigStr))
				return Regex.IsMatch(text, @"(?i)\{\{(" + site.disambigStr + ")}}");
			Console.WriteLine(Bot.Msg("Initializing disambiguation template tags..."));
			site.disambigStr = "disambiguation|disambig|dab";
			Uri res = new Uri("http://en.wikipedia.org/w/index.php?title=Template:Disambig/doc" +
				"&action=raw&ctype=text/plain&dontcountme=s");
			string buffer = text;
			text = Bot.GetWebResource(res, "");
			string[] iw = GetInterWikiLinks();
			foreach (string s in iw)
				if (s.StartsWith(site.language + ":")) {
					site.disambigStr += "|" + s.Substring(s.LastIndexOf(":") + 1,
						s.Length - s.LastIndexOf(":") - 1);
					break;
				}
			text = buffer;
			return Regex.IsMatch(text, @"(?i)\{\{(" + site.disambigStr + ")}}");
		}

		/// <summary>This internal function removes the namespace prefix from page title.</summary>
		public void RemoveNSPrefix()
		{
			title = site.RemoveNSPrefix(title, 0);
		}

		/// <summary>Function changes default English namespace prefixes to correct local prefixes
		/// (e.g. for German wiki-sites it changes "Category:..." to "Kategorie:...").</summary>
		public void CorrectNSPrefix()
		{
			title = site.CorrectNSPrefix(title);
		}

		/// <summary>Returns the array of strings, containing all wikilinks ([[...]])
		/// found in page text, excluding links in image descriptions, but including
		/// interwiki links, links to sister projects, categories, images, etc.</summary>
		/// <returns>Returns raw links in strings array.</returns>
		public string[] GetAllLinks()
		{
			MatchCollection matches = site.wikiLinkRE.Matches(text);
			string[] matchStrings = new string[matches.Count];
			for(int i = 0; i < matches.Count; i++)
				matchStrings[i] = matches[i].Groups[1].Value;
			return matchStrings;
		}

		/// <summary>Finds all internal wikilinks in page text, excluding interwiki
		/// links, links to sister projects, categories, embedded images and links in
		/// image descriptions.</summary>
		/// <returns>Returns the PageList object, where page titles are the wikilinks,
		/// found in text.</returns>
		public PageList GetLinks()
		{
			MatchCollection matches = site.wikiLinkRE.Matches(text);
			StringCollection exclLinks = new StringCollection();
			exclLinks.AddRange(GetInterWikiLinks());
			exclLinks.AddRange(GetSisterWikiLinks(true));
			StringCollection inclLinks = new StringCollection();
			string str;
			for(int i = 0; i < matches.Count; i++)
				if (exclLinks.Contains(matches[i].Groups[1].Value) == false &&
					exclLinks.Contains(matches[i].Groups[1].Value.TrimStart(':')) == false) {
					str = matches[i].Groups[1].Value;
					if (str.IndexOf("#") != -1)
						str = str.Substring(0, str.IndexOf("#"));
					inclLinks.Add(str); }
			PageList pl = new PageList(site, inclLinks);
			pl.RemoveNamespaces(new int[] {6,14});
			foreach (Page p in pl.pages)
				p.title = p.title.TrimStart(':');
			return pl;
		}

		/// <summary>Returns the array of strings, containing external links,
		/// found in page text.</summary>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetExternalLinks()
		{
			MatchCollection matches = site.webLinkRE.Matches(text);
			string[] matchStrings = new string[matches.Count];
			for(int i = 0; i < matches.Count; i++)
				matchStrings[i] = matches[i].Value;
			return matchStrings;
		}

		/// <summary>Returns the array of strings, containing interwiki links,
		/// found in page text. But no displayed links are returned,
		/// like [[:de:Stern]] - these are returned by GetSisterWikiLinks(true)
		/// function. Interwiki links are returned without square brackets.</summary>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetInterWikiLinks()
		{
			return GetInterWikiLinks(false);
		}

		/// <summary>Returns the array of strings, containing interwiki links,
		/// found in page text. But no displayed links are returned,
		/// like [[:de:Stern]] - these are returned by GetSisterWikiLinks(true)
		/// function.</summary>
		/// <param name="inSquareBrackets">Pass "true" to get interwiki links
		///in square brackets, for example "[[de:Stern]]", otherwise the result
		/// will be like "de:Stern", without brackets.</param>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetInterWikiLinks(bool inSquareBrackets)
		{
			if (string.IsNullOrEmpty(Site.WMLangsStr))
				site.GetWikimediaWikisList();
			MatchCollection matches = site.iwikiLinkRE.Matches(text);
			string[] matchStrings = new string[matches.Count];
			for(int i = 0; i < matches.Count; i++) {
				matchStrings[i] = matches[i].Groups[1].Value;
				if (inSquareBrackets)
					matchStrings[i] = "[[" + matchStrings[i] + "]]";
			}
			return matchStrings;
		}

		/// <summary>Adds interwiki links to the page. It doesn't remove or replace old
		/// interwiki links, this can be done by calling RemoveInterWikiLinks function
		/// or manually, if necessary.</summary>
		/// <param name="iwikiLinks">Interwiki links as an array of strings, with or
		/// without square brackets, for example: "de:Stern" or "[[de:Stern]]".</param>
		public void AddInterWikiLinks(string[] iwikiLinks)
		{
			if (iwikiLinks.Length == 0)
				throw new ArgumentNullException("iwikiLinks");
			List<string> iwList = new List<string>(iwikiLinks);
			AddInterWikiLinks(iwList);
		}

		/// <summary>Adds interwiki links to the page. It doesn't remove or replace old
		/// interwiki links, this can be done by calling RemoveInterWikiLinks function
		/// or manually, if necessary.</summary>
		/// <param name="iwikiLinks">Interwiki links as List of strings, with or
		/// without square brackets, for example: "de:Stern" or "[[de:Stern]]".</param>
		public void AddInterWikiLinks(List<string> iwikiLinks)
		{
			if (iwikiLinks.Count == 0)
				throw new ArgumentNullException("iwikiLinks");
			if (iwikiLinks.Count == 1 && iwikiLinks[0] == null)
				iwikiLinks.Clear();
			for(int i = 0; i < iwikiLinks.Count; i++)
				iwikiLinks[i] = iwikiLinks[i].Trim("[]\f\n\r\t\v ".ToCharArray());
			iwikiLinks.AddRange(GetInterWikiLinks());
			SortInterWikiLinks(ref iwikiLinks);
			RemoveInterWikiLinks();
			text += "\r\n";
			foreach(string str in iwikiLinks)
				text += "\r\n[[" + str + "]]";
		}

		/// <summary>Sorts interwiki links in page text according to site rules.
		/// Only rules for some Wikipedia projects are implemented so far.
		/// In other cases links are ordered alphabetically.</summary>
		public void SortInterWikiLinks()
		{
			AddInterWikiLinks(new string[] { null });
		}

		/// <summary>This internal function sorts interwiki links in page text according 
		/// to site rules. Only rules for some Wikipedia projects are implemented
		/// so far. In other cases links are ordered alphabetically.</summary>
		/// <param name="iwList">Interwiki links without square brackets in
		/// List object, either ordered or unordered.</param>
		public void SortInterWikiLinks(ref List<string> iwList)
		{
			string[] iwikiLinksOrder = null;
			if (iwList.Count < 2)
				return;
			switch(site.site) {
				case "http://en.wikipedia.org":
				case "http://simple.wikipedia.org":
				case "http://no.wikipedia.org": iwikiLinksOrder = Site.iwikiLinksOrder1; break;
				case "http://ms.wikipedia.org":
				case "http://et.wikipedia.org":
				case "http://fi.wikipedia.org":
				case "http://vi.wikipedia.org": iwikiLinksOrder = Site.iwikiLinksOrder2; break;
				default: iwList.Sort(); break;
			}
			if (iwikiLinksOrder == null)
				return;
			List<string> sortedIwikiList = new List<string>();
			string prefix;
			foreach (string iwikiLang in iwikiLinksOrder) {
				prefix = iwikiLang + ":";
				foreach (string iwikiLink in iwList)
					if (iwikiLink.StartsWith(prefix))
						sortedIwikiList.Add(iwikiLink);
			}
			foreach (string iwikiLink in iwList)
				if (!sortedIwikiList.Contains(iwikiLink))
					sortedIwikiList.Add(iwikiLink);
			iwList = sortedIwikiList;
		}

		/// <summary>Removes all interwiki links from text of page.</summary>
		public void RemoveInterWikiLinks()
		{
			if (string.IsNullOrEmpty(Site.WMLangsStr))
				site.GetWikimediaWikisList();
			text = site.iwikiLinkRE.Replace(text, "");
			text = text.TrimEnd("\r\n".ToCharArray());
		}

		/// <summary>Returns the array of strings, containing links to sister Wikimedia
		/// Foundation Projects, found in page text.</summary>
		/// <param name="includeDisplayedInterWikiLinks">Include displayed interwiki
		/// links like "[[:de:Stern]]".</param>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetSisterWikiLinks(bool includeDisplayedInterWikiLinks)
		{
			if (string.IsNullOrEmpty(Site.WMLangsStr))
				site.GetWikimediaWikisList();
			MatchCollection sisterMatches = site.sisterWikiLinkRE.Matches(text);
			MatchCollection iwikiMatches = site.iwikiDispLinkRE.Matches(text);
			int size = (includeDisplayedInterWikiLinks == true) ?
				sisterMatches.Count + iwikiMatches.Count : sisterMatches.Count;
			string[] matchStrings = new string[size];
			int i = 0;
			for(; i < sisterMatches.Count; i++)
				matchStrings[i] = sisterMatches[i].Groups[1].Value;
			if (includeDisplayedInterWikiLinks == true)
				for(int j = 0; j < iwikiMatches.Count; i++, j++)
					matchStrings[i] = iwikiMatches[j].Groups[1].Value;
			return matchStrings;
		}

		/// <summary>Function converts basic HTML markup in page text to wiki
		/// markup, except for tables markup, that is left unchanged. Use
		/// ConvertHtmlTablesToWikiTables function to convert HTML
		/// tables markup to wiki format.</summary>
		public void ConvertHtmlMarkupToWikiMarkup()
		{
			text = Regex.Replace(text, "(?is)n?<(h1)( [^/>]+?)?>(.+?)</\\1>n?", "\n= $3 =\n");
			text = Regex.Replace(text, "(?is)n?<(h2)( [^/>]+?)?>(.+?)</\\1>n?", "\n== $3 ==\n");
			text = Regex.Replace(text, "(?is)n?<(h3)( [^/>]+?)?>(.+?)</\\1>n?", "\n=== $3 ===\n");
			text = Regex.Replace(text, "(?is)n?<(h4)( [^/>]+?)?>(.+?)</\\1>n?", "\n==== $3 ====\n");
			text = Regex.Replace(text, "(?is)n?<(h5)( [^/>]+?)?>(.+?)</\\1>n?",
				"\n===== $3 =====\n");
			text = Regex.Replace(text, "(?is)n?<(h6)( [^/>]+?)?>(.+?)</\\1>n?",
				"\n====== $3 ======\n");
			text = Regex.Replace(text, "(?is)\n?\n?<p( [^/>]+?)?>(.+?)</p>", "\n\n$2");
			text = Regex.Replace(text, "(?is)<a href ?= ?[\"'](http:[^\"']+)[\"']>(.+?)</a>",
				"[$1 $2]");
			text = Regex.Replace(text, "(?i)</?(b|strong)>", "'''");
			text = Regex.Replace(text, "(?i)</?(i|em)>", "''");
			text = Regex.Replace(text, "(?i)\n?<hr ?/?>\n?", "\n----\n");
			text = Regex.Replace(text, "(?i)<(hr|br)( [^/>]+?)? ?/?>", "<$1$2 />");
		}

		/// <summary>Function converts HTML table markup in page text to wiki
		/// table markup.</summary>
		public void ConvertHtmlTablesToWikiTables()
		{
			if (!text.Contains("</table>"))
				return;
			text = Regex.Replace(text, ">\\s+<", "><");
			text = Regex.Replace(text, "<table( ?[^>]*)>", "\n{|$1\n");
			text = Regex.Replace(text, "</table>", "|}\n");
			text = Regex.Replace(text, "<caption( ?[^>]*)>", "|+$1 | ");
			text = Regex.Replace(text, "</caption>", "\n");
			text = Regex.Replace(text, "<tr( ?[^>]*)>", "|-$1\n");
			text = Regex.Replace(text, "</tr>", "\n");
			text = Regex.Replace(text, "<th([^>]*)>", "!$1 | ");
			text = Regex.Replace(text, "</th>", "\n");
			text = Regex.Replace(text, "<td([^>]*)>", "|$1 | ");
			text = Regex.Replace(text, "</td>", "\n");
			text = Regex.Replace(text, "\n(\\||\\|\\+|!) \\| ", "\n$1 ");
			text = text.Replace("\n\n|", "\n|");
		}

		/// <summary>Returns the array of strings, containing category names found in
		/// page text with namespace prefix, but without sorting keys. Use the result
		/// strings to call FillFromCategory(string) or FillFromCategoryTree(string)
		/// function. Categories, added by templates, are not returned. Use GetAllCategories
		/// function to get such categories too.</summary>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetCategories()
		{
			return GetCategories(true, false);
		}

		/// <summary>Returns the array of strings, containing category names found in
		/// page text. Categories, added by templates, are not returned. Use GetAllCategories
		/// function to get categories added by templates too.</summary>
		/// <param name="withNameSpacePrefix">If true, function returns strings with
		/// namespace prefix like "Category:Stars", not just "Stars".</param>
		/// <param name="withSortKey">If true, function returns strings with sort keys,
		/// if found. Like "Stars|D3" (in [[Category:Stars|D3]]), not just "Stars".</param>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetCategories(bool withNameSpacePrefix, bool withSortKey)
		{
			MatchCollection matches = site.wikiCategoryRE.Matches(
				Regex.Replace(text, "(?is)<nowiki>.+?</nowiki>", ""));
			string[] matchStrings = new string[matches.Count];
			for(int i = 0; i < matches.Count; i++) {
				matchStrings[i] = matches[i].Groups[4].Value;
				if (withSortKey == true)
					matchStrings[i] += matches[i].Groups[5].Value;
				if (withNameSpacePrefix == true)
					matchStrings[i] = site.namespaces["14"] + ":" + matchStrings[i];
			}
			return matchStrings;
		}

		/// <summary>Returns the array of strings, containing category names found in
		/// page text and added by page's templates. Categories are returned  with
		/// namespace prefix, but without sorting keys. Use the result strings
		/// to call FillFromCategory(string) or FillFromCategoryTree(string).</summary>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetAllCategories()
		{
			return GetAllCategories(true);
		}

		/// <summary>Returns the array of strings, containing category names found in
		/// page text and added by page's templates.</summary>
		/// <param name="withNameSpacePrefix">If true, function returns strings with
		/// namespace prefix like "Category:Stars", not just "Stars".</param>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetAllCategories(bool withNameSpacePrefix)
		{
			string uri;
			if (Bot.useBotQuery == true && site.botQuery == true && site.ver >= new Version(1,15))
				uri = site.site + site.indexPath +
					"api.php?action=query&prop=categories" +
					"&clprop=sortkey|hidden&cllimit=5000&format=xml&titles=" +
					HttpUtility.UrlEncode(title);
			else
				uri = site.site + site.indexPath + "index.php?title=" +
					HttpUtility.UrlEncode(title) + "&redirect=no";

			string xpathQuery;
			if (Bot.useBotQuery == true && site.botQuery == true && site.ver >= new Version(1,15))
				xpathQuery = "//categories/cl/@title";
			else if (site.ver >= new Version(1,13))
				xpathQuery = "//ns:div[ @id='mw-normal-catlinks' or @id='mw-hidden-catlinks' ]" +
					"/ns:span/ns:a";
			else
				xpathQuery = "//ns:div[ @id='catlinks' ]/ns:p/ns:span/ns:a";

			XPathNodeIterator iterator = site.GetXMLNodes(uri, null, xpathQuery);
			string[] matchStrings = new string[iterator.Count];
			iterator.MoveNext();
			for (int i = 0; i < iterator.Count; i++) {
				matchStrings[i] = (withNameSpacePrefix ? site.namespaces["14"] + ":" : "" ) + 
					site.RemoveNSPrefix(HttpUtility.HtmlDecode(iterator.Current.Value), 14);
					iterator.MoveNext();
			}

			return matchStrings;
		}

		/// <summary>Adds the page to the specified category by adding
		/// link to that category in page text. If the link to the specified category
		/// already exists, the function does nothing.</summary>
		/// <param name="categoryName">Category name, with or without prefix.
		/// Sort key can also be included after "|", like "Category:Stars|D3".</param>
		public void AddToCategory(string categoryName)
		{
			if (string.IsNullOrEmpty(categoryName))
				throw new ArgumentNullException("categoryName");
			categoryName = site.RemoveNSPrefix(categoryName, 14);
			string cleanCategoryName = !categoryName.Contains("|") ? categoryName.Trim()
				: categoryName.Substring(0, categoryName.IndexOf('|')).Trim();
			string[] categories = GetCategories(false, false);
			foreach (string category in categories)
				if (category == Bot.Capitalize(cleanCategoryName) ||
					category == Bot.Uncapitalize(cleanCategoryName))
						return;
			string[] iw = GetInterWikiLinks();
			RemoveInterWikiLinks();
			text += (categories.Length == 0 ? "\r\n" : "") +
				"\r\n[[" + site.namespaces["14"] + ":" + categoryName + "]]\r\n";
			if (iw.Length != 0)
				AddInterWikiLinks(iw);
			text = text.TrimEnd("\r\n".ToCharArray());
		}

		/// <summary>Removes the page from category by deleting link to that category in
		/// page text.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void RemoveFromCategory(string categoryName)
		{
			if (string.IsNullOrEmpty(categoryName))
				throw new ArgumentNullException("categoryName");
			categoryName = site.RemoveNSPrefix(categoryName, 14).Trim();
			categoryName = !categoryName.Contains("|") ? categoryName
				: categoryName.Substring(0, categoryName.IndexOf('|'));
			string[] categories = GetCategories(false, false);
			if (Array.IndexOf(categories, Bot.Capitalize(categoryName)) == -1 &&
				Array.IndexOf(categories, Bot.Uncapitalize(categoryName)) == -1)
					return;
			string regexCategoryName = Regex.Escape(categoryName);
			regexCategoryName = regexCategoryName.Replace("_", "\\ ").Replace("\\ ", "[_\\ ]");
			int firstCharIndex = (regexCategoryName[0] == '\\') ? 1 : 0;
			regexCategoryName = "[" + char.ToLower(regexCategoryName[firstCharIndex]) + 
				char.ToUpper(regexCategoryName[firstCharIndex]) + "]" +
				regexCategoryName.Substring(firstCharIndex + 1);
			text = Regex.Replace(text, @"\[\[((?i)" + site.namespaces["14"] + "|" +
				Site.wikiNSpaces["14"] + "):" + regexCategoryName + @"(\|.*?)?]]\r?\n?", "");
			text = text.TrimEnd("\r\n".ToCharArray());
		}

		/// <summary>Returns the array of strings, containing names of templates, used on page
		/// (applied to page). The "msgnw:" template modifier is not returned.
		/// Links to templates (like [[:Template:...]]) are not returned. Templates,
		/// mentioned inside <nowiki></nowiki> tags are also not returned. The "magic words"
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words) are recognized and
		/// not returned by this function as templates. When using this function on text of the
		/// template, parameters names and numbers (like {{{link}}} and {{{1}}}) are not returned
		/// by this function as templates too.</summary>
		/// <param name="withNameSpacePrefix">If true, function returns strings with
		/// namespace prefix like "Template:SomeTemplate", not just "SomeTemplate".</param>
		/// <returns>Returns the string[] array. Duplicates are possible.</returns>
		public string[] GetTemplates(bool withNameSpacePrefix)
		{
			string str = site.noWikiMarkupRE.Replace(text, "");
			if (GetNamespace() == 10)
				str = Regex.Replace(str, @"\{\{\{.*?}}}", "");
			MatchCollection matches = Regex.Matches(str, @"(?s)\{\{(.+?)(}}|\|)");
			string[] matchStrings = new string[matches.Count];
			string match = "";
			int j = 0;
			for(int i = 0; i < matches.Count; i++) {
				match = matches[i].Groups[1].Value;
				foreach (string mediaWikiVar in Site.mediaWikiVars)
					if (match.ToLower() == mediaWikiVar) {
						match = "";
						break;
					}
				if (string.IsNullOrEmpty(match))
					continue;
				foreach (string parserFunction in Site.parserFunctions)
					if (match.ToLower().StartsWith(parserFunction)) {
						match = "";
						break;
					}
				if (string.IsNullOrEmpty(match))
					continue;
				if (match.StartsWith("msgnw:") && match.Length > 6)
					match = match.Substring(6);
				match = site.RemoveNSPrefix(match, 10).Trim();
				if (withNameSpacePrefix)
					matchStrings[j++] = site.namespaces["10"] + ":" + match;
				else
					matchStrings[j++] = match;
			}
			Array.Resize(ref matchStrings, j);
			return matchStrings;
		}

		/// <summary>Returns the array of strings, containing templates, used on page
		/// (applied to page). Everything inside braces is returned  with all parameters
		/// untouched. Links to templates (like [[:Template:...]]) are not returned. Templates,
		/// mentioned inside <nowiki></nowiki> tags are also not returned. The "magic words"
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words) are recognized and
		/// not returned by this function as templates. When using this function on text of the
		/// template, parameters names and numbers (like {{{link}}} and {{{1}}}) are not returned
		/// by this function as templates too.</summary>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetTemplatesWithParams()
		{
			Dictionary<int, int> templPos = new Dictionary<int, int>();
			StringCollection templates = new StringCollection();
			int startPos, endPos, len = 0;
			string str = text;
			while ((startPos = str.LastIndexOf("{{")) != -1) {
				endPos = str.IndexOf("}}", startPos);
				len = (endPos != -1) ? endPos - startPos + 2 : 2;
				if (len != 2)
					templPos.Add(startPos, len);
				str = str.Remove(startPos, len);
				str = str.Insert(startPos, new String('_', len));
			}
			string[] templTitles = GetTemplates(false);
			Array.Reverse(templTitles);
			foreach (KeyValuePair<int, int> pos in templPos)
				templates.Add(text.Substring(pos.Key + 2, pos.Value - 4));
			for (int i = 0; i < templTitles.Length; i++)
				while (i < templates.Count &&
					!templates[i].StartsWith(templTitles[i]) &&
					!templates[i].StartsWith(site.namespaces["10"].ToString() + ":" +
						templTitles[i], true, site.langCulture) &&
					!templates[i].StartsWith(Site.wikiNSpaces["10"].ToString() + ":" +
						templTitles[i], true, site.langCulture) &&
					!templates[i].StartsWith("msgnw:" + templTitles[i]))
						templates.RemoveAt(i);
			string[] arr = new string[templates.Count];
			templates.CopyTo(arr, 0);
			Array.Reverse(arr);
			return arr;
		}

		/// <summary>Adds a specified template to the end of the page text
		/// (right before categories).</summary>
		/// <param name="templateText">Template text, like "{{template_name|...|...}}".</param>
		public void AddTemplate(string templateText)
		{
			if (string.IsNullOrEmpty(templateText))
				throw new ArgumentNullException("templateText");
			Regex templateInsertion = new Regex("([^}]\n|}})\n*\\[\\[((?i)" +
				Regex.Escape(site.namespaces["14"].ToString()) + "|" +
				Regex.Escape(Site.wikiNSpaces["14"].ToString()) + "):");
			if (templateInsertion.IsMatch(text))
				text = templateInsertion.Replace(text, "$1\n" + templateText + "\n\n[[" +
					site.namespaces["14"] + ":", 1);
			else {
				string[] iw = GetInterWikiLinks();
				RemoveInterWikiLinks();
				text += "\n\n" + templateText;
				if (iw.Length != 0)
					AddInterWikiLinks(iw);
				text = text.TrimEnd("\r\n".ToCharArray());
			}
		}

		/// <summary>Removes a specified template from page text.</summary>
		/// <param name="templateTitle">Title of template to remove.</param>
		public void RemoveTemplate(string templateTitle)
		{
			if (string.IsNullOrEmpty(templateTitle))
				throw new ArgumentNullException("templateTitle");
			templateTitle = Regex.Escape(templateTitle);
			templateTitle = "(" + Char.ToUpper(templateTitle[0]) + "|" +
				Char.ToLower(templateTitle[0]) + ")" +
				(templateTitle.Length > 1 ? templateTitle.Substring(1) : "");
			text = Regex.Replace(text, @"(?s)\{\{\s*" + templateTitle +
				@"(.*?)}}\r?\n?", "");
		}

		/// <summary>Returns the array of strings, containing names of files,
		/// embedded in page, including images in galleries (inside "gallery" tag).
		/// But no links to images and files, like [[:Image:...]] or [[:File:...]] or
		/// [[Media:...]].</summary>
		/// <param name="withNameSpacePrefix">If true, function returns strings with
		/// namespace prefix like "Image:Example.jpg" or "File:Example.jpg",
		/// not just "Example.jpg".</param>
		/// <returns>Returns the string[] array. The array can be empty (of size 0). Strings in
		/// array may recur, indicating that file was mentioned several times on the page.</returns>
		public string[] GetImages(bool withNameSpacePrefix)
		{
			return GetImagesEx(withNameSpacePrefix, false);
		}

		/// <summary>Returns the array of strings, containing names of files,
		/// mentioned on a page.</summary>
		/// <param name="withNameSpacePrefix">If true, function returns strings with
		/// namespace prefix like "Image:Example.jpg" or "File:Example.jpg",
		/// not just "Example.jpg".</param>
		/// <param name="includeFileLinks">If true, function also returns links to images,
		/// like [[:Image:...]] or [[:File:...]] or [[Media:...]]</param>
		/// <returns>Returns the string[] array. The array can be empty (of size 0).Strings in
		/// array may recur, indicating that file was mentioned several times on the page.</returns>
		public string[] GetImagesEx(bool withNameSpacePrefix, bool includeFileLinks)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException("text");
			string nsPrefixes = "File|Image|" + Regex.Escape(site.namespaces["6"].ToString());
			if (includeFileLinks) {
				nsPrefixes += "|" + Regex.Escape(site.namespaces["-2"].ToString()) + "|" +
					Regex.Escape(Site.wikiNSpaces["-2"].ToString());
			}
			MatchCollection matches;
			if (Regex.IsMatch(text, "(?is)<gallery>.*</gallery>"))
				matches = Regex.Matches(text, "(?i)" + (includeFileLinks ? "" : "(?<!:)") +
					"(" + nsPrefixes + ")(:)(.*?)(\\||\r|\n|]])");		// FIXME: inexact matches
			else
				matches = Regex.Matches(text, @"\[\[" + (includeFileLinks ? ":?" : "") +
					"(?i)((" + nsPrefixes + @"):(.+?))(\|(.+?))*?]]");
			string[] matchStrings = new string[matches.Count];
			for(int i = 0; i < matches.Count; i++) {
				if (withNameSpacePrefix == true)
					matchStrings[i] = site.namespaces["6"] + ":" + matches[i].Groups[3].Value;
				else
					matchStrings[i] = matches[i].Groups[3].Value;
			}
			return matchStrings;
		}

		/// <summary>Identifies the namespace of the page.</summary>
		/// <returns>Returns the integer key of the namespace.</returns>
		public int GetNamespace()
		{
			title = title.TrimStart(new char[] {':'});
			foreach (DictionaryEntry ns in site.namespaces) {
				if (title.StartsWith(ns.Value + ":", true, site.langCulture))
					return int.Parse(ns.Key.ToString());
			}
			foreach (DictionaryEntry ns in Site.wikiNSpaces) {
				if (title.StartsWith(ns.Value + ":", true, site.langCulture))
					return int.Parse(ns.Key.ToString());
			}
			return 0;
		}

		/// <summary>Sends page title to console.</summary>
		public void ShowTitle()
		{
			Console.Write("\n" + Bot.Msg("The title of this page is \"{0}\".") + "\n", title);
		}

		/// <summary>Sends page text to console.</summary>
		public void ShowText()
		{
			Console.Write("\n" + Bot.Msg("The text of \"{0}\" page:"), title);
			Console.Write("\n\n" + text + "\n\n");
		}

		/// <summary>Renames the page.</summary>
		/// <param name="newTitle">New title of that page.</param>
		/// <param name="reason">Reason for renaming.</param>
		public void RenameTo(string newTitle, string reason)
		{
			if (string.IsNullOrEmpty(newTitle))
				throw new ArgumentNullException("newTitle");
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to rename."));
			//Page mp = new Page(site, "Special:Movepage/" + HttpUtility.UrlEncode(title));
			Page mp = new Page(site, "Special:Movepage/" + title);
			mp.GetEditSessionData();
			if (string.IsNullOrEmpty(mp.editSessionToken))
				throw new WikiBotException(string.Format(
					Bot.Msg("Unable to rename page \"{0}\" to \"{1}\"."), title, newTitle));
			if (Bot.askConfirm) {
				Console.Write("\n\n" +
					Bot.Msg("The page \"{0}\" is going to be renamed to \"{1}\".\n"),
					title, newTitle);
				if(!Bot.UserConfirms())
					return;
			}
			string postData = string.Format("wpNewTitle={0}&wpOldTitle={1}&wpEditToken={2}" +
				"&wpReason={3}", HttpUtility.UrlEncode(newTitle), HttpUtility.UrlEncode(title),
				HttpUtility.UrlEncode(mp.editSessionToken), HttpUtility.UrlEncode(reason));
			string respStr = site.PostDataAndGetResultHTM(site.indexPath +
				"index.php?title=Special:Movepage&action=submit", postData);
			if (site.editSessionTokenRE2.IsMatch(respStr))
				throw new WikiBotException(string.Format(
					Bot.Msg("Failed to rename page \"{0}\" to \"{1}\"."), title, newTitle));
			Console.WriteLine(
				Bot.Msg("Page \"{0}\" was successfully renamed to \"{1}\"."), title, newTitle);
			title = newTitle;
		}

		/// <summary>Deletes the page. Sysop rights are needed to delete page.</summary>
		/// <param name="reason">Reason for deleting.</param>
		public void Delete(string reason)
		{
			if (string.IsNullOrEmpty(title))
				throw new WikiBotException(Bot.Msg("No title specified for page to delete."));
			string respStr1 = site.GetPageHTM(site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(title) + "&action=delete");
			editSessionToken = site.editSessionTokenRE1.Match(respStr1).Groups[1].ToString();
			if (string.IsNullOrEmpty(editSessionToken))
				editSessionToken = site.editSessionTokenRE2.Match(respStr1).Groups[1].ToString();
			if (string.IsNullOrEmpty(editSessionToken))
				throw new WikiBotException(
					string.Format(Bot.Msg("Unable to delete page \"{0}\"."), title));
			if (Bot.askConfirm) {
				Console.Write("\n\n" + Bot.Msg("The page \"{0}\" is going to be deleted.\n"), title);
				if(!Bot.UserConfirms())
					return;
			}
			string postData = string.Format("wpReason={0}&wpEditToken={1}",
				HttpUtility.UrlEncode(reason), HttpUtility.UrlEncode(editSessionToken));
			string respStr2 = site.PostDataAndGetResultHTM(site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(title) + "&action=delete", postData);
			if (site.editSessionTokenRE2.IsMatch(respStr2))
				throw new WikiBotException(
					string.Format(Bot.Msg("Failed to delete page \"{0}\"."), title));
			Console.WriteLine(Bot.Msg("Page \"{0}\" was successfully deleted."), title);
			title = "";
		}
	}

	/// <summary>Class defines a set of wiki pages (constructed inside as List object).</summary>
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class PageList
	{
		/// <summary>Internal generic List, that contains collection of pages.</summary>
		public List<Page> pages = new List<Page>();
		/// <summary>Site, on which the pages are located.</summary>
		public Site site;

		/// <summary>This constructor creates PageList object with specified Site object and fills
		/// it with Page objects with specified titles. When constructed, new Page objects
		/// in PageList don't contain text. Use Load() method to get text from live wiki,
		/// or use LoadEx() to get both text and metadata via XML export interface.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <param name="pageNames">Page titles as array of strings.</param>
		/// <returns>Returns the PageList object.</returns>
		public PageList(Site site, string[] pageNames)
		{
			this.site = site;
			foreach (string pageName in pageNames)
				pages.Add(new Page(site, pageName));
			CorrectNSPrefixes();
		}

		/// <summary>This constructor creates PageList object with specified Site object and fills
		/// it with Page objects with specified titles. When constructed, new Page objects
		/// in PageList don't contain text. Use Load() method to get text from live wiki,
		/// or use LoadEx() to get both text and metadata via XML export interface.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <param name="pageNames">Page titles as StringCollection object.</param>
		/// <returns>Returns the PageList object.</returns>
		public PageList(Site site, StringCollection pageNames)
		{
			this.site = site;
			foreach (string pageName in pageNames)
				pages.Add(new Page(site, pageName));
			CorrectNSPrefixes();
		}

		/// <summary>This constructor creates empty PageList object with specified
		/// Site object.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <returns>Returns the PageList object.</returns>
		public PageList(Site site)
		{
			this.site = site;
		}

		/// <summary>This constructor creates empty PageList object, Site object with default
		/// properties is created internally and logged in. Constructing new Site object
		/// is too slow, don't use this constructor needlessly.</summary>
		/// <returns>Returns the PageList object.</returns>
		public PageList()
		{
			site = new Site();
		}

		/// <summary>This index allows to call pageList[i] instead of pageList.pages[i].</summary>
		/// <param name="index">Zero-based index.</param>
		/// <returns>Returns the Page object.</returns>
		public Page this[int index]
		{
			get { return pages[index]; }
			set { pages[index] = value; }
		}

		/// <summary>This function allows to access individual pages in this PageList.
		/// But it's better to use simple pageList[i] index, when it is possible.</summary>
		/// <param name="index">Zero-based index.</param>
		/// <returns>Returns the Page object.</returns>
		public Page GetPageAtIndex(int index)
		{
			return pages[index];
		}

		/// <summary>This function allows to set individual pages in this PageList.
		/// But it's better to use simple pageList[i] index, when it is possible.</summary>
		/// <param name="page">Page object to set in this PageList.</param>
		/// <param name="index">Zero-based index.</param>
		/// <returns>Returns the Page object.</returns>
		public void SetPageAtIndex(Page page, int index)
		{
			pages[index] = page;
		}

		/// <summary>This index allows to call pageList["title"]. Don't forget to use correct
		/// local namespace prefixes. Call CorrectNSPrefixes function to correct namespace
		/// prefixes in a whole PageList at once.</summary>
		/// <param name="index">Title of page to get.</param>
		/// <returns>Returns the Page object, or null if there is no page with the specified
		/// title in this PageList.</returns>
		public Page this[string index]
		{
			get {
				foreach (Page p in pages)
					if (p.title == index)
						return p;
				return null;
			}
			set {
				for (int i=0; i < pages.Count; i++)
					if (pages[i].title == index)
						pages[i] = value;
			}
		}

		/// <summary>This standard internal function allows to directly use PageList objects
		/// in "foreach" statements.</summary>
		/// <returns>Returns IEnumerator object.</returns>
		public IEnumerator GetEnumerator()
		{
			return pages.GetEnumerator();
		}

		/// <summary>This function adds specified page to the end of this PageList.</summary>
		/// <param name="page">Page object to add.</param>
		public void Add(Page page)
		{
			pages.Add(page);
		}

		/// <summary>Inserts an element into this PageList at the specified index.</summary>
		/// <param name="page">Page object to insert.</param>
		/// <param name="index">Zero-based index.</param>
		public void Insert(Page page, int index)
		{
			pages.Insert(index, page);
		}

		/// <summary>This function returns true, if in this PageList there exists a page with
		/// the same title, as a page specified as a parameter.</summary>
		/// <param name="page">.</param>
		/// <returns>Returns bool value.</returns>
		public bool Contains(Page page)
		{
			page.CorrectNSPrefix();
			CorrectNSPrefixes();
			foreach (Page p in pages)
				if (p.title == page.title)
					return true;
			return false;
		}

		/// <summary>This function returns true, if a page with specified title exists
		/// in this PageList.</summary>
		/// <param name="title">Title of page to check.</param>
		/// <returns>Returns bool value.</returns>
		public bool Contains(string title)
		{
			Page page = new Page(site, title);
			page.CorrectNSPrefix();
			CorrectNSPrefixes();
			foreach (Page p in pages)
				if (p.title == page.title)
					return true;
			return false;
		}

		/// <summary>This function returns the number of pages in PageList.</summary>
		/// <returns>Number of pages as positive integer value.</returns>
		public int Count()
		{
			return pages.Count;
		}

		/// <summary>Removes page at specified index from PageList.</summary>
		/// <param name="index">Zero-based index.</param>
		public void RemoveAt(int index)
		{
			pages.RemoveAt(index);
		}

		/// <summary>Removes a page with specified title from this PageList.</summary>
		/// <param name="title">Title of page to remove.</param>
		public void Remove(string title)
		{
			for(int i = 0; i < Count(); i++)
				if (pages[i].title == title)
					pages.RemoveAt(i);
		}

		/// <summary>Gets page titles for this PageList from "Special:Allpages" MediaWiki page.
		/// That means a list of site pages in alphabetical order.</summary>
		/// <param name="firstPageTitle">Title of page to start enumerating from. The title
		/// must have no namespace prefix (like "Talk:"), just the page title itself. Or you can
		/// specify just a letter or two instead of full real title. Pass the empty string or null
		/// to start from the very beginning.</param>
		/// <param name="neededNSpace">Integer, presenting the key of namespace to get pages
		/// from. Only one key of one namespace can be specified (zero for default).</param>
		/// <param name="acceptRedirects">Set this to "false" to exclude redirects.</param>
		/// <param name="quantity">Maximum allowed quantity of pages in this PageList.</param>
		public void FillFromAllPages(string firstPageTitle, int neededNSpace, bool acceptRedirects,
			int quantity)
		{
			if (quantity <= 0)
				throw new ArgumentOutOfRangeException("quantity",
					Bot.Msg("Quantity must be positive."));
			if (Bot.useBotQuery == true && site.botQuery == true) {
				FillFromCustomBotQueryList("allpages", "apnamespace=" + neededNSpace +
				(acceptRedirects ? "" : "&apfilterredir=nonredirects") +
				(string.IsNullOrEmpty(firstPageTitle) ? "" : "&apfrom=" +
				HttpUtility.UrlEncode(firstPageTitle)), quantity);
				return;
			}
			Console.WriteLine(
				Bot.Msg("Getting {0} page titles from \"Special:Allpages\" MediaWiki page..."),
				quantity);
			int count = pages.Count;
			quantity += pages.Count;
			Regex linkToPageRE;
			if (acceptRedirects)
				linkToPageRE = new Regex("<td[^>]*>(?:<div class=\"allpagesredirect\">)?" +
					"<a href=\"[^\"]*?\" title=\"([^\"]*?)\">");
			else
				linkToPageRE = new Regex("<td[^>]*><a href=\"[^\"]*?\" title=\"([^\"]*?)\">");
			MatchCollection matches;
			do {
				string res = site.site + site.indexPath +
					"index.php?title=Special:Allpages&from=" +
					HttpUtility.UrlEncode(
						string.IsNullOrEmpty(firstPageTitle) ? "!" : firstPageTitle) +
					"&namespace=" + neededNSpace.ToString();
				matches = linkToPageRE.Matches(site.GetPageHTM(res));
				if (matches.Count < 2)
					break;
				for (int i = 1; i < matches.Count; i++)
					pages.Add(new Page(site, HttpUtility.HtmlDecode(matches[i].Groups[1].Value)));
				firstPageTitle = site.RemoveNSPrefix(pages[pages.Count - 1].title, neededNSpace) +
					"!";
			}
			while(pages.Count < quantity);
			if (pages.Count > quantity)
				pages.RemoveRange(quantity, pages.Count - quantity);
			Console.WriteLine(Bot.Msg("PageList filled with {0} page titles from " +
				"\"Special:Allpages\" MediaWiki page."), (pages.Count - count).ToString());
		}

		/// <summary>Gets page titles for this PageList from specified special page,
		/// e.g. "Deadendpages". The function does not filter namespaces. And the function
		/// does not clear the existing PageList, so new titles will be added.</summary>
		/// <param name="pageTitle">Title of special page, e.g. "Deadendpages".</param>
		/// <param name="quantity">Maximum number of page titles to get. Usually
		/// MediaWiki provides not more than 1000 titles.</param>
		public void FillFromCustomSpecialPage(string pageTitle, int quantity)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			if (quantity <= 0)
				throw new ArgumentOutOfRangeException("quantity",
					Bot.Msg("Quantity must be positive."));
			Console.WriteLine(Bot.Msg("Getting {0} page titles from \"Special:{1}\" page..."),
				quantity, pageTitle);
			string res = site.site + site.indexPath + "index.php?title=Special:" +
				HttpUtility.UrlEncode(pageTitle) + "&limit=" + quantity.ToString();
			string src = site.GetPageHTM(res);
			MatchCollection matches;
			if (pageTitle == "Unusedimages" || pageTitle == "Uncategorizedimages" ||
				pageTitle == "UnusedFiles" || pageTitle == "UncategorizedFiles")
					matches = site.linkToPageRE3.Matches(src);
			else
				matches = site.linkToPageRE2.Matches(src);
			if (matches.Count == 0)
				throw new WikiBotException(string.Format(
					Bot.Msg("Page \"Special:{0}\" does not contain page titles."), pageTitle));
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(Bot.Msg("PageList filled with {0} page titles from " +
				"\"Special:{1}\" page."), matches.Count, pageTitle);
		}

		/// <summary>Gets page titles for this PageList from specified special page,
		/// e.g. "Deadendpages". The function does not filter namespaces. And the function
		/// does not clear the existing PageList, so new titles will be added.
		/// The function uses XML (XHTML) parsing instead of regular expressions matching.
		/// This function is slower, than FillFromCustomSpecialPage.</summary>
		/// <param name="pageTitle">Title of special page, e.g. "Deadendpages".</param>
		/// <param name="quantity">Maximum number of page titles to get. Usually
		/// MediaWiki provides not more than 1000 titles.</param>
		public void FillFromCustomSpecialPageEx(string pageTitle, int quantity)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			if (quantity <= 0)
				throw new ArgumentOutOfRangeException("quantity",
					Bot.Msg("Quantity must be positive."));
			Console.WriteLine(Bot.Msg("Getting {0} page titles from \"Special:{1}\" page..."),
				quantity, pageTitle);
			string res = site.site + site.indexPath + "index.php?title=Special:" +
				HttpUtility.UrlEncode(pageTitle) + "&limit=" + quantity.ToString();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(site.GetPageHTM(res));
			XmlNodeList nl = doc.DocumentElement.SelectNodes("//ns:ol/ns:li/ns:a[@title != '']",
				site.xmlNS);
			if (nl.Count == 0)
				throw new WikiBotException(string.Format(
					Bot.Msg("Nothing was found on \"Special:{0}\" page."), pageTitle));
			foreach (XmlNode node in nl)
				pages.Add(
					new Page(site, HttpUtility.HtmlDecode(node.Attributes["title"].InnerXml)));
			Console.WriteLine(Bot.Msg("PageList filled with {0} page titles from " +
				"\"Special:{1}\" page."), nl.Count, pageTitle);
		}

		/// <summary>Gets page titles for this PageList from specified MediaWiki events log.
		/// The function does not filter namespaces. And the function does not clear the
		/// existing PageList, so new titles will be added.</summary>
		/// <param name="logType">Type of log, it could be: "block" for blocked users log;
		/// "protect" for protected pages log; "rights" for users rights log; "delete" for
		/// deleted pages log; "upload" for uploaded files log; "move" for renamed pages log;
		/// "import" for transwiki import log; "renameuser" for renamed accounts log;
		/// "newusers" for new users log; "makebot" for bot status assignment log.</param>
		/// <param name="userName">Select log entries only for specified account. Pass empty
		/// string, if that restriction is not needed.</param>
		/// <param name="pageTitle">Select log entries only for specified page. Pass empty
		/// string, if that restriction is not needed.</param>
		/// <param name="quantity">Maximum number of page titles to get.</param>
		public void FillFromCustomLog(string logType, string userName, string pageTitle,
			int quantity)
		{
			if (string.IsNullOrEmpty(logType))
				throw new ArgumentNullException("logType");
			if (quantity <= 0)
				throw new ArgumentOutOfRangeException("quantity",
					Bot.Msg("Quantity must be positive."));
			Console.WriteLine(Bot.Msg("Getting {0} page titles from \"{1}\" log..."),
				quantity.ToString(), logType);
			string res = site.site + site.indexPath + "index.php?title=Special:Log&type=" +
				 logType + "&user=" + HttpUtility.UrlEncode(userName) + "&page=" +
				 HttpUtility.UrlEncode(pageTitle) + "&limit=" + quantity.ToString();
			string src = site.GetPageHTM(res);
			MatchCollection matches = site.linkToPageRE2.Matches(src);
			if (matches.Count == 0)
				throw new WikiBotException(
					string.Format(Bot.Msg("Log \"{0}\" does not contain page titles."), logType));
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(Bot.Msg("PageList filled with {0} page titles from \"{1}\" log."),
				matches.Count, logType);
		}

		/// <summary>Gets page titles for this PageList from specified list, produced by
		/// bot query interface ("api.php" MediaWiki extension). The function
		/// does not clear the existing PageList, so new titles will be added.</summary>
		/// <param name="listType">Title of list, the following values are supported: 
		/// "allpages", "alllinks", "allusers", "backlinks", "categorymembers",
		/// "embeddedin", "imageusage", "logevents", "recentchanges", 
		/// "usercontribs", "watchlist", "exturlusage". Detailed documentation
		/// can be found at "http://en.wikipedia.org/w/api.php".</param>
		/// <param name="queryParams">Additional query parameters, specific to the
		/// required list, e.g. "cmtitle=Category:Physical%20sciences&amp;cmnamespace=0|2".
		/// Parameter values must be URL-encoded with HttpUtility.UrlEncode function
		/// before calling this function.</param>
		/// <param name="quantity">Maximum number of page titles to get.</param>
		/// <example><code>pageList.FillFromCustomBotQueryList("categorymembers",
		/// 	"cmcategory=Physical%20sciences&amp;cmnamespace=0|14",
		/// 	int.MaxValue);</code></example>
		public void FillFromCustomBotQueryList(string listType, string queryParams, int quantity)
		{
			if (!site.botQuery)
				throw new WikiBotException(
					Bot.Msg("The \"api.php\" MediaWiki extension is not available."));
			if (string.IsNullOrEmpty(listType))
				throw new ArgumentNullException("listType");
			if (!Site.botQueryLists.Contains(listType))
				throw new WikiBotException(
					string.Format(Bot.Msg("The list \"{0}\" is not supported."), listType));
			if (quantity <= 0)
				throw new ArgumentOutOfRangeException("quantity",
					Bot.Msg("Quantity must be positive."));
			string prefix = Site.botQueryLists[listType].ToString();
			string continueAttrTag = prefix + "continue";
			string attrTag = (listType != "allusers") ? "title" : "name";
			string queryUri = site.indexPath + "api.php?action=query&list=" + listType +
				"&format=xml&" + prefix + "limit=" +
				((quantity > 500) ? "500" : quantity.ToString());
			string src = "", next = "", queryFullUri = "";
			int count = pages.Count;
			if (quantity != int.MaxValue)
				quantity += pages.Count;
			do {
				queryFullUri = queryUri;
				if (next != "")
					queryFullUri += "&" + prefix + "continue=" + HttpUtility.UrlEncode(next);
				src = site.PostDataAndGetResultHTM(queryFullUri, queryParams);
				using (XmlTextReader reader = new XmlTextReader(new StringReader(src))) {
					next = "";
					while (reader.Read()) {
						if (reader.IsEmptyElement && reader[attrTag] != null)
							pages.Add(new Page(site, HttpUtility.HtmlDecode(reader[attrTag])));
						if (reader.IsEmptyElement && reader[continueAttrTag] != null)
							next = reader[continueAttrTag];
					}
				}
			}
			while (next != "" && pages.Count < quantity);
			if (pages.Count > quantity)
				pages.RemoveRange(quantity, pages.Count - quantity);
			if (!string.IsNullOrEmpty(Environment.StackTrace) &&
				!Environment.StackTrace.Contains("FillAllFromCategoryEx"))
					Console.WriteLine(Bot.Msg("PageList filled with {0} page titles " +
						"from \"{1}\" bot interface list."),
						(pages.Count - count).ToString(), listType);
		}

		/// <summary>Gets page titles for this PageList from recent changes page,
		/// "Special:Recentchanges". File uploads, page deletions and page renamings are
		/// not included, use FillFromCustomLog function instead to fill from respective logs.
		/// The function does not clear the existing PageList, so new titles will be added.
		/// Use FilterNamespaces() or RemoveNamespaces() functions to remove
		/// pages from unwanted namespaces.</summary>
		/// <param name="hideMinor">Ignore minor edits.</param>
		/// <param name="hideBots">Ignore bot edits.</param>
		/// <param name="hideAnons">Ignore anonymous users edits.</param>
		/// <param name="hideLogged">Ignore logged-in users edits.</param>
		/// <param name="hideSelf">Ignore edits of this bot account.</param>
		/// <param name="limit">Maximum number of changes to get.</param>
		/// <param name="days">Get changes for this number of recent days.</param>
		public void FillFromRecentChanges(bool hideMinor, bool hideBots, bool hideAnons,
			bool hideLogged, bool hideSelf, int limit, int days)
		{
			if (limit <= 0)
				throw new ArgumentOutOfRangeException("limit", Bot.Msg("Limit must be positive."));
			if (days <= 0)
				throw new ArgumentOutOfRangeException("days",
					Bot.Msg("Number of days must be positive."));
			Console.WriteLine(Bot.Msg("Getting {0} page titles from " +
				"\"Special:Recentchanges\" page..."), limit);
			string uri = string.Format("{0}{1}index.php?title=Special:Recentchanges&" +
				"hideminor={2}&hideBots={3}&hideAnons={4}&hideliu={5}&hidemyself={6}&" +
				"limit={7}&days={8}", site.site, site.indexPath,
				hideMinor ? "1" : "0", hideBots ? "1" : "0", hideAnons ? "1" : "0",
				hideLogged ? "1" : "0", hideSelf ? "1" : "0",
				limit.ToString(), days.ToString());
			string respStr = site.GetPageHTM(uri);
			MatchCollection matches = site.linkToPageRE2.Matches(respStr);
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(Bot.Msg("PageList filled with {0} page titles from " +
				"\"Special:Recentchanges\" page."), matches.Count);
		}

		/// <summary>Gets page titles for this PageList from specified wiki category page, excluding
		/// subcategories. Use FillSubsFromCategory function to get subcategories.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillFromCategory(string categoryName)
		{
			int count = pages.Count;
			PageList pl = new PageList(site);
			pl.FillAllFromCategory(categoryName);
			pl.RemoveNamespaces(new int[] {14});
			pages.AddRange(pl.pages);
			if (pages.Count != count)
				Console.WriteLine(
					Bot.Msg("PageList filled with {0} page titles, found in \"{1}\" category."),
					(pages.Count - count).ToString(), categoryName);
			else
				Console.Error.WriteLine(
					Bot.Msg("Nothing was found in \"{0}\" category."), categoryName);
		}

		/// <summary>Gets subcategories titles for this PageList from specified wiki category page,
		/// excluding other pages. Use FillFromCategory function to get other pages.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillSubsFromCategory(string categoryName)
		{
			int count = pages.Count;
			PageList pl = new PageList(site);
			pl.FillAllFromCategory(categoryName);
			pl.FilterNamespaces(new int[] {14});
			pages.AddRange(pl.pages);
			if (pages.Count != count)
				Console.WriteLine(Bot.Msg("PageList filled with {0} subcategory page titles, " +
					"found in \"{1}\" category."), (pages.Count - count).ToString(), categoryName);
			else
				Console.Error.WriteLine(
					Bot.Msg("Nothing was found in \"{0}\" category."), categoryName);
		}

		/// <summary>This internal function gets all page titles for this PageList from specified
		/// category page, including subcategories.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillAllFromCategory(string categoryName)
		{
			if (string.IsNullOrEmpty(categoryName))
				throw new ArgumentNullException("categoryName");
			categoryName = categoryName.Trim("[]\f\n\r\t\v ".ToCharArray());
			categoryName = site.RemoveNSPrefix(categoryName, 14);
			categoryName = site.namespaces["14"] + ":" + categoryName;
			Console.WriteLine(Bot.Msg("Getting category \"{0}\" contents..."), categoryName);
			//RemoveAll();
			if (Bot.useBotQuery == true && site.botQuery == true) {
				FillAllFromCategoryEx(categoryName);
				return;
			}
			string src = "";
			MatchCollection matches;
			Regex nextPortionRE = new Regex("&(?:amp;)?from=([^\"=]+)\" title=\"");
			do {
				string res = site.site + site.indexPath + "index.php?title=" +
					HttpUtility.UrlEncode(categoryName) +
					"&from=" + nextPortionRE.Match(src).Groups[1].Value;
				src = site.GetPageHTM(res);
				src = HttpUtility.HtmlDecode(src);
				matches = site.linkToPageRE1.Matches(src);
				foreach (Match match in matches)
					pages.Add(new Page(site, match.Groups[1].Value));
				if (src.Contains("<div class=\"gallerytext\">\n")) {
					matches = site.linkToImageRE.Matches(src);
					foreach (Match match in matches)
						pages.Add(new Page(site, match.Groups[1].Value));
				}
				if (src.Contains("<div class=\"CategoryTreeChildren\"")) {
					matches = site.linkToSubCategoryRE.Matches(src);
					foreach (Match match in matches)
						pages.Add(new Page(site, site.namespaces["14"] + ":" +
							match.Groups[1].Value));
				}
			}
			while(nextPortionRE.IsMatch(src));
		}

		/// <summary>This internal function gets all page titles for this PageList from specified
		/// category using "api.php" MediaWiki extension (bot interface), if it is available.
		/// It gets subcategories too.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillAllFromCategoryEx(string categoryName)
		{
			if (string.IsNullOrEmpty(categoryName))
				throw new ArgumentNullException("categoryName");
			categoryName = categoryName.Trim("[]\f\n\r\t\v ".ToCharArray());
			categoryName = site.RemoveNSPrefix(categoryName, 14);
			if (site.botQueryVersions.ContainsKey("ApiQueryCategoryMembers.php")) {
				if (int.Parse(
					site.botQueryVersions["ApiQueryCategoryMembers.php"].ToString()) >= 30533)
						FillFromCustomBotQueryList("categorymembers", "cmtitle=" + 
							HttpUtility.UrlEncode(site.namespaces["14"].ToString() + ":" +
							categoryName), int.MaxValue);
				else
					FillFromCustomBotQueryList("categorymembers", "cmcategory=" +  
						HttpUtility.UrlEncode(categoryName), int.MaxValue);
			}
			else if (site.botQueryVersions.ContainsKey("query.php"))
				FillAllFromCategoryExOld(categoryName);
			else {
				Console.WriteLine(Bot.Msg("Can't get category members using bot interface.\n" +
					"Switching to common user interface (\"site.botQuery\" is set to \"false\")."));
				site.botQuery = false;
				FillAllFromCategory(categoryName);
			}
		}

		/// <summary>This internal function is kept for backwards compatibility only.
		/// It gets all pages and subcategories in specified category using old obsolete 
		/// "query.php" bot interface and adds all found pages and subcategories to PageList object.
		/// It gets titles portion by portion. The "query.php" interface was superseded by
		/// "api.php" in MediaWiki 1.8.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillAllFromCategoryExOld(string categoryName)
		{
			if (string.IsNullOrEmpty(categoryName))
				throw new ArgumentNullException("categoryName");
			string src = "";
			MatchCollection matches;
			Regex nextPortionRE = new Regex("<category next=\"(.+?)\" />");
			do {
				string res = site.site + site.indexPath + "query.php?what=category&cptitle=" +
					HttpUtility.UrlEncode(categoryName) + "&cpfrom=" +
					nextPortionRE.Match(src).Groups[1].Value + "&cplimit=500&format=xml";
				src = site.GetPageHTM(res);
				matches = site.pageTitleTagRE.Matches(src);
				foreach (Match match in matches)
					pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			}
			while(nextPortionRE.IsMatch(src));
		}

		/// <summary>Gets all levels of subcategories of some wiki category (that means
		/// subcategories, sub-subcategories, and so on) and fills this PageList with titles
		/// of all pages, found in all levels of subcategories. The multiplicates of recurring pages
		/// are removed. Use FillSubsFromCategoryTree function instead to get titles
		/// of subcategories. This operation may be very time-consuming and traffic-consuming.
		/// The function clears the PageList before filling.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillFromCategoryTree(string categoryName)
		{
			FillAllFromCategoryTree(categoryName);
			RemoveNamespaces(new int[] {14});
			if (pages.Count != 0)
				Console.WriteLine(
					Bot.Msg("PageList filled with {0} page titles, found in \"{1}\" category."),
					Count().ToString(), categoryName);
			else
				Console.Error.WriteLine(
					Bot.Msg("Nothing was found in \"{0}\" category."), categoryName);
		}

		/// <summary>Gets all levels of subcategories of some wiki category (that means
		/// subcategories, sub-subcategories, and so on) and fills this PageList with found
		/// subcategory titles. Use FillFromCategoryTree function instead to get pages of other
		/// namespaces. The multiplicates of recurring categories are removed. The operation may
		/// be very time-consuming and traffic-consuming. The function clears the PageList
		/// before filling.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillSubsFromCategoryTree(string categoryName)
		{
			FillAllFromCategoryTree(categoryName);
			FilterNamespaces(new int[] {14});
			if (pages.Count != 0)
				Console.WriteLine(Bot.Msg("PageList filled with {0} subcategory page titles, " +
					"found in \"{1}\" category."), Count().ToString(), categoryName);
			else
				Console.Error.WriteLine(
					Bot.Msg("Nothing was found in \"{0}\" category."), categoryName);
		}

		/// <summary>Gets all levels of subcategories of some wiki category (that means
		/// subcategories, sub-subcategories, and so on) and fills this PageList with titles
		/// of all pages, found in all levels of subcategories, including the titles of
		/// subcategories. The multiplicates of recurring pages and subcategories are removed.
		/// The operation may be very time-consuming and traffic-consuming. The function clears
		/// the PageList before filling.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillAllFromCategoryTree(string categoryName)
		{
			Clear();
			categoryName = site.CorrectNSPrefix(categoryName);
			StringCollection doneCats = new StringCollection();
			FillAllFromCategory(categoryName);
			doneCats.Add(categoryName);
			for (int i = 0; i < Count(); i++)
				if (pages[i].GetNamespace() == 14 && !doneCats.Contains(pages[i].title)) {
					FillAllFromCategory(pages[i].title);
					doneCats.Add(pages[i].title);
				}
			RemoveRecurring();
		}

		/// <summary>Gets page history and fills this PageList with specified number of last page
		/// revisions. But only revision identifiers, user names, timestamps and comments are
		/// loaded, not the texts. Call Load() (but not LoadEx) to load the texts of page revisions.
		/// The function combines XML (XHTML) parsing and regular expressions matching.</summary>
		/// <param name="pageTitle">Page to get history of.</param>
		/// <param name="lastRevisions">Number of last page revisions to get.</param>
		public void FillFromPageHistory(string pageTitle, int lastRevisions)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			if (lastRevisions <= 0)
				throw new ArgumentOutOfRangeException("quantity",
					Bot.Msg("Quantity must be positive."));
			Console.WriteLine(
				Bot.Msg("Getting {0} last revisons of \"{1}\" page..."), lastRevisions, pageTitle);
			string res = site.site + site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(pageTitle) + "&limit=" + lastRevisions.ToString() + 
					"&action=history";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(site.GetPageHTM(res));
			XmlNodeList nl = doc.DocumentElement.SelectNodes("//ns:ul[@id='pagehistory']/ns:li",
				site.xmlNS);
			Regex revisionLinkRE = new Regex(@"(?<!diff=\d+&amp;)oldid=(\d+).+?>(.+?)<");
			XmlNode subn;
			foreach (XmlNode n in nl) {
				Page p = new Page(site, pageTitle);
				p.lastRevisionID = revisionLinkRE.Match(n.InnerXml).Groups[1].Value;
				DateTime.TryParse(revisionLinkRE.Match(n.InnerXml).Groups[2].Value,
					site.regCulture, DateTimeStyles.AssumeLocal, out p.timestamp);
				p.lastUser =
					n.SelectSingleNode("ns:span[@class='history-user']/ns:a", site.xmlNS).InnerText;
				if ((subn =
					n.SelectSingleNode("ns:span[@class='history-size']", site.xmlNS)) != null)
						int.TryParse(Regex.Replace(subn.InnerText, @"[^-+\d]", ""),
							out p.lastBytesModified);
				p.lastMinorEdit =
					(n.SelectSingleNode("ns:span[@class='minor']", site.xmlNS) != null) ?
					true : false;
				p.comment = (n.SelectSingleNode("ns:span[@class='comment']", site.xmlNS) != null) ?
					n.SelectSingleNode("ns:span[@class='comment']", site.xmlNS).InnerText : "";
				pages.Add(p);
			}
			Console.WriteLine(Bot.Msg("PageList filled with {0} last revisons of \"{1}\" page..."),
				nl.Count, pageTitle);
		}

		/// <summary>Gets page history using  bot query interface ("api.php" MediaWiki extension)
		/// and fills this PageList with specified number of last page revisions, optionally loading
		/// revision texts as well. On most sites not more than 50 last revisions can be obtained.
		/// Thanks to Jutiphan Mongkolsuthree for idea and outline of this function.</summary>
		/// <param name="pageTitle">Page to get history of.</param>
		/// <param name="lastRevisions">Number of last page revisions to obtain.</param>
		/// <param name="loadTexts">Load revision texts right away.</param>
		public void FillFromPageHistoryEx(string pageTitle, int lastRevisions, bool loadTexts)
		{
			if (!site.botQuery)
				throw new WikiBotException(
					Bot.Msg("The \"api.php\" MediaWiki extension is not available."));
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			if (lastRevisions <= 0)
				throw new ArgumentOutOfRangeException("lastRevisions",
					Bot.Msg("Quantity must be positive."));
			Console.WriteLine(
				Bot.Msg("Getting {0} last revisons of \"{1}\" page..."), lastRevisions, pageTitle);
			string queryUri = site.site + site.indexPath +
				"api.php?action=query&prop=revisions&titles=" +
				HttpUtility.UrlEncode(pageTitle) + "&rvprop=ids|user|comment|timestamp" +
				(loadTexts ? "|content" : "") + "&format=xml&rvlimit=" + lastRevisions.ToString();
			Page p;
			using (XmlReader reader = XmlReader.Create(queryUri)) {
				reader.ReadToFollowing("api");
				reader.Read();
				if (reader.Name == "error")
					Console.Error.WriteLine(Bot.Msg("Error: {0}"), reader.GetAttribute("info"));
				while (reader.ReadToFollowing("rev")) {
					p = new Page(site, pageTitle);
					p.lastRevisionID = reader.GetAttribute("revid");
					p.lastUser = reader.GetAttribute("user");
					p.comment = reader.GetAttribute("comment");
					p.timestamp =
						DateTime.Parse(reader.GetAttribute("timestamp")).ToUniversalTime();
					if (loadTexts)
						p.text = reader.ReadString();
					pages.Add(p);
				}
			}
			Console.WriteLine(Bot.Msg("PageList filled with {0} last revisons of \"{1}\" page."),
				pages.Count, pageTitle);
		}

		/// <summary>Gets page titles for this PageList from links in some wiki page. But only
		/// links to articles and pages from Project, Template and Help namespaces will be
		/// retrieved. And no interwiki links. Use FillFromAllPageLinks function instead
		/// to filter namespaces manually.</summary>
		/// <param name="pageTitle">Page name to get links from.</param>
		public void FillFromPageLinks(string pageTitle)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			FillFromAllPageLinks(pageTitle);
			FilterNamespaces(new int[] {0,4,10,12});
		}

		/// <summary>Gets page titles for this PageList from all links in some wiki page. All links
		/// will be retrieved, from all standard namespaces, except interwiki links to other
		/// sites. Use FillFromPageLinks function instead to filter namespaces
		/// automatically.</summary>
		/// <param name="pageTitle">Page title as string.</param>
		/// <example><code>pageList.FillFromAllPageLinks("Art");</code></example>
		public void FillFromAllPageLinks(string pageTitle)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			if (string.IsNullOrEmpty(Site.WMLangsStr))
				site.GetWikimediaWikisList();
			Regex wikiLinkRE = new Regex(@"\[\[:*(.+?)(]]|\|)");
			Page page = new Page(site, pageTitle);
			page.Load();
			MatchCollection matches = wikiLinkRE.Matches(page.text);
			Regex outWikiLink = new Regex("^(" + Site.WMLangsStr +
				/*"|" + Site.WMSitesStr + */ "):");
			foreach (Match match in matches)
				if (!outWikiLink.IsMatch(match.Groups[1].Value))
					pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine(
				Bot.Msg("PageList filled with links, found on \"{0}\" page."), pageTitle);
		}

		/// <summary>Gets page titles for this PageList from "Special:Whatlinkshere" Mediawiki page
		/// of specified page. That means the titles of pages, referring to the specified page.
		/// But not more than 5000 titles. The function does not internally remove redirecting
		///	pages from the results. Call RemoveRedirects() manually, if you need it. And the
		/// function does not clears the existing PageList, so new titles will be added.</summary>
		/// <param name="pageTitle">Page title as string.</param>
		public void FillFromLinksToPage(string pageTitle)
		{
			if (string.IsNullOrEmpty(pageTitle))
				throw new ArgumentNullException("pageTitle");
			//RemoveAll();
			string res = site.site + site.indexPath +
				"index.php?title=Special:Whatlinkshere/" +
				HttpUtility.UrlEncode(pageTitle) + "&limit=5000";
			string src = site.GetPageHTM(res);
			MatchCollection matches = site.linkToPageRE1.Matches(src);
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			//RemoveRedirects();
			Console.WriteLine(
				Bot.Msg("PageList filled with titles of pages, referring to \"{0}\" page."),
				pageTitle);
		}

		/// <summary>Gets titles of pages, in which the specified image file is included.
		/// Function also works with non-image files.</summary>
		/// <param name="imageFileTitle">File title. With or without "Image:" or
		/// "File:" prefix.</param>
		public void FillFromPagesUsingImage(string imageFileTitle)
		{
			if (string.IsNullOrEmpty(imageFileTitle))
				throw new ArgumentNullException("imageFileTitle");
			imageFileTitle = site.RemoveNSPrefix(imageFileTitle, 6);
			string res = site.site + site.indexPath + "index.php?title=" +
				HttpUtility.UrlEncode(site.namespaces["6"].ToString()) + ":" +
				HttpUtility.UrlEncode(imageFileTitle);
			string src = site.GetPageHTM(res);
			if (site.messages == null || !site.messages.Contains("linkstoimage"))
				site.GetMediaWikiMessagesEx(false);
			int pos = src.IndexOf(site.messages["linkstoimage"].text);
			if (pos <= 0) {
				Console.Error.WriteLine(Bot.Msg("No page contains \"{0}\" image."), imageFileTitle);
				return;
			}
			src = src.Substring(pos, src.IndexOf("<div class=\"printfooter\">") - pos);
			MatchCollection matches = site.linkToPageRE1.Matches(src);
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(
				Bot.Msg("PageList filled with titles of pages, that contain \"{0}\" image."),
				imageFileTitle);
		}

		/// <summary>Gets page titles for this PageList from user contributions
		/// of specified user. The function does not internally remove redirecting
		/// pages from the results. Call RemoveRedirects() manually, if you need it. And the
		/// function does not clears the existing PageList, so new titles will be added.</summary>
		/// <param name="userName">User's name.</param>
		/// <param name="limit">Maximum number of page titles to get.</param>
		public void FillFromUserContributions(string userName, int limit)
		{
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentNullException("userName");
			if (limit <= 0)
				throw new ArgumentOutOfRangeException("limit", Bot.Msg("Limit must be positive."));
			string res = site.site + site.indexPath +
				"index.php?title=Special:Contributions&target=" + HttpUtility.UrlEncode(userName) +
				"&limit=" + limit.ToString();
			string src = site.GetPageHTM(res);
			MatchCollection matches = site.linkToPageRE2.Matches(src);
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(
				Bot.Msg("PageList filled with user's \"{0}\" contributions."), userName);
		}

		/// <summary>Gets page titles for this PageList from watchlist
		/// of bot account. The function does not internally remove redirecting
		/// pages from the results. Call RemoveRedirects() manually, if you need that. And the
		/// function neither filters namespaces, nor clears the existing PageList,
		/// so new titles will be added to the existing in PageList.</summary>
		public void FillFromWatchList()
		{
			string src = site.GetPageHTM(site.indexPath + "index.php?title=Special:Watchlist/edit");
			MatchCollection matches = site.linkToPageRE2.Matches(src);
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(Bot.Msg("PageList filled with bot account's watchlist."));
		}

		/// <summary>Gets page titles for this PageList from list of recently changed
		/// watched articles (watched by bot account). The function does not internally
		/// remove redirecting pages from the results. Call RemoveRedirects() manually,
		/// if you need it. And the function neither filters namespaces, nor clears
		/// the existing PageList, so new titles will be added to the existing
		/// in PageList.</summary>
		public void FillFromChangedWatchedPages()
		{
			string src = site.GetPageHTM(site.indexPath + "index.php?title=Special:Watchlist/edit");
			MatchCollection matches = site.linkToPageRE2.Matches(src);
			Console.WriteLine(src);
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(
				Bot.Msg("PageList filled with changed pages from bot account's watchlist."));
		}

		/// <summary>Gets page titles for this PageList from wiki site internal search results.
		/// The function does not filter namespaces. And the function does not clear
		/// the existing PageList, so new titles will be added.</summary>
		/// <param name="searchStr">String to search.</param>
		/// <param name="limit">Maximum number of page titles to get.</param>
		public void FillFromSearchResults(string searchStr, int limit)
		{
			if (string.IsNullOrEmpty(searchStr))
				throw new ArgumentNullException("searchStr");
			if (limit <= 0)
				throw new ArgumentOutOfRangeException("limit", Bot.Msg("Limit must be positive."));
			string res = site.site + site.indexPath +
				"index.php?title=Special:Search&fulltext=Search&search=" +
				HttpUtility.UrlEncode(searchStr) + "&limit=" + limit.ToString();
			string src = site.GetPageHTM(res);
			src = src.Substring(src.IndexOf("<ul class='mw-search-results'>"));
			MatchCollection matches = site.linkToPageRE2.Matches(src);
			foreach (Match match in matches)
				pages.Add(new Page(site, HttpUtility.HtmlDecode(match.Groups[1].Value)));
			Console.WriteLine(Bot.Msg("PageList filled with search results."));
		}

		/// <summary>Gets page titles for this PageList from www.google.com search results.
		/// The function does not filter namespaces. And the function does not clear
		/// the existing PageList, so new titles will be added.</summary>
		/// <param name="searchStr">String to search.</param>
		/// <param name="limit">Maximum number of page titles to get.</param>
		public void FillFromGoogleSearchResults(string searchStr, int limit)
		{
			if (string.IsNullOrEmpty(searchStr))
				throw new ArgumentNullException("searchStr");
			if (limit <= 0)
				throw new ArgumentOutOfRangeException("limit", Bot.Msg("Limit must be positive."));
			Uri res = new Uri("http://www.google.com/search?q=" + HttpUtility.UrlEncode(searchStr) +
				"+site:" + site.site.Substring(site.site.IndexOf("://") + 3) +
				"&num=" + limit.ToString());
			string src = Bot.GetWebResource(res, "");
			Regex GoogleLinkToPageRE = new Regex("<a href=\"" + Regex.Escape(site.site) + "(" +
				(string.IsNullOrEmpty(site.wikiPath) == false ?
					Regex.Escape(site.wikiPath) + "|" : "") +
				Regex.Escape(site.indexPath) + @"index\.php\?title=)" +
				"([^\"]+?)\" class=\"?l\"?>");
			MatchCollection matches = GoogleLinkToPageRE.Matches(src);
			foreach (Match match in matches)
				pages.Add(new Page(site,
					HttpUtility.UrlDecode(match.Groups[2].Value).Replace("_", " ")));
			Console.WriteLine(Bot.Msg("PageList filled with www.google.com search results."));
		}

		/// <summary>Gets page titles from UTF8-encoded file. Each title must be on new line.
		/// The function does not clear the existing PageList, so new pages will be added.</summary>
		public void FillFromFile(string filePathName)
		{
			//RemoveAll();
			StreamReader strmReader = new StreamReader(filePathName);
			string input;
			while ((input = strmReader.ReadLine()) != null) {
				input = input.Trim(" []".ToCharArray());
				if (string.IsNullOrEmpty(input) != true)
					pages.Add(new Page(site, input));
			}
			strmReader.Close();
			Console.WriteLine(
				Bot.Msg("PageList filled with titles, found in \"{0}\" file."), filePathName);
		}

		/// <summary>Protects or unprotects all pages in this PageList, so only chosen category
		/// of users can edit or rename it. Changing page protection modes requires administrator
		/// (sysop) rights on target wiki.</summary>
		/// <param name="editMode">Protection mode for editing this page (0 = everyone allowed
		/// to edit, 1 = only registered users are allowed, 2 = only administrators are allowed 
		/// to edit).</param>
		/// <param name="renameMode">Protection mode for renaming this page (0 = everyone allowed to
		/// rename, 1 = only registered users are allowed, 2 = only administrators
		/// are allowed).</param>
		/// <param name="cascadeMode">In cascading mode, all the pages, included into this page
		/// (e.g., templates or images) are also fully automatically protected.</param>
		/// <param name="expiryDate">Date ant time, expressed in UTC, when the protection expires
		/// and page becomes fully unprotected. Use DateTime.ToUniversalTime() method to convert
		/// local time to UTC, if necessary. Pass DateTime.MinValue to make protection
		/// indefinite.</param>
		/// <param name="reason">Reason for protecting this page.</param>
		public void Protect(int editMode, int renameMode, bool cascadeMode,
			DateTime expiryDate, string reason)
		{
			if (IsEmpty())
				throw new WikiBotException(Bot.Msg("The PageList is empty. Nothing to protect."));
			foreach (Page p in pages)
				p.Protect(editMode, renameMode, cascadeMode, expiryDate, reason);
		}

		/// <summary>Adds all pages in this PageList to bot account's watchlist.</summary>
		public void Watch()
		{
			if (IsEmpty())
				throw new WikiBotException(Bot.Msg("The PageList is empty. Nothing to watch."));
			foreach (Page p in pages)
				p.Watch();
		}

		/// <summary>Removes all pages in this PageList from bot account's watchlist.</summary>
		public void Unwatch()
		{
			if (IsEmpty())
				throw new WikiBotException(Bot.Msg("The PageList is empty. Nothing to unwatch."));
			foreach (Page p in pages)
				p.Unwatch();
		}

		/// <summary>Removes the pages, that are not in given namespaces.</summary>
		/// <param name="neededNSs">Array of integers, presenting keys of namespaces
		/// to retain.</param>
		/// <example><code>pageList.FilterNamespaces(new int[] {0,3});</code></example>
		public void FilterNamespaces(int[] neededNSs)
		{
			for (int i=pages.Count-1; i >= 0; i--) {
				if (Array.IndexOf(neededNSs, pages[i].GetNamespace()) == -1)
					pages.RemoveAt(i); }
		}

		/// <summary>Removes the pages, that are in given namespaces.</summary>
		/// <param name="needlessNSs">Array of integers, presenting keys of namespaces
		/// to remove.</param>
		/// <example><code>pageList.RemoveNamespaces(new int[] {2,4});</code></example>
		public void RemoveNamespaces(int[] needlessNSs)
		{
			for (int i=pages.Count-1; i >= 0; i--) {
				if (Array.IndexOf(needlessNSs, pages[i].GetNamespace()) != -1)
					pages.RemoveAt(i); }
		}

		/// <summary>This function sorts all pages in PageList by titles.</summary>
		public void Sort()
		{
			if (IsEmpty())
				throw new WikiBotException(Bot.Msg("The PageList is empty. Nothing to sort."));
			pages.Sort(ComparePagesByTitles);
		}

		/// <summary>This internal function compares pages by titles (alphabetically).</summary>
		/// <returns>Returns 1 if x is greater, -1 if y is greater, 0 if equal.</returns>
		public int ComparePagesByTitles(Page x, Page y)
		{
			int r = string.Compare(x.title, y.title, false, site.langCulture);
			return (r != 0) ? r/Math.Abs(r) : 0;
		}

		/// <summary>Removes all pages in PageList from specified category by deleting
		/// links to that category in pages texts.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void RemoveFromCategory(string categoryName)
		{
			foreach (Page p in pages)
				p.RemoveFromCategory(categoryName);
		}

		/// <summary>Adds all pages in PageList to the specified category by adding
		/// links to that category in pages texts.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void AddToCategory(string categoryName)
		{
			foreach (Page p in pages)
				p.AddToCategory(categoryName);
		}

		/// <summary>Adds a specified template to the end of all pages in PageList.</summary>
		/// <param name="templateText">Template text, like "{{template_name|...|...}}".</param>
		public void AddTemplate(string templateText)
		{
			foreach (Page p in pages)
				p.AddTemplate(templateText);
		}

		/// <summary>Removes a specified template from all pages in PageList.</summary>
		/// <param name="templateTitle">Title of template  to remove.</param>
		public void RemoveTemplate(string templateTitle)
		{
			foreach (Page p in pages)
				p.RemoveTemplate(templateTitle);
		}

		/// <summary>Loads text for pages in PageList from site via common web interface.
		/// Please, don't use this function when going to edit big amounts of pages on
		/// popular public wikis, as it compromises edit conflict detection. In that case,
		/// each page's text should be loaded individually right before its processing
		/// and saving.</summary>
		public void Load()
		{
			if (IsEmpty())
				throw new WikiBotException(Bot.Msg("The PageList is empty. Nothing to load."));
			foreach (Page page in pages)
				page.Load();
		}

		/// <summary>Loads text and metadata for pages in PageList via XML export interface.
		/// Non-existent pages will be automatically removed from the PageList.
		/// Please, don't use this function when going to edit big amounts of pages on
		/// popular public wikis, as it compromises edit conflict detection. In that case,
		/// each page's text should be loaded individually right before its processing
		/// and saving.</summary>
		public void LoadEx()
		{
			if (IsEmpty())
				throw new WikiBotException(Bot.Msg("The PageList is empty. Nothing to load."));
			Console.WriteLine(Bot.Msg("Loading {0} pages..."), pages.Count);
			string res = site.site + site.indexPath +
				"index.php?title=Special:Export&action=submit";
			string postData = "curonly=True&pages=";
			foreach (Page page in pages)
				postData += HttpUtility.UrlEncode(page.title) + "\r\n";
			XmlReader reader = XmlReader.Create(
				new StringReader(site.PostDataAndGetResultHTM(res, postData)));
			Clear();
			while (reader.ReadToFollowing("page")) {
				Page p = new Page(site, "");
				p.ParsePageXML(reader.ReadOuterXml());
				pages.Add(p);
			}
			reader.Close();
		}

		/// <summary>Loads text and metadata for pages in PageList via XML export interface.
		/// The function uses XPathNavigator and is less efficient than LoadEx().</summary>
		public void LoadEx2()
		{
			if (IsEmpty())
				throw new WikiBotException("The PageList is empty. Nothing to load.");
			Console.WriteLine(Bot.Msg("Loading {0} pages..."), pages.Count);
			string res = site.site + site.indexPath +
				"index.php?title=Special:Export&action=submit";
			string postData = "curonly=True&pages=";
			foreach (Page page in pages)
				postData += page.title + "\r\n";
			StringReader strReader =
				new StringReader(site.PostDataAndGetResultHTM(res, postData));
			XPathDocument doc = new XPathDocument(strReader);
			strReader.Close();
			XPathNavigator nav = doc.CreateNavigator();
			foreach (Page page in pages)
			{
				if (page.title.Contains("'")) {
					page.LoadEx();
					continue;
				}
				string query = "//ns:page[ns:title='" + page.title + "']/";
				try {
					page.text =
						nav.SelectSingleNode(query + "ns:revision/ns:text", site.xmlNS).InnerXml;
				}
				catch (System.NullReferenceException) {
					continue;
				}
				page.text = HttpUtility.HtmlDecode(page.text);
				page.pageID = nav.SelectSingleNode(query + "ns:id", site.xmlNS).InnerXml;
				try {
					page.lastUser = nav.SelectSingleNode(query +
						"ns:revision/ns:contributor/ns:username", site.xmlNS).InnerXml;
					page.lastUserID = nav.SelectSingleNode(query +
						"ns:revision/ns:contributor/ns:id", site.xmlNS).InnerXml;
				}
				catch (System.NullReferenceException) {
					page.lastUser = nav.SelectSingleNode(query +
						"ns:revision/ns:contributor/ns:ip", site.xmlNS).InnerXml;
				}
				page.lastUser = HttpUtility.HtmlDecode(page.lastUser);
				page.lastRevisionID = nav.SelectSingleNode(query +
					"ns:revision/ns:id", site.xmlNS).InnerXml;
				page.lastMinorEdit = (nav.SelectSingleNode(query +
					"ns:revision/ns:minor", site.xmlNS) == null) ? false : true;
				try {
					page.comment = nav.SelectSingleNode(query +
						"ns:revision/ns:comment", site.xmlNS).InnerXml;
					page.comment = HttpUtility.HtmlDecode(page.comment);
				}
				catch (System.NullReferenceException) {;}
				page.timestamp = nav.SelectSingleNode(query +
					"ns:revision/ns:timestamp", site.xmlNS).ValueAsDateTime;
			}
			Console.WriteLine(Bot.Msg("Pages download completed."));
		}

		/// <summary>Loads text and metadata for pages in PageList via XML export interface.
		/// The function loads pages one by one, it is slightly less efficient
		/// than LoadEx().</summary>
		public void LoadEx3()
		{
			if (IsEmpty())
				throw new WikiBotException("The PageList is empty. Nothing to load.");
			foreach (Page p in pages)
				p.LoadEx();
		}

		/// <summary>Gets page titles and page text from local XML dump.
		/// This function consumes much resources.</summary>
		/// <param name="filePathName">The path to and name of the XML dump file as string.</param>
		public void FillAndLoadFromXMLDump(string filePathName)
		{
			Console.WriteLine(Bot.Msg("Loading pages from XML dump..."));
			XmlReader reader = XmlReader.Create(filePathName);
			while (reader.ReadToFollowing("page")) {
				Page p = new Page(site, "");
				p.ParsePageXML(reader.ReadOuterXml());
				pages.Add(p);
			}
			reader.Close();
			Console.WriteLine(Bot.Msg("XML dump loaded successfully."));
		}

		/// <summary>Gets page titles and page texts from all ".txt" files in the specified
		/// directory (folder). Each file becomes a page. Page titles are constructed from
		/// file names. Page text is read from file contents. If any Unicode numeric codes
		/// (also known as numeric character references or NCRs) of the forbidden characters
		/// (forbidden in filenames) are recognized in filenames, those codes are converted
		/// to characters (e.g. "&#x7c;" is converted to "|").</summary>
		/// <param name="dirPath">The path and name of a directory (folder)
		/// to load files from.</param>
		public void FillAndLoadFromFiles(string dirPath)
		{
			foreach (string fileName in Directory.GetFiles(dirPath, "*.txt")) {
				Page p = new Page(site, Path.GetFileNameWithoutExtension(fileName));
				p.title = p.title.Replace("&#x22;", "\"");
				p.title = p.title.Replace("&#x3c;", "<");
				p.title = p.title.Replace("&#x3e;", ">");
				p.title = p.title.Replace("&#x3f;", "?");
				p.title = p.title.Replace("&#x3a;", ":");
				p.title = p.title.Replace("&#x5c;", "\\");
				p.title = p.title.Replace("&#x2f;", "/");
				p.title = p.title.Replace("&#x2a;", "*");
				p.title = p.title.Replace("&#x7c;", "|");
				p.LoadFromFile(fileName);
				pages.Add(p);
			}
		}

		/// <summary>Saves all pages in PageList to live wiki site. Uses default bot
		/// edit comment and default minor edit mark setting ("true" by default). This function
		/// doesn't limit the saving speed, so in case of working on public wiki, it's better
		/// to use SaveSmoothly function in order not to overload public server (HTTP errors or
		/// framework errors may arise in case of overloading).</summary>
		public void Save()
		{
			Save(Bot.editComment, Bot.isMinorEdit);
		}

		/// <summary>Saves all pages in PageList to live wiki site. This function
		/// doesn't limit the saving speed, so in case of working on public wiki, it's better
		/// to use SaveSmoothly function in order not to overload public server (HTTP errors or
		/// framework errors may arise in case of overloading).</summary>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>
		public void Save(string comment, bool isMinorEdit)
		{
			foreach (Page page in pages)
				page.Save(page.text, comment, isMinorEdit);
		}

		/// <summary>Saves all pages in PageList to live wiki site. The function waits for 5 seconds
		/// between each page save operation in order not to overload server. Uses default bot
		/// edit comment and default minor edit mark setting ("true" by default). This function
		/// doesn't limit the saving speed, so in case of working on public wiki, it's better
		/// to use SaveSmoothly function in order not to overload public server (HTTP errors or
		/// framework errors may arise in case of overloading).</summary>
		public void SaveSmoothly()
		{
			SaveSmoothly(5, Bot.editComment, Bot.isMinorEdit);
		}

		/// <summary>Saves all pages in PageList to live wiki site. The function waits for specified
		/// number of seconds between each page save operation in order not to overload server.
		/// Uses default bot edit comment and default minor edit mark setting
		/// ("true" by default).</summary>
		/// <param name="intervalSeconds">Number of seconds to wait between each
		/// save operation.</param>
		public void SaveSmoothly(int intervalSeconds)
		{
			SaveSmoothly(intervalSeconds, Bot.editComment, Bot.isMinorEdit);
		}

		/// <summary>Saves all pages in PageList to live wiki site. The function waits for specified
		/// number of seconds between each page save operation in order not to overload
		/// server.</summary>
		/// <param name="intervalSeconds">Number of seconds to wait between each
		/// save operation.</param>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>
		public void SaveSmoothly(int intervalSeconds, string comment, bool isMinorEdit)
		{
			if (intervalSeconds == 0)
				intervalSeconds = 1;
			foreach (Page page in pages) {
				Thread.Sleep(intervalSeconds * 1000);
				page.Save(page.text, comment, isMinorEdit);
			}
		}

		/// <summary>Undoes the last edit of every page in this PageList, so every page text reverts
		/// to previous contents. The function doesn't affect other operations
		/// like renaming.</summary>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>
		public void Revert(string comment, bool isMinorEdit)
		{
			foreach (Page page in pages)
				page.Revert(comment, isMinorEdit);
		}

		/// <summary>Saves titles of all pages in PageList to the specified file. Each title
		/// on a new line. If the target file already exists, it is overwritten.</summary>
		/// <param name="filePathName">The path to and name of the target file as string.</param>
		public void SaveTitlesToFile(string filePathName)
		{
			SaveTitlesToFile(filePathName, false);
		}

		/// <summary>Saves titles of all pages in PageList to the specified file. Each title
		/// on a separate line. If the target file already exists, it is overwritten.</summary>
		/// <param name="filePathName">The path to and name of the target file as string.</param>
		/// <param name="useSquareBrackets">If true, each page title is enclosed
		/// in square brackets.</param>
		public void SaveTitlesToFile(string filePathName, bool useSquareBrackets)
		{
			StringBuilder titles = new StringBuilder();
			foreach (Page page in pages)
				titles.Append(useSquareBrackets ?
					"[[" + page.title + "]]\r\n" : page.title + "\r\n");
			File.WriteAllText(filePathName, titles.ToString().Trim(), Encoding.UTF8);
			Console.WriteLine(Bot.Msg("Titles in PageList saved to \"{0}\" file."), filePathName);
		}

		/// <summary>Saves the contents of all pages in pageList to ".txt" files in specified
		/// directory. Each page is saved to separate file, the name of that file is constructed
		/// from page title. Forbidden characters in filenames are replaced with their
		/// Unicode numeric codes (also known as numeric character references or NCRs).
		/// If the target file already exists, it is overwritten.</summary>
		/// <param name="dirPath">The path and name of a directory (folder)
		/// to save files to.</param>
		public void SaveToFiles(string dirPath)
		{
			string curDirPath = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(dirPath);
			foreach (Page page in pages)
				page.SaveToFile();
			Directory.SetCurrentDirectory(curDirPath);
		}

		/// <summary>Loads the contents of all pages in pageList from live site via XML export
		/// and saves the retrieved XML content to the specified file. The functions just dumps
		/// data, it does not load pages in PageList itself, call LoadEx() or
		/// FillAndLoadFromXMLDump() to do that. Note, that on some sites, MediaWiki messages
		/// from standard namespace 8 are not available for export.</summary>
		/// <param name="filePathName">The path to and name of the target file as string.</param>
		public void SaveXMLDumpToFile(string filePathName)
		{
			Console.WriteLine(Bot.Msg("Loading {0} pages for XML dump..."), this.pages.Count);
			string res = site.site + site.indexPath +
				"index.php?title=Special:Export&action=submit";
			string postData = "catname=&curonly=true&action=submit&pages=";
			foreach (Page page in pages)
				postData += HttpUtility.UrlEncode(page.title + "\r\n");
			string rawXML = site.PostDataAndGetResultHTM(res, postData);
			rawXML = rawXML.Replace("\n", "\r\n");
			if (File.Exists(filePathName))
				File.Delete(filePathName);
			FileStream fs = File.Create(filePathName);
			byte[] XMLBytes = new System.Text.UTF8Encoding(true).GetBytes(rawXML);
			fs.Write(XMLBytes, 0, XMLBytes.Length);
			fs.Close();
			Console.WriteLine(
				Bot.Msg("XML dump successfully saved in \"{0}\" file."), filePathName);
		}

		/// <summary>Removes all empty pages from PageList. But firstly don't forget to load
		/// the pages from site using pageList.LoadEx().</summary>
		public void RemoveEmpty()
		{
			for (int i=pages.Count-1; i >= 0; i--)
				if (pages[i].IsEmpty())
					pages.RemoveAt(i);
		}

		/// <summary>Removes all recurring pages from PageList. Only one page with some title will
		/// remain in PageList. This makes all page elements in PageList unique.</summary>
		public void RemoveRecurring()
		{
			for (int i=pages.Count-1; i >= 0; i--)
				for (int j=i-1; j >= 0; j--)
					if (pages[i].title == pages[j].title) {
						pages.RemoveAt(i);
						break;
					}
		}

		/// <summary>Removes all redirecting pages from PageList. But firstly don't forget to load
		/// the pages from site using pageList.LoadEx().</summary>
		public void RemoveRedirects()
		{
			for (int i=pages.Count-1; i >= 0; i--)
				if (pages[i].IsRedirect())
					pages.RemoveAt(i);
		}

		/// <summary>For all redirecting pages in this PageList, this function loads the titles and
		/// texts of redirected-to pages.</summary>
		public void ResolveRedirects()
		{
			foreach (Page page in pages) {
				if (page.IsRedirect() == false)
					continue;
				page.title = page.RedirectsTo();
				page.Load();
			}
		}

		/// <summary>Removes all disambiguation pages from PageList. But firstly don't
		/// forget to load the pages from site using pageList.LoadEx().</summary>
		public void RemoveDisambigs()
		{
			for (int i=pages.Count-1; i >= 0; i--)
				if (pages[i].IsDisambig())
					pages.RemoveAt(i);
		}


		/// <summary>Removes all pages from PageList.</summary>
		public void RemoveAll()
		{
			pages.Clear();
		}

		/// <summary>Removes all pages from PageList.</summary>
		public void Clear()
		{
			pages.Clear();
		}

		/// <summary>Function changes default English namespace prefixes to correct local prefixes
		/// (e.g. for German wiki-sites it changes "Category:..." to "Kategorie:...").</summary>
		public void CorrectNSPrefixes()
		{
			foreach (Page p in pages)
				p.CorrectNSPrefix();
		}

		/// <summary>Shows if there are any Page objects in this PageList.</summary>
		/// <returns>Returns bool value.</returns>
		public bool IsEmpty()
		{
			return (pages.Count == 0) ? true : false;
		}

		/// <summary>Sends titles of all contained pages to console.</summary>
		public void ShowTitles()
		{
			Console.WriteLine("\n" + Bot.Msg("Pages in this PageList:"));
			foreach (Page p in pages)
				Console.WriteLine(p.title);
			Console.WriteLine("\n");
		}

		/// <summary>Sends texts of all contained pages to console.</summary>
		public void ShowTexts()
		{
			Console.WriteLine("\n" + Bot.Msg("Texts of all pages in this PageList:"));
			Console.WriteLine("--------------------------------------------------");
			foreach (Page p in pages) {
				p.ShowText();
				Console.WriteLine("--------------------------------------------------");
			}
			Console.WriteLine("\n");
		}
	}

	/// <summary>Class establishes custom application exceptions.</summary>
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[SerializableAttribute]
	public class WikiBotException : System.Exception
	{
		/// <summary>Just overriding default constructor.</summary>
		/// <returns>Returns Exception object.</returns>
		public WikiBotException() {}
		/// <summary>Just overriding constructor.</summary>
		/// <returns>Returns Exception object.</returns>
		public WikiBotException(string message)
			: base (message) { Console.Beep(); /*Console.ForegroundColor = ConsoleColor.Red;*/ }
		/// <summary>Just overriding constructor.</summary>
		/// <returns>Returns Exception object.</returns>
		public WikiBotException(string message, System.Exception inner)
			: base (message, inner) {}
		/// <summary>Just overriding constructor.</summary>
		/// <returns>Returns Exception object.</returns>
		protected WikiBotException(System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base (info, context) {}
		/// <summary>Destructor is invoked automatically when exception object becomes
		/// inaccessible.</summary>
		~WikiBotException() {}
	}

	/// <summary>Class defines a Bot instance, some configuration settings
	/// and some auxiliary functions.</summary>
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class Bot
	{
		/// <summary>Title and description of web agent.</summary>
		public static readonly string botVer = "DotNetWikiBot";
		/// <summary>Version of DotNetWikiBot Framework.</summary>
		public static readonly Version version = new Version("2.72");
		/// <summary>Desired bot's messages language (ISO 639-1 language code).
		/// If not set explicitly, the language will be detected automatically.</summary>
		/// <example><code>Bot.botMessagesLang = "fr";</code></example>
		public static string botMessagesLang = null;
		/// <summary>Default edit comment. You can set it to what you like.</summary>
		/// <example><code>Bot.editComment = "My default edit comment";</code></example>
		public static string editComment = "Automatic page editing";
		/// <summary>If set to true, all the bot's edits are marked as minor by default.</summary>
		public static bool isMinorEdit = true;
		/// <summary>If true, the bot uses "MediaWiki API" extension
		/// (special MediaWiki bot interface, "api.php"), if it is available.
		/// If false, the bot uses common user interface. True by default.
		/// Set it to false manually, if some problem with bot interface arises on site.</summary>
		/// <example><code>Bot.useBotQuery = false;</code></example>
		public static bool useBotQuery = true;
		/// <summary>Number of times to retry bot action in case of temporary connection failure or
		/// some other common net problems.</summary>
		public static int retryTimes = 3;
		/// <summary>If true, the bot asks user to confirm next Save, RenameTo or Delete operation.
		/// False by default. Set it to true manually, when necessary.</summary>
		/// <example><code>Bot.askConfirm = true;</code></example>
		public static bool askConfirm = false;
		/// <summary>If true, bot only reports errors and warnings. Call EnableSilenceMode
		/// function to enable that mode, don't change this variable's value manually.</summary>
		public static bool silenceMode = false;
		/// <summary>If set to some file name (e.g. "DotNetWikiBot_Report.txt"), the bot
		/// writes all output to that file instead of a console. If no path was specified,
		/// the bot creates that file in it's current directory. File is encoded in UTF-8.
		/// Call EnableLogging function to enable log writing, don't change this variable's
		/// value manually.</summary>
		public static string logFile = null;

		/// <summary>Array, containing localized DotNetWikiBot interface messages.</summary>
		public static SortedDictionary<string, string> messages =
			new SortedDictionary<string, string>();
		/// <summary>Internal web client, that is used to access sites.</summary>
		public static WebClient wc = new WebClient();
		/// <summary>Content type for HTTP header of web client.</summary>
		public static readonly string webContentType = "application/x-www-form-urlencoded";
		/// <summary>If true, assembly is running on Mono framework. If false,
		/// it is running on original Microsoft .NET Framework. This variable is set
		/// automatically, just get it's value, don't change it.</summary>
		public static readonly bool isRunningOnMono = (Type.GetType("Mono.Runtime") != null);
		/// <summary>Initial state of HttpWebRequestElement.UseUnsafeHeaderParsing boolean
		/// configuration setting. 0 means true, 1 means false, 2 means unchanged.</summary>
		public static int unsafeHttpHeaderParsingUsed = 2;

		/// <summary>This constructor is used to generate Bot object.</summary>
		/// <returns>Returns Bot object.</returns>
		static Bot()
		{
			if (botMessagesLang == null)
				botMessagesLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
			if (botMessagesLang != "en")
				if (!LoadLocalizedMessages(botMessagesLang))
					botMessagesLang = "en";
			botVer += "/" + version + " (" + Environment.OSVersion.VersionString + "; " +
				".NET CLR " + Environment.Version.ToString() + ")";
			ServicePointManager.Expect100Continue = false;
		}

		/// <summary>The destructor is used to uninitialize Bot objects.</summary>
		~Bot()
		{
			//if (unsafeHttpHeaderParsingUsed != 2)
				//SwitchUnsafeHttpHeaderParsing(unsafeHttpHeaderParsingUsed == 1 ? true : false);
		}

		/// <summary>Call this function to make bot write all output to the specified file
		/// instead of a console. If only error logging is desirable, first call this
		/// function, and after that call EnableSilenceMode function.</summary>
		/// <param name="logFileName">Path and name of a file to write output to.
		/// If no path was specified, the bot creates that file in it's current directory.
		/// File is encoded in UTF-8.</param>
		public static void EnableLogging(string logFileName)
		{
			logFile = logFileName;
			StreamWriter log = File.AppendText(logFile);
			log.AutoFlush = true;
			Console.SetError(log);
			if (!silenceMode)
				Console.SetOut(log);
		}

		/// <summary>Call this function to make bot report only errors and warnings,
		/// no other messages will be displayed or logged.</summary>
		public static void EnableSilenceMode()
		{
			silenceMode = true;
			Console.SetOut(new StringWriter());
		}

		/// <summary>Function loads localized bot interface messages from 
		/// "DotNetWikiBot.i18n.xml" file. Function is called in Bot class constructor, 
		/// but can also be called manually to change interface language at runtime.</summary>
		/// <param name="language">Desired language's ISO 639-1 code.</param>
		/// <returns>Returns false, if messages for specified language were not found.
		/// Returns true on success.</returns>
		public static bool LoadLocalizedMessages(string language)
		{
			if (!File.Exists("DotNetWikiBot.i18n.xml")) {
				Console.Error.WriteLine("Localization file \"DotNetWikiBot.i18n.xml\" is missimg.");
				return false;
			}
			using (XmlReader reader = XmlReader.Create("DotNetWikiBot.i18n.xml")) {
				if (!reader.ReadToFollowing(language)) {
					Console.Error.WriteLine("\nLocalized messages not found for language \"{0}\"." +
						"\nYou can help DotNetWikiBot project by translating the messages in\n" +
						"\"DotNetWikiBot.i18n.xml\" file and sending it to developers for " +
						"distribution.\n", language);
					return false;
				}
				if (!reader.ReadToDescendant("msg"))
					return false;
				else {
					if (messages.Count > 0)
						messages.Clear();
					messages[reader["id"]] = reader.ReadString();
				}
				while (reader.ReadToNextSibling("msg"))
					messages[reader["id"]] = reader.ReadString();
			}
			return true;
		}

		/// <summary>The function gets localized (translated) form of the specified bot
		/// interface message.</summary>
		/// <param name="message">Message itself, placeholders for substituted parameters are
		/// denoted in curly brackets like {0}, {1}, {2} and so on.</param>
		/// <returns>Returns localized form of the specified bot interface message,
		/// or English form if localized form was not found.</returns>
		public static string Msg(string message)
		{
			if (botMessagesLang == "en")
				return message;
			try {
				return messages[message];
			}
			catch (KeyNotFoundException) {
				return message;
			}
		}

		/// <summary>This function asks user to confirm next action. The message
		/// "Would you like to proceed (y/n/a)? " is displayed and user response is
		/// evaluated. Make sure to set "askConfirm" variable to "true" before
		/// calling this function.</summary>
		/// <returns>Returns true, if user has confirmed the action.</returns>
		/// <example><code>if (Bot.askConfirm) {
		///     Console.Write("Some action on live wiki is going to occur.\n\n");
		///     if(!Bot.UserConfirms())
		///         return;
		/// }</code></example>
		public static bool UserConfirms()
		{
			if (!askConfirm)
				return true;
			ConsoleKeyInfo k;
			Console.Write(Bot.Msg("Would you like to proceed (y/n/a)?") + " ");
			k = Console.ReadKey();
			Console.Write("\n");
			if (k.KeyChar == 'y')
				return true;
			else if (k.KeyChar == 'a') {
				askConfirm = false;
				return true;
			}
			else
				return false;
		}

		/// <summary>This auxiliary function counts the occurrences of specified string
		/// in specified text. This count is often needed, but strangely there is no
		/// such function in .NET Framework's String class.</summary>
		/// <param name="text">String to look in.</param>
		/// <param name="str">String to look for.</param>
		/// <param name="ignoreCase">Pass "true" if you need case-insensitive search.
		/// But remember that case-sensitive search is faster.</param>
		/// <returns>Returns the number of found occurrences.</returns>
		/// <example><code>int m = CountMatches("Bot Bot bot", "Bot", false); // =2</code></example>
		public static int CountMatches(string text, string str, bool ignoreCase)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException("text");
			if (string.IsNullOrEmpty(str))
				throw new ArgumentNullException("str");
			int matches = 0;
			int position = 0;
			StringComparison rule = ignoreCase
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;
			while ((position = text.IndexOf(str, position, rule)) != -1) {
				matches++;
				position++;
			}
			return matches;
		}

		/// <summary>This auxiliary function makes the first letter in specified string upper-case.
		/// This is often needed, but strangely there is no such function in .NET Framework's
		/// String class.</summary>
		/// <param name="str">String to capitalize.</param>
		/// <returns>Returns capitalized string.</returns>
		public static string Capitalize(string str)
		{
			return char.ToUpper(str[0]) + str.Substring(1);
		}

		/// <summary>This auxiliary function makes the first letter in specified string lower-case.
		/// This is often needed, but strangely there is no such function in .NET Framework's
		/// String class.</summary>
		/// <param name="str">String to uncapitalize.</param>
		/// <returns>Returns uncapitalized string.</returns>
		public static string Uncapitalize(string str)
		{
			return char.ToLower(str[0]) + str.Substring(1);
		}

		/// <summary>Suspends execution for specified number of seconds.</summary>
		/// <param name="seconds">Number of seconds to wait.</param>
		public static void Wait(int seconds)
		{
			Thread.Sleep(seconds * 1000);
		}

		/// <summary>This internal function switches unsafe HTTP headers parsing on or off.
		/// This is needed to ignore unimportant HTTP protocol violations,
		/// committed by misconfigured web servers.</summary>
		public static void SwitchUnsafeHttpHeaderParsing(bool enabled)
		{
			System.Configuration.Configuration config =
				System.Configuration.ConfigurationManager.OpenExeConfiguration(
					System.Configuration.ConfigurationUserLevel.None);
			System.Net.Configuration.SettingsSection section =
				(System.Net.Configuration.SettingsSection)config.GetSection("system.net/settings");
			if (unsafeHttpHeaderParsingUsed == 2)
				unsafeHttpHeaderParsingUsed = section.HttpWebRequest.UseUnsafeHeaderParsing ? 1 : 0;
			section.HttpWebRequest.UseUnsafeHeaderParsing = enabled;
			config.Save();
			System.Configuration.ConfigurationManager.RefreshSection("system.net/settings");
		}

		/// <summary>This internal function initializes web client to get resources
		/// from web.</summary>
		public static void InitWebClient()
		{
			wc.UseDefaultCredentials = true;
			wc.Encoding = Encoding.UTF8;
			wc.Headers.Add("Content-Type", webContentType);
			wc.Headers.Add("User-agent", botVer);
		}

		/// <summary>This internal wrapper function gets web resource in a fault-tolerant manner.
		/// It should be used only in simple cases, because it sends no cookies, it doesn't support
		/// traffic compression and lacks other special features.</summary>
		/// <param name="address">Web resource address.</param>
		/// <param name="postData">Data to post with web request, can be "" or null.</param>
		/// <returns>Returns web resource as text.</returns>
		public static string GetWebResource(Uri address, string postData)
		{
			string webResourceText = null;
			for (int errorCounter = 0; errorCounter <= retryTimes; errorCounter++) {
				try {
					Bot.InitWebClient();
					if (string.IsNullOrEmpty(postData))
						webResourceText = Bot.wc.DownloadString(address);
					else
						webResourceText = Bot.wc.UploadString(address, postData);
					break;
				}
				catch (WebException e) {
					if (errorCounter > retryTimes)
						throw;
					string message = isRunningOnMono ? e.InnerException.Message : e.Message;
					if (Regex.IsMatch(message, ": \\(50[02349]\\) ")) {		// Remote problem
						Console.Error.WriteLine(Msg("{0} Retrying in 60 seconds."), message);
						Thread.Sleep(60000);
					}
					else if (message.Contains("Section=ResponseStatusLine")) {	// Squid problem
						SwitchUnsafeHttpHeaderParsing(true);
						Console.Error.WriteLine(Msg("{0} Retrying in 60 seconds."), message);
						Thread.Sleep(60000);
					}
					else
						throw;
				}
			}
			return webResourceText;
		}
	}
}