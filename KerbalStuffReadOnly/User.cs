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

using System;
using System.Collections.Generic;
using System.Linq;

namespace KerbalStuff
{
	public class User
	{
		public string username
		{
			get;
			private set;
		}

		public string twitterUsername
		{
			get;
			private set;
		}

		public List<Mod> mods
		{
			get;
			private set;
		}

		public string redditUsername
		{
			get;
			private set;
		}

		public string ircNick
		{
			get;
			private set;
		}

		public string description
		{
			get;
			private set;
		}

		public string forumUsername
		{
			get;
			private set;
		}

		public User(Dictionary<string, object> jsonDict) : this()
		{
			this.username = (string)jsonDict["username"];
			this.twitterUsername = (string)jsonDict["twitterUsername"];
			this.redditUsername = (string)jsonDict["redditUsername"];
			this.ircNick = (string)jsonDict["ircNick"];
			this.forumUsername = (string)jsonDict["forumUsername"];

			this.description = (string)jsonDict["description"];

			this.mods = new List<Mod>();

			foreach (object modObj in (jsonDict["mods"] as List<object>))
			{
				this.mods.Add(new Mod(modObj as Dictionary<string, object>));
			}
		}

		public User(object jsonObj) : this((Dictionary<string, object>)jsonObj) {}

		private User() {}

		public override string ToString()
		{
			return string.Format(
				"User: username={0}, twitterUsername={1}, redditUsername={3}, ircNick={4}, description={5}, forumUsername={6}\nmods:\n{2}",
				username,
				twitterUsername,
				string.Join(
					"\n",
					mods.Select(m => m.ToString()).ToArray()
				),
				redditUsername,
				ircNick,
				description,
				forumUsername
			);
		}
	}
}