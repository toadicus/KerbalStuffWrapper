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
	/// Class representing a KerbalStuff user as presented by the KerbalStuff API.
	/// </summary>
	public class User
	{
		/// <summary>
		/// The user's KerbalStuff username.
		/// </summary>
		public string Username
		{
			get;
			private set;
		}

		/// <summary>
		/// The user's Twitter username.
		/// </summary>
		public string TwitterUsername
		{
			get;
			private set;
		}

		/// <summary>
		/// A read-only list of <see cref="KerbalStuff.Mod"/> objects maintained by the user.
		/// </summary>
		public IList<Mod> Mods
		{
			get
			{
				return (this.mods == null) ? null : this.mods.AsReadOnly();
			}
		}

		/// <summary>
		/// The user's Reddit username.
		/// </summary>
		public string RedditUsername
		{
			get;
			private set;
		}

		/// <summary>
		/// The user's IRC nickname.
		/// </summary>
		public string IrcNick
		{
			get;
			private set;
		}

		/// <summary>
		/// The user's profile description.
		/// </summary>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		/// The user's KSP Forum username.
		/// </summary>
		public string ForumUsername
		{
			get;
			private set;
		}

		private List<Mod> mods;

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.User"/> class from a dictionary of JSON objects.
		/// </summary>
		/// <param name="jsonDict">Dictionary containing the deserialized JSON response from KerbalStuff.</param>
		public User(Dictionary<string, object> jsonDict) : this()
		{
			this.Username = (string)jsonDict["username"];
			this.TwitterUsername = (string)jsonDict["twitterUsername"];
			this.RedditUsername = (string)jsonDict["redditUsername"];
			this.IrcNick = (string)jsonDict["ircNick"];
			this.ForumUsername = (string)jsonDict["forumUsername"];

			this.Description = (string)jsonDict["description"];

			this.mods = new List<Mod>();

			foreach (object modObj in (jsonDict["mods"] as List<object>))
			{
				this.mods.Add(new Mod(modObj as Dictionary<string, object>));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KerbalStuff.User"/> class from an ambiguously-typed dictionary
		/// of JSON objects.
		/// </summary>
		/// <param name="jsonObj">Dictionary containing the JSON response from KerbalStuff.</param>
		public User(object jsonObj) : this((Dictionary<string, object>)jsonObj) {}

		private User() {}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="KerbalStuff.User"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="KerbalStuff.User"/>.</returns>
		public override string ToString()
		{
			return string.Format(
				"User: username={0}, twitterUsername={1}, redditUsername={3}, ircNick={4}, description={5}, forumUsername={6}\nmods:\n{2}",
				Username,
				TwitterUsername,
				string.Join(
					"\n",
					Mods.Select(m => m.ToString()).ToArray()
				),
				RedditUsername,
				IrcNick,
				Description,
				ForumUsername
			);
		}
	}
}