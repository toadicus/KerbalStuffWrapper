// KerbalStuffWrapper
//
// Author:
// 	toadicus
//
// Copyright © 2014, toadicus
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
// following conditions are met:
//
// 	* Redistributions of source code must retain the above copyright notice, this list of conditions and the
// 	  following disclaimer.
// 	* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
// 	  following disclaimer in the documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// This software uses the CLAP .NET Command-Line Parser, Copyright © 2011 Adrian Aisemberg, SharpRegion.
// Used under license.
//
// This software uses the MiniJSON .NET JSON Parser, Copyright © 2013 Calvin Rien.  Used under license.
//
// This software uses the FormUpload multipart/form-data library,
// http://www.briangrinstead.com/blog/multipart-form-post-in-c.
//

using MiniJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KerbalStuff
{
	public class KerbalStuffReadOnly
	{
		public const string RootUri = "https://kerbalstuff.com";

		public const string APIUri = RootUri + "/api";

		public const string UserAgent = "KerbalStuffWrapper by toadicus";

		public static Mod ModInfo(long modId)
		{
			string uri = string.Format(KerbalStuffAction.ModInfo.UriFormat, modId);

			ExecuteGetRequest(uri, KerbalStuffAction.ModInfo.RequestMethod);

			Mod mod = null;

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				mod = new Mod(currentJson as Dictionary<string, object>);
			}

			return mod;
		}

		public static ModVersion ModLatest(long modId)
		{
			string uri = string.Format(KerbalStuffAction.ModLatest.UriFormat, modId);

			ExecuteGetRequest(uri, KerbalStuffAction.ModLatest.RequestMethod);

			ModVersion ver = null;

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				ver = new ModVersion(currentJson as Dictionary<string, object>);
			}

			return ver;
		}

		public static List<Mod> ModSearch(string query)
		{
			string uri = string.Format(KerbalStuffAction.ModSearch.UriFormat, query);

			ExecuteGetRequest(uri, KerbalStuffAction.ModSearch.RequestMethod);

			List<Mod> rList = new List<Mod>();

			if (currentJson != null && currentJson is List<object>)
			{
				foreach (var modObj in (currentJson as List<object>))
				{
					if (modObj is Dictionary<string, object>)
					{
						rList.Add(new Mod(modObj as Dictionary<string, object>));
					}
				}
			}

			return rList;
		}

		public static User UserInfo(string username)
		{
			ExecuteGetRequest(KerbalStuffAction.UserInfo, false, username);


			User user = null;

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				user = new User(currentJson);
			}

			return user;
		}

		public static List<User> UserSearch(string query)
		{
			ExecuteGetRequest(KerbalStuffAction.UserSearch, false, query);

			List<User> users = new List<User>();

			if (currentJson != null && currentJson is List<object>)
			{
				foreach (object userObj in (currentJson as List<object>))
				{
					users.Add(new User(userObj));
				}
			}

			return users;
		}

		public static HttpWebResponse currentResponse
		{
			get;
			protected set;
		}

		public static CookieCollection cookies
		{
			get;
			protected set;
		}

		protected static HttpWebRequest currentRequest;

		public static object currentJson
		{
			get;
			protected set;
		}

		protected static void ExecuteGetRequest(KerbalStuffAction action, bool assignCookies, params object[] formatArgs)
		{
			string uri = string.Format(action.UriFormat, formatArgs);

			ExecuteGetRequest(uri, action.RequestMethod);
		}

		protected static void ExecuteGetRequest(string uri, string method)
		{
			currentJson = null;
			currentRequest = null;
			currentResponse = null;

			if (uri == string.Empty)
			{
				throw new ArgumentOutOfRangeException("KerbalStuffWrapper.ExecuteRequest: uri must not be empty.");
			}

			uri = Uri.EscapeUriString(uri);

			method = method.ToUpper();

			if (method != "POST" && method != "GET")
			{
				throw new ArgumentOutOfRangeException("KerbalStuffWrapper.ExecuteRequest: method must be POST or GET.");
			}

			currentRequest = (HttpWebRequest)WebRequest.Create(uri);
			currentRequest.Method = method;

			try
			{
				currentResponse = currentRequest.GetResponse() as HttpWebResponse;
			}
			catch (WebException ex)
			{
				currentResponse = ex.Response as HttpWebResponse;
			}

			if (currentResponse.ContentType == "application/json")
			{
				var responseReader = new StreamReader(currentResponse.GetResponseStream());

				string json = responseReader.ReadToEnd();

				currentJson = Json.Deserialize(json);
			}
		}

		protected KerbalStuffReadOnly() {}
	}

	public struct KerbalStuffAction
	{
		public static readonly KerbalStuffAction Create = new KerbalStuffAction("create", "/mod/create", "POST");
		public static readonly KerbalStuffAction Login = new KerbalStuffAction("login", "/login", "POST");
		public static readonly KerbalStuffAction ModInfo = new KerbalStuffAction("modinfo", "/mod/{0:d}", "GET");
		public static readonly KerbalStuffAction ModLatest = new KerbalStuffAction(
			"modlatest",
			"/mod/{0:d}/latest",
			"GET"
		);
		public static readonly KerbalStuffAction ModSearch = new KerbalStuffAction(
			"modsearch",
			"/search/mod?query={0}",
			"GET"
		);
		public static readonly KerbalStuffAction Update = new KerbalStuffAction("update", "/mod/{0:d}/update", "POST");
		public static readonly KerbalStuffAction UserInfo = new KerbalStuffAction("userinfo", "/user/{0}", "GET");
		public static readonly KerbalStuffAction UserSearch = new KerbalStuffAction(
			"usersearch",
			"/search/user?query={0}",
			"GET"
		);

		public string Action;

		public string UriPathFormat;

		public string RequestMethod;

		public string UriFormat
		{
			get
			{
				return string.Format("{0}{1}", KerbalStuffReadOnly.APIUri, this.UriPathFormat);
			}
		}

		public KerbalStuffAction(string action, string uriFormat, string requestMethod) : this()
		{
			this.Action = action;
			this.UriPathFormat = uriFormat;
			this.RequestMethod = requestMethod;
		}
	}
}