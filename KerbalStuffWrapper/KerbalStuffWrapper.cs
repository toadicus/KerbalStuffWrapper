using MiniJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace KerbalStuffWrapper
{
	public static class KerbalStuffWrapper
	{
		private static HttpWebRequest currentRequest;
		private static HttpWebResponse currentResponse;
		private static CookieContainer cookies;

		private static void ExecuteRequest(string uri, string method, bool assignCookies)
		{
			if (uri == string.Empty)
			{
				throw new ArgumentOutOfRangeException("KerbalStuffWrapper.ExecuteRequest: uri must not be empty.");
			}

			method = method.ToUpper();

			if (method != "POST" && method != "GET")
			{
				throw new ArgumentOutOfRangeException("KerbalStuffWrapper.ExecuteRequest: method must be POST or GET.");
			}

			currentRequest = (HttpWebRequest)WebRequest.Create(uri);
			currentRequest.Method = method;

			if (assignCookies)
			{
				if (cookies == null)
				{
					throw new ArgumentNullException("KerbalStuffWrapper.ExecuteRequest: cookies must not be null.");
				}

				currentRequest.CookieContainer = cookies;
			}


		}

		public static List<Dictionary<string, object>> Search(string query)
		{
			// ServicePointManager.ServerCertificateValidationCallback
			string uri = string.Format(KerbalStuffAction.Search.UriFormat, query);

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
			req.Method = KerbalStuffAction.Search.RequestMethod;

			var responseReader = new StreamReader(req.GetResponse().GetResponseStream());

			string json = responseReader.ReadToEnd();

			Console.WriteLine(json);

			object jsonObj = Json.Deserialize(json);

			return (List<Dictionary<string, object>>)jsonObj;
		}
	}

	public struct KerbalStuffAction
	{
		public static readonly KerbalStuffAction Search = new KerbalStuffAction("search", "search/mod?query=%s", "GET");
		public static readonly KerbalStuffAction ModInfo = new KerbalStuffAction("modinfo", "mod/%d", "GET");

		public const string APIUri = "https://www.kerbalstuff.com/api/";

		public string Action;

		public string UriPathFormat;

		public string RequestMethod;

		public string UriFormat
		{
			get
			{
				return string.Format("{0}{1}", APIUri, this.UriPathFormat);
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

