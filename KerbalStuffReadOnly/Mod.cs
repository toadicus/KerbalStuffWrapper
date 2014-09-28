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

using System;
using System.Collections.Generic;
using System.Linq;

namespace KerbalStuff
{
	/// <summary>
	/// Class representing a KerbalStuff Mod as presented by the KerbalStuff API.
	/// </summary>
	public class Mod
	{
		/// <summary>
		/// The number of times all versions of this Mod have been downloaded.
		/// </summary>
		public long Downloads
		{
			get;
			private set;
		}

		/// <summary>
		/// The name of the Mod.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// The number of followers subscribed to this Mod on KerbalStuff.
		/// </summary>
		public long Followers
		{
			get;
			private set;
		}

		/// <summary>
		/// The name of the <see cref="KerbalStuff.User"/> that authors this Mod.
		/// </summary>
		public string Author
		{
			get;
			private set;
		}

		/// <summary>
		/// The ID of the default <see cref="KerbalStuff.ModVersion"/> of this Mod.
		/// </summary>
		public long DefaultVersionId
		{
			get;
			private set;
		}

		/// <summary>
		/// A read-only list of the available versions of this Mod.
		/// </summary>
		/// <seealso cref="KerbalStuff.ModVersion"/>
		public IList<ModVersion> Versions
		{
			get
			{
				return (versions == null) ? null : versions.AsReadOnly();
			}
		}

		/// <summary>
		/// The Id of this Mod on KerbalStuff.
		/// </summary>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		/// The URI of the Mod's background image, relative to the mediacru.sh CDN root.
		/// </summary>
		public string Background
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the vertical offset of the Mod's background image, in pixels.
		/// </summary>
		/// <value>The background vertical offset.</value>
		public long BackgroundVerticalOffset
		{
			get;
			private set;
		}

		/// <summary>
		/// A short (1000 characters or less) description of this Mod.
		/// </summary>
		public string ShortDescription
		{
			get;
			private set;
		}

		/// <summary>
		/// The name or title (128 characters or less) of the License under which this Mod is released.
		/// </summary>
		public string License
		{
			get;
			private set;
		}

		private List<ModVersion> versions;

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.Mod"/> class from a Dictionary of JSON objects.
		/// </summary>
		/// <param name="jsonDict">Dictionary containing the deserialized JSON response from KerbalStuff.</param>
		public Mod(Dictionary<string, object> jsonDict) : this()
		{
			this.Downloads = (long)jsonDict["downloads"];
			this.Name = (string)jsonDict["name"];
			this.Followers = (long)jsonDict["followers"];
			this.Author = (string)jsonDict["author"];
			this.DefaultVersionId = (long)jsonDict["default_version_id"];
			this.Id = (long)jsonDict["id"];
			this.ShortDescription = (string)jsonDict["short_description"];
			this.Background = (string)jsonDict["background"];
			this.BackgroundVerticalOffset = (long)jsonDict["bg_offset_y"];

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

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.Mod"/> class from strings describing the Mod.
		/// Useful for creating new Mods for upload.
		/// </summary>
		/// <seealso cref="KerbalStuff.KerbalStuff.Create"/>
		/// <param name="name">The name of the Mod</param>
		/// <param name="shortDescription">A short (1000 characters or less) description of this Mod.</param>
		/// <param name="version">Version.</param>
		/// <param name="kspVersion">Ksp version.</param>
		/// <param name="license">The name or title (128 characters or less) of the License under which this Mod is released.</param>
		public Mod(string name, string shortDescription, string version, string kspVersion, string license) : this()
		{
			this.Name = name;
			this.ShortDescription = shortDescription;
			this.License = license;

			this.versions.Add(new ModVersion(version, kspVersion));
		}

		private Mod()
		{
			this.versions = new List<ModVersion>();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="KerbalStuff.Mod"/>.
		/// </summary>
		public override string ToString()
		{
			return string.Format(
				"Mod: {1}\n" +
				"id: {6}\n" +
				"author: {3}\n" +
				"downloads: {0}\n" +
				"followers: {2}\n" +
				"short_description: {7}\n" +
				"default_version_id: {4}\n" +
				"versions:\n[\n{5}\n]\n",
				Downloads,
				Name,
				Followers,
				Author,
				DefaultVersionId,
				string.Join("\n", Versions.Select(v => v.ToString()).ToArray()),
				Id,
				ShortDescription
			);
		}
	}

	/// <summary>
	/// Class representing a single version of a KerbalStuff Mod as presented by the KerbalStuff API.
	/// </summary>
	public class ModVersion
	{
		/// <summary>
		/// An optional log describing the changes made in this ModVersion 
		/// </summary>
		public string ChangeLog
		{
			get;
			private set;
		}

		/// <summary>
		/// The primary version of KSP for which this ModVersion was developed.
		/// </summary>
		public string KspVersion
		{
			get;
			private set;
		}

		/// <summary>
		/// The path of the download archive for this ModVersion, relative to the KerbalStuff
		/// root.
		/// </summary>
		/// <seealso cref="KerbalStuff.KerbalStuff.RootUri"/>
		public string DownloadPath
		{
			get;
			private set;
		}

		/// <summary>
		/// The Id of this ModVersion on KerbalStuff.
		/// </summary>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		/// The human-friendly (or not) name or number of this ModVersion.
		/// </summary>
		/// <value>The friendly version.</value>
		public string FriendlyVersion
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.ModVersion"/> class from a Dictionary of JSON
		/// objects.
		/// </summary>
		/// <param name="jsonDict">Dictionary containing the deserialized JSON response from KerbalStuff.</param>
		public ModVersion(Dictionary<string, object> jsonDict) : this()
		{
			this.ChangeLog = (string)jsonDict["changelog"];
			this.KspVersion = (string)jsonDict["ksp_version"];
			this.DownloadPath = (string)jsonDict["download_path"];
			this.Id = (long)jsonDict["id"];
			this.FriendlyVersion = (string)jsonDict["friendly_version"];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.ModVersion"/> class from strings describing the
		/// Version.
		/// </summary>
		/// <param name="version">The human-friendly (or not) name or number of this ModVersion.</param>
		/// <param name="kspVersion">The primary version of KSP for which this ModVersion was developed.</param>
		/// <param name="changeLog">An optional log describing the changes made in this ModVersion </param>
		public ModVersion(string version, string kspVersion, string changeLog) : this(version, kspVersion)
		{
			this.ChangeLog = changeLog;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.ModVersion"/> class from strings describing the
		/// Version.
		/// </summary>
		/// <param name="version">The human-friendly (or not) name or number of this ModVersion.</param>
		/// <param name="kspVersion">The primary version of KSP for which this ModVersion was developed.</param>
		public ModVersion(string version, string kspVersion) : this()
		{
			this.FriendlyVersion = version;
			this.KspVersion = kspVersion;
		}

		private ModVersion() {}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="KerbalStuff.ModVersion"/>.
		/// </summary>
		public override string ToString()
		{
			return string.Format(
				"ModVersion {4}:\nid: {3}\nksp_version: {1}\ndownload_path: {2}\nchangelog: {0}",
				ChangeLog,
				KspVersion,
				DownloadPath,
				Id,
				FriendlyVersion
			);
		}
	}
}