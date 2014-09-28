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
	/// <para>Class of static methods for accessing the read-write elements of the KerbalStuff API.</para>
	/// <para>https://github.com/KerbalStuff/KerbalStuff/blob/master/api.md</para>
	/// </summary>
	public class KerbalStuff : KerbalStuffReadOnly
	{

		/// <summary>
		/// The cookies returned by KerbalStuff after a Login request, and assigned to successive Create and Update
		/// requests.
		/// </summary>
		public static CookieCollection Cookies
		{
			get;
			protected set;
		}

		/// <summary>
		/// <para>Performs a Login request to KerbalStuff with the given username and password, returning a Dictionary of
		/// deserialized JSON objects received from KerbalStuff after the request, or null if an error occurs.</para>
		/// <para>Sets KerbalStuff.Cookies on success.</para>
		/// </summary>
		/// <param name="username">A valid KerbalStuff username, exact and case-sensitive.</param>
		/// <param name="password">A valid KerbalStuff password associated with the username, exact and case-sensitive.</param>
		public static Dictionary<string, object> Login(string username, string password)
		{
			string uri = KerbalStuffAction.Login.UriFormat;

			Dictionary<string, object> postParams = new Dictionary<string, object>();
			postParams.Add("username", username);
			postParams.Add("password", password);

			ExecutePostRequest(uri, postParams, null);

			if (currentResponse.Cookies.Count > 0)
			{
				Cookies = currentResponse.Cookies;
			}

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				return currentJson as Dictionary<string, object>;
			}

			return null;
		}

		/// <summary>
		/// <para>Performs a creation request to KerbalStuff, creating a new mod described by the given
		/// <see cref="KerbalStuff.Mod"/> object and uploading the file with the given name and path.</para>
		/// <para>Returns a Dictionary of deserialized JSON objects received from KerbalStuff after the request, or null
		/// if an error occurs.</para>
		/// </summary>
		/// <param name="mod">The Mod to be created on KerbalStuff</param>
		/// <param name="fileName">The name of the file to be uploaded</param>
		/// <param name="filePath">The program-relative path of the file to be uploaded</param>
		public static Dictionary<string, object> Create(Mod mod, string fileName, string filePath)
		{
			if (mod == null)
			{
				throw new ArgumentNullException("KerbalStuffWrapper.Create: mod argument cannot be null.");
			}
			else
			{
				if (mod.Name == string.Empty)
					throw new ArgumentException("mod.name cannot be empty.");
				if (mod.License == string.Empty)
					throw new ArgumentException("mod.license cannot be empty.");
				if (mod.ShortDescription == string.Empty)
					throw new ArgumentException("mod.short_description cannot be empty.");
				if (mod.Versions.Count < 1)
					throw new ArgumentException("mod must have a single version to create.");
				else if (mod.Versions[0] == null)
				{
					throw new ArgumentNullException("mod.versions[0] cannot be null.");
				}
				else
				{
					if (mod.Versions[0].FriendlyVersion == string.Empty)
						throw new ArgumentException("mod.versions[0].friendly_version cannot be empty.");
					if (mod.Versions[0].KspVersion == string.Empty)
						throw new ArgumentException("mod.versions[0].ksp_version cannot be empty.");
				}
			}

			if (Cookies == null)
			{
				throw new Exception("KerbalStuffWrapper.Create: Must log in first.");
			}

			if (!File.Exists(filePath))
			{
				throw new IOException(string.Format("KerbalStuffWrapper.Create: File '{0}' does not exist.", filePath));
			}

			Dictionary<string, object> postParams = new Dictionary<string, object>();
			postParams.Add("name", mod.Name);
			postParams.Add("short-description", mod.ShortDescription);
			postParams.Add("license", mod.License);
			postParams.Add("version", mod.Versions[0].FriendlyVersion);
			postParams.Add("ksp-version", mod.Versions[0].KspVersion);
			postParams.Add("zipball", ReadZipballParameter(fileName, filePath));

			ExecutePostRequest(KerbalStuffAction.Create.UriFormat, postParams, Cookies);

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				var rval = currentJson as Dictionary<string, object>;

				if (rval.ContainsKey("reason"))
				{
					rval["message"] = rval["reason"];
				}

				return rval;
			}

			return null;
		}

		/// <summary>
		/// <para>Performs and Update request to KerbalStuff, uploading a new version described in the given
		/// <see cref="KerbalStuff.ModVersion"/> of the mod with the given Id, uploading the file with the given name
		/// and path.  KerbalStuff will notify followers of the mod if requested.</para>
		/// <para>Returns a Dictionary of deserialized JSON objects received from KerbalStuff after the request, or null
		/// if an error occurs.</para>
		/// </summary>
		/// <param name="modId">The Id of Mod to be updated on KerbalStuff.</param>
		/// <param name="version">The ModVersion to be added to the Mod.</param>
		/// <param name="notifyFollowers">If set to <c>true</c> KerbalStuff will notify followers of the mod.</param>
		/// <param name="fileName">The name of the file to be uploaded</param>
		/// <param name="filePath">The program-relative path of the file to be uploaded</param>
		public static Dictionary<string, object> Update(long modId, ModVersion version, bool notifyFollowers, string fileName, string filePath)
		{
			if (version == null)
			{
				throw new ArgumentNullException("KerbalStuffWrapper.Update: version cannot be null");
			}
			if (version.FriendlyVersion == string.Empty)
				throw new ArgumentException("KerbalStuffWrapper.Update: version.friendly_version cannot be empty");
			if (version.KspVersion == string.Empty)
				throw new ArgumentException("KerbalStuffWrapper.Update: version.ksp_version cannot be empty");

			if (Cookies == null)
			{
				throw new Exception("KerbalStuffWrapper.Update: Must log in first.");
			}

			if (!File.Exists(filePath))
			{
				throw new IOException(string.Format("KerbalStuffWrapper.Update: File '{0}' does not exist.", filePath));
			}

			string uri = string.Format(KerbalStuffAction.Update.UriFormat, modId);

			Dictionary<string, object> postParams = new Dictionary<string, object>();
			postParams.Add("version", version.FriendlyVersion);
			postParams.Add("ksp-version", version.KspVersion);

			if (version.ChangeLog != null && version.ChangeLog != string.Empty)
			{
				postParams.Add("changelog", version.ChangeLog);
			}

			postParams.Add("notify-followers", notifyFollowers ? "yes" : "no");

			postParams.Add("zipball", ReadZipballParameter(fileName, filePath));

			ExecutePostRequest(uri, postParams, Cookies);

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				return currentJson as Dictionary<string, object>;
			}

			return null;
		}

		/// <summary>
		/// Executes an HTTP post request to the given URI, including the given Dictionary of post parameters as
		/// multipart/form-data.  If cookieCollection is not null, includes the given cookies as a part of the request.
		/// <seealso cref="FormUpload"/>
		/// </summary>
		/// <param name="uri">Absolute URI</param>
		/// <param name="postParams">String-keyed dictionary of objects to be serialized into multipart/form-data</param>
		/// <param name="cookieCollection">Optional, cookies to be included with the request.</param>
		protected static void ExecutePostRequest(string uri, Dictionary<string, object> postParams, CookieCollection cookieCollection = null)
		{
			currentJson = null;
			currentRequest = null;
			currentResponse = null;

			CookieContainer jar = new CookieContainer();

			if (cookieCollection != null)
			{
				jar.Add(cookieCollection);
			}

			try
			{
				currentResponse = FormUpload.MultipartFormDataPost(
					uri,
					"KerbalStuffWrapper by toadicus",
					postParams,
					jar
				);
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
		/// Reads in the file at the given path and generates a FormUpload.FileParameter object for use in postParam
		/// Dictionaries.
		/// </summary>
		/// <returns>The zipball parameter.</returns>
		/// <param name="fileName">Name of the file to be read</param>
		/// <param name="filePath">Program-relative path of the file to be read</param>
		protected static FormUpload.FileParameter ReadZipballParameter(string fileName, string filePath)
		{
			using (FileStream file = File.OpenRead(filePath))
			{
				byte[] buffer = new byte[1 << 16];
				int bytesRead;

				MemoryStream stream = new MemoryStream();

				while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
				{
					stream.Write(buffer, 0, bytesRead);
				}

				stream.Seek(0, SeekOrigin.Begin);

				byte[] fileBytes = new byte[stream.Length];

				bytesRead = stream.Read(fileBytes, 0, (int)stream.Length);

				return new FormUpload.FileParameter(fileBytes, fileName, "application/zip");
			}
		}

		/// <summary>
		/// Constructor is protected to allow class inheritance, but the class is static and should not be instatiated.
		/// </summary>
		[Obsolete("Do not instantiate KerbalStuff objects; all class members are static.")]
		protected KerbalStuff() {}
	}
}