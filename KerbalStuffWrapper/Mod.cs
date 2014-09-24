using System;
using System.Collections.Generic;
using System.Linq;

namespace KerbalStuff
{
	public class Mod
	{
		public long downloads
		{
			get;
			private set;
		}

		public string name
		{
			get;
			private set;
		}

		public long followers
		{
			get;
			private set;
		}

		public string author
		{
			get;
			private set;
		}

		public long default_version_id
		{
			get;
			private set;
		}

		public List<ModVersion> versions
		{
			get;
			private set;
		}

		public long id
		{
			get;
			private set;
		}

		public string short_description
		{
			get;
			private set;
		}

		public string license
		{
			get;
			private set;
		}

		public Mod(Dictionary<string, object> jsonDict) : this()
		{
			this.downloads = (long)jsonDict["downloads"];
			this.name = (string)jsonDict["name"];
			this.followers = (long)jsonDict["followers"];
			this.author = (string)jsonDict["author"];
			this.default_version_id = (long)jsonDict["default_version_id"];
			this.id = (long)jsonDict["id"];
			this.short_description = (string)jsonDict["short_description"];

			if (jsonDict.ContainsKey("versions"))
			{
				foreach (var ver in (jsonDict["versions"] as List<object>))
				{
					if (ver is Dictionary<string, object>)
					{
						this.versions.Add(new ModVersion(ver as Dictionary<string, object>));
					}
				}
			}
		}

		public Mod(string name, string short_description, string version, string ksp_version, string license) : this()
		{
			this.name = name;
			this.short_description = short_description;
			this.license = license;

			this.versions.Add(new ModVersion(version, ksp_version));
		}

		private Mod()
		{
			this.versions = new List<ModVersion>();
		}

		public override string ToString()
		{
			return string.Format("Mod: {1}\nid: {6}\nauthor: {3}\ndownloads: {0}\nfollowers: {2}\nshort_description: {7}\ndefault_version_id: {4}\nversions:\n[\n{5}\n]\n", downloads, name, followers, author, default_version_id, string.Join("\n", versions.Select(v => v.ToString()).ToArray()), id, short_description);
		}
	}

	public class ModVersion
	{
		public string changelog
		{
			get;
			private set;
		}

		public string ksp_version
		{
			get;
			private set;
		}

		public string download_path
		{
			get;
			private set;
		}

		public long id
		{
			get;
			private set;
		}

		public string friendly_version
		{
			get;
			private set;
		}

		public ModVersion(Dictionary<string, object> jsonDict) : this()
		{
			this.changelog = (string)jsonDict["changelog"];
			this.ksp_version = (string)jsonDict["ksp_version"];
			this.download_path = (string)jsonDict["download_path"];
			this.id = (long)jsonDict["id"];
			this.friendly_version = (string)jsonDict["friendly_version"];
		}

		public ModVersion(string version, string ksp_version, string changelog) : this(version, ksp_version)
		{
			this.changelog = changelog;
		}

		public ModVersion(string version, string ksp_version) : this()
		{
			this.friendly_version = version;
			this.ksp_version = ksp_version;
		}

		private ModVersion() {}

		public override string ToString()
		{
			return string.Format("ModVersion {4}:\nid: {3}\nksp_version: {1}\ndownload_path: {2}\nchangelog: {0}", changelog, ksp_version, download_path, id, friendly_version);
		}
	}
}

