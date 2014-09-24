using MiniJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KerbalStuff
{
	public static class KerbalStuff
	{
		public const string RootUri = "https://kerbalstuff.com";

		public const string APIUri = RootUri + "/api";

		public const string UserAgent = "KerbalStuffWrapper by toadicus";

		public static void Login(string username, string password)
		{
			Dictionary<string, object> postParams = new Dictionary<string, object>();
			postParams.Add("username", username);
			postParams.Add("password", password);

			HttpWebResponse response = FormUpload.MultipartFormDataPost(
				KerbalStuffAction.Login.UriFormat,
				"KerbalStuffWrapper by toadicus",
				postParams
			);

			cookies = response.Cookies;
		}

		public static string Create(Mod mod, string fileName, string filePath)
		{
			if (mod == null)
			{
				throw new ArgumentNullException("KerbalStuffWrapper.Create: mod argument cannot be null.");
			}
			else
			{
				if (mod.name == string.Empty)
					throw new ArgumentException("mod.name cannot be empty.");
				if (mod.license == string.Empty)
					throw new ArgumentException("mod.license cannot be empty.");
				if (mod.short_description == string.Empty)
					throw new ArgumentException("mod.short_description cannot be empty.");
				if (mod.versions.Count < 1)
					throw new ArgumentException("mod must have a single version to create.");
				else if (mod.versions[0] == null)
				{
					throw new ArgumentNullException("mod.versions[0] cannot be null.");
				}
				else
				{
					if (mod.versions[0].friendly_version == string.Empty)
						throw new ArgumentException("mod.versions[0].friendly_version cannot be empty.");
					if (mod.versions[0].ksp_version == string.Empty)
						throw new ArgumentException("mod.versions[0].ksp_version cannot be empty.");
				}
			}

			if (cookies == null)
			{
				throw new Exception("KerbalStuffWrapper.Create: Must log in first.");
			}

			if (!File.Exists(filePath))
			{
				throw new IOException(string.Format("KerbalStuffWrapper.Create: File '{0}' does not exist.", filePath));
			}

			Dictionary<string, object> postParams = new Dictionary<string, object>();
			postParams.Add("name", mod.name);
			postParams.Add("short-description", mod.short_description);
			postParams.Add("license", mod.license);
			postParams.Add("version", mod.versions[0].friendly_version);
			postParams.Add("ksp-version", mod.versions[0].ksp_version);
			postParams.Add("zipball", ReadZipballParameter(fileName, filePath));

			ExecutePostRequest(KerbalStuffAction.Create.UriFormat, postParams, cookies);

			return string.Concat(RootUri, (currentJson as Dictionary<string, object>)["url"]);
		}

		public static string Update(long modId, ModVersion version, bool notifyFollowers, string fileName, string filePath)
		{
			if (version == null)
			{
				throw new ArgumentNullException("KerbalStuffWrapper.Update: version cannot be null");
			}
			if (version.friendly_version == string.Empty)
				throw new ArgumentException("KerbalStuffWrapper.Update: version.friendly_version cannot be empty");
			if (version.ksp_version == string.Empty)
				throw new ArgumentException("KerbalStuffWrapper.Update: version.ksp_version cannot be empty");

			if (cookies == null)
			{
				throw new Exception("KerbalStuffWrapper.Update: Must log in first.");
			}

			if (!File.Exists(filePath))
			{
				throw new IOException(string.Format("KerbalStuffWrapper.Update: File '{0}' does not exist.", filePath));
			}

			string uri = string.Format(KerbalStuffAction.Update.UriFormat, modId);

			Dictionary<string, object> postParams = new Dictionary<string, object>();
			postParams.Add("version", version.friendly_version);
			postParams.Add("ksp-version", version.ksp_version);

			if (version.changelog != null && version.changelog != string.Empty)
			{
				postParams.Add("changelog", version.changelog);
			}

			postParams.Add("notify-followers", notifyFollowers ? "yes" : "no");

			postParams.Add("zipball", ReadZipballParameter(fileName, filePath));

			ExecutePostRequest(uri, postParams, cookies);

			return string.Concat(RootUri, (currentJson as Dictionary<string, object>)["url"]);
		}

		public static Mod ModInfo(long modId)
		{
			string uri = string.Format(KerbalStuffAction.ModInfo.UriFormat, modId);

			ExecuteGetRequest(uri, KerbalStuffAction.ModInfo.RequestMethod, false);

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

			ExecuteGetRequest(uri, KerbalStuffAction.ModLatest.RequestMethod, false);

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

			ExecuteGetRequest(uri, KerbalStuffAction.ModSearch.RequestMethod, false);

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

		private static HttpWebRequest currentRequest;
		private static HttpWebResponse currentResponse;
		private static CookieCollection cookies;

		private static object currentJson;

		private static void ExecuteGetRequest(KerbalStuffAction action, bool assignCookies, params object[] formatArgs)
		{
			string uri = string.Format(action.UriFormat, formatArgs);

			ExecuteGetRequest(uri, action.RequestMethod, assignCookies);
		}

		private static void ExecuteGetRequest(string uri, string method, bool assignCookies, byte[] formData = null)
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

			Console.WriteLine("Request cookies: " + currentRequest.CookieContainer);

			currentResponse = (HttpWebResponse)currentRequest.GetResponse();

			Console.WriteLine("Response cookies: " + string.Join(
					",",
					currentResponse.Cookies.Cast<Cookie>().Select(c => c.ToString()).ToArray()
				));

			if (currentResponse.StatusCode == HttpStatusCode.NotFound)
			{
				throw new WebException(string.Format("KerbalStuffWrapper.ExecuteRequest: URI not found: {0}", uri));
			}

			if (currentResponse.ContentType == "application/json")
			{
				var responseReader = new StreamReader(currentResponse.GetResponseStream());

				string json = responseReader.ReadToEnd();

				currentJson = Json.Deserialize(json);
			}
		}

		private static void ExecutePostRequest(string uri, Dictionary<string, object> postParams, CookieCollection cookieCollection = null)
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
					/*"http://toad.homelinux.net/post_dump.php",*/
					"KerbalStuffWrapper by toadicus",
					postParams,
					jar
				);

				currentJson = Json.Deserialize((new StreamReader(currentResponse.GetResponseStream())).ReadToEnd());
			}
			catch (WebException ex)
			{
				Console.WriteLine(string.Format("KerbalStuffWrapper.Create: Caught WebException.  Response: {0}", (new StreamReader(ex.Response.GetResponseStream())).ReadToEnd()));
			}
		}

		private static FormUpload.FileParameter ReadZipballParameter(string fileName, string filePath)
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

				byte[] fileBytes = stream.GetBuffer();

				return new FormUpload.FileParameter(fileBytes, fileName, "application/zip");
			}

			return null;
		}
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
				return string.Format("{0}{1}", KerbalStuff.APIUri, this.UriPathFormat);
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

