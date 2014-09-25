using MiniJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KerbalStuff
{
	public class KerbalStuff : KerbalStuffReadOnly
	{
		public static Dictionary<string, object> Login(string username, string password)
		{
			string uri = KerbalStuffAction.Login.UriFormat;

			Dictionary<string, object> postParams = new Dictionary<string, object>();
			postParams.Add("username", username);
			postParams.Add("password", password);

			ExecutePostRequest(uri, postParams, null);

			if (currentResponse.Cookies.Count > 0)
			{
				cookies = currentResponse.Cookies;
			}

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				return currentJson as Dictionary<string, object>;
			}

			return null;
		}

		public static Dictionary<string, object> Create(Mod mod, string fileName, string filePath)
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

		public static Dictionary<string, object> Update(long modId, ModVersion version, bool notifyFollowers, string fileName, string filePath)
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

			if (currentJson != null && currentJson is Dictionary<string, object>)
			{
				return currentJson as Dictionary<string, object>;
			}

			return null;
		}

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

				byte[] fileBytes = stream.GetBuffer();

				return new FormUpload.FileParameter(fileBytes, fileName, "application/zip");
			}
		}

		protected KerbalStuff() {}
	}
}

