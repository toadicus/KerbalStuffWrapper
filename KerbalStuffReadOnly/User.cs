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

