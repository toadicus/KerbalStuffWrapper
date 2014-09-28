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
// KerbalStuff is copyright © 2014 Drew DeVault.  Used under license.
//

/*
 * PATH NOTE:
 * 
 * All partial URI path string should end without a slash, and begin with a slash or protocol identifier.
 * 
 * GOOD: "https://kerbalstuff.com", "/api", "/mod/{0:s}"
 * 
 * BAD:  "kerbalstuff.com/", "api", "mod/{0:s}"
 * */

using MiniJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KerbalStuff
{
	/// <summary>
	/// <para>Class of static methods for accessing the read-only elements of the KerbalStuff API.</para>
	/// <para>https://github.com/KerbalStuff/KerbalStuff/blob/master/api.md</para>
	/// </summary>
	public class KerbalStuffReadOnly
	{
		/// <summary>
		/// The root URI of the KerbalStuff website, including protocol.
		/// </summary>
		public const string RootUri = "https://kerbalstuff.com";

		/// <summary>
		/// The URI of the KerbalStuff API, relative to the KerbalStuff root.
		/// </summary>
		/// <seealso cref="KerbalStuffReadOnly.RootUri"/>
		public const string APIUri = RootUri + "/api";

		private const string UserAgent = "KerbalStuffWrapper by toadicus";

		/// <summary>
		/// The response received from KerbalStuff after the current request.  Reset to null at the beginning of each
		/// new request.
		/// </summary>
		public static HttpWebResponse currentResponse
		{
			get;
			protected set;
		}

		/// <summary>
		/// The List or Dictionary of deserialized JSON objects received from KerbalStuff after the current request.
		/// Reset to null at the beginning of each new request.
		/// </summary>
		/// <value>The current json.</value>
		public static object currentJson
		{
			get;
			protected set;
		}

		/// <summary>
		/// Queries KerbalStuff for the mod with the given Id, returning a <see cref="KerbalStuff.Mod"/> object,
		/// or null if an error occured.
		/// </summary>
		/// <param name="modId">The Id of Mod to be queried on KerbalStuff.</param>
		public static Mod ModInfo(long modId)
		{
			ExecuteGetRequest(KerbalStuffAction.ModInfo, modId);

			Mod mod = null;

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				mod = new Mod(currentJson as Dictionary<string, object>);
			}

			return mod;
		}

		/// <summary>
		/// Queries KerbalStuff for the latest version of the mod with then given Id, returning a
		/// <see cref="KerbalStuff.ModVersion"/> object, or null if an error occured.
		/// </summary>
		/// <param name="modId">The Id of the Mod to be queried on KerbalStuff.</param>
		public static ModVersion ModLatest(long modId)
		{
			ExecuteGetRequest(KerbalStuffAction.ModLatest, modId);

			ModVersion ver = null;

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				ver = new ModVersion(currentJson as Dictionary<string, object>);
			}

			return ver;
		}

		/// <summary>
		/// Searches KerbalStuff for a mod containing the given query string, returning a List of
		/// <see cref="KerbalStuff.Mod"/> objects, or an empty List if none are found or an error occurs.
		/// </summary>
		/// <param name="query">The search query</param>
		public static List<Mod> ModSearch(string query)
		{
			ExecuteGetRequest(KerbalStuffAction.ModSearch, query);

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

		/// <summary>
		/// Queries KerbalStuff for a user with the given username, returning a <see cref="KerbalStuff.User"/> object,
		/// or null if an error occurs.
		/// </summary>
		/// <param name="username">The exact, case-sensitive username to query.</param>
		public static User UserInfo(string username)
		{
			ExecuteGetRequest(KerbalStuffAction.UserInfo, username);


			User user = null;

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				user = new User(currentJson);
			}

			return user;
		}

		/// <summary>
		/// Searches KerbalStuff for a user containing the query string, returning a List of
		/// <see cref="KerbalStuff.User"/> objects, or an empty List if none are found or an error occurs.
		/// </summary>
		/// <returns>The search.</returns>
		/// <param name="query">Query.</param>
		public static List<User> UserSearch(string query)
		{
			ExecuteGetRequest(KerbalStuffAction.UserSearch, query);

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

		/// <summary>
		/// The current HTTP request delivered in an API action.  Reset to null at the beginning of each new action.
		/// </summary>
		protected static HttpWebRequest currentRequest;

		/// <summary>
		/// Executes an HTTP GET request using the URI format from the specified
		/// <see cref="KerbalStuff.KerbalStuffAction"/> and objects for inclusion in the formatted URI string.
		/// </summary>
		/// <param name="action">A KerbalStuffAction object describing the desired API action.</param>
		/// <param name="formatArgs">Format arguments</param>
		protected static void ExecuteGetRequest(KerbalStuffAction action, params object[] formatArgs)
		{
			string uri = string.Format(action.UriFormat, formatArgs);

			ExecuteGetRequest(uri);
		}

		/// <summary>
		/// Executes an HTTP GET request to the specified uri.
		/// </summary>
		/// <param name="uri">Absolute URI</param>
		protected static void ExecuteGetRequest(string uri)
		{
			currentJson = null;
			currentRequest = null;
			currentResponse = null;

			if (uri == string.Empty)
			{
				throw new ArgumentOutOfRangeException("KerbalStuffWrapper.ExecuteRequest: uri must not be empty.");
			}

			uri = Uri.EscapeUriString(uri);

			currentRequest = (HttpWebRequest)WebRequest.Create(uri);
			currentRequest.Method = "GET";
			currentRequest.UserAgent = UserAgent;

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

		/// <summary>
		/// Constructor is protected to allow class inheritance, but the class is static and should not be instatiated.
		/// </summary>
		[Obsolete("Do not instantiate KerbalStuffReadOnly objects; all class members are static.")]
		protected KerbalStuffReadOnly() {}
	}

	/// <summary>
	/// Struct describing a KerbalStuff API action.
	/// </summary>
	public struct KerbalStuffAction
	{
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff create API action.
		/// </summary>
		public static readonly KerbalStuffAction Create = new KerbalStuffAction("create", "/mod/create", "POST");
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff login API action.
		/// </summary>
		public static readonly KerbalStuffAction Login = new KerbalStuffAction("login", "/login", "POST");
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff mod/&lt;mod_id&gt; API action.
		/// </summary>
		public static readonly KerbalStuffAction ModInfo = new KerbalStuffAction("modinfo", "/mod/{0:d}", "GET");
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff mod/&lt;mod_id&gt;/latest API action.
		/// </summary>
		public static readonly KerbalStuffAction ModLatest = new KerbalStuffAction(
			"modlatest",
			"/mod/{0:d}/latest",
			"GET"
		);
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff search/mod API action.
		/// </summary>
		public static readonly KerbalStuffAction ModSearch = new KerbalStuffAction(
			"modsearch",
			"/search/mod?query={0}",
			"GET"
		);
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff update API action.
		/// </summary>
		public static readonly KerbalStuffAction Update = new KerbalStuffAction("update", "/mod/{0:d}/update", "POST");
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff user/&lt;username&gt; API action.
		/// </summary>
		public static readonly KerbalStuffAction UserInfo = new KerbalStuffAction("userinfo", "/user/{0}", "GET");
		/// <summary>
		/// KerbalStuffAction object describing access to the KerbalStuff search/user API action.
		/// </summary>
		public static readonly KerbalStuffAction UserSearch = new KerbalStuffAction(
			"usersearch",
			"/search/user?query={0}",
			"GET"
		);

		/// <summary>
		/// The name of the action, currently unused.
		/// </summary>
		public string Action;

		/// <summary>
		/// A format string for generating a URI path relative to the KerbalStuff root URI.
		/// </summary>
		public string UriPathFormat;

		/// <summary>
		/// The HTTP request method, "GET" or "POST".
		/// </summary>
		public string RequestMethod;

		/// <summary>
		/// Read-only access to the absolute URI format string containing the protocol and KerbalStuff root URI.
		/// </summary>
		/// <value>The URI format.</value>
		public string UriFormat
		{
			get
			{
				return string.Format("{0}{1}", KerbalStuffReadOnly.APIUri, this.UriPathFormat);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.KerbalStuffAction"/> struct.
		/// </summary>
		/// <param name="action">The name of the action, currently unused.</param>
		/// <param name="uriPathFormat">A format string for generating a URI path relative to the KerbalStuff root URI.</param>
		/// <param name="requestMethod">The HTTP request method, "GET" or "POST".</param>
		public KerbalStuffAction(string action, string uriPathFormat, string requestMethod) : this()
		{
			requestMethod = requestMethod.ToUpper();

			if (requestMethod != "GET" && requestMethod != "POST")
			{
				throw new ArgumentOutOfRangeException(
					"KerbalStuffAction.ctor: 'requestMethod' must be one of \"GET\" or \"POST\"");
			}

			this.Action = action;
			this.UriPathFormat = uriPathFormat;
			this.RequestMethod = requestMethod;
		}
	}
}