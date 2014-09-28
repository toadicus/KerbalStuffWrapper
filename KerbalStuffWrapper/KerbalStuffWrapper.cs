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
// 	* Neither the name of the author nor the names of other contributors may be used to endorse or promote products
// 	  derived from this software without specific prior written permission.
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

using CLAP;
using CLAP.Interception;
using CLAP.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// TODO: Write a console program that uses the KerbalStuff wrapper API.

namespace KerbalStuff.Wrapper
{
	public class KerbalStuffWrapper
	{
		private static Parser<KerbalStuffWrapper> parser;
		private static string[] args;

		public static int Main(string[] args)
		{
			KerbalStuffWrapper.args = args;
			parser = new Parser<KerbalStuffWrapper>();

			return parser.RunStatic(args);
		}

		private static void Write(string message, TextWriter output)
		{

			output.Write(string.Format("{0}: {1}", appName, message));
		}

		private static void WriteOut(string message)
		{
			Write(message, Console.Out);
		}

		private static void WriteOut(string format, params object[] args)
		{
			WriteOut(string.Format(format, args));
		}

		private static void WriteOutLine(string format, params object[] args)
		{
			WriteOut(format, args);
			Console.Out.WriteLine();
		}

		private static void WriteError(string message)
		{
			Write(message, Console.Error);
		}

		private static void WriteError(string format, params object[] args)
		{
			WriteError(string.Format(format, args));
		}

		private static void WriteErrorLine(string format, params object[] args)
		{
			WriteError(format, args);
			Console.Error.WriteLine();
		}

		private const string appName = "KerbalStuffWrapper";

		// TODO: Action methods.

		[Verb(Aliases = "mod,m")]
		public static void ModInfo([Required]long modId)
		{
			Mod mod = KerbalStuff.ModInfo(modId);

			if (mod == null)
			{
				WriteErrorLine("Couldn't get Mod info for mod {0}: {1}.", modId, KerbalStuff.currentResponse.StatusDescription);
			}
			else
			{
				Console.WriteLine(mod);
			}
		}

		[Verb(Aliases = "latest,l")]
		public static void ModLatest([Required]long modId)
		{
			ModVersion ver = KerbalStuff.ModLatest(modId);

			if (ver == null)
			{
				WriteErrorLine("Couldn't get version info for mod {0}: {1}.", modId, KerbalStuff.currentResponse.StatusDescription);
			}
			else
			{
				Console.WriteLine(ver);
			}
		}

		[Verb(Aliases = "search,s")]
		public static void ModSearch([Required]string query)
		{
			List<Mod> mods = KerbalStuff.ModSearch(query);

			if (mods.Count < 1)
			{
				WriteOutLine("Query yielded no results.");
			}
			else
			{
				foreach (Mod mod in mods)
				{
					Console.WriteLine(mod);
					Console.WriteLine();
				}
			}
		}

		[Verb(Aliases = "user,u")]
		public static void UserInfo([Required]string username)
		{
			User user = KerbalStuff.UserInfo(username);

			if (user == null)
			{
				WriteErrorLine("Couldn't get user info for username '{0}': {1}", username, KerbalStuff.currentResponse.StatusDescription);
			}
			else
			{
				Console.WriteLine(user);
			}
		}

		[Verb(Aliases = "us")]
		public static void UserSearch([Required]string query)
		{
			List<User> users = KerbalStuff.UserSearch(query);

			if (users.Count < 1)
			{
				WriteOutLine("Query yielded no results.");
			}
			else
			{
				foreach (User user in users)
				{
					Console.WriteLine(user);
					Console.WriteLine();
				}
			}
		}

		[Verb(Aliases = "create,c")]
		public static void CreateMod(
			[Required]
			[Aliases("user,u")]
			string username,
			[Required]
			[Aliases("pass,p")]
			string password,
			[Required]
			[Aliases("n")]
			string name,
			[Required]
			[Aliases("desc,d")]
			string short_description,
			[Required]
			[Aliases("ver,v")]
			string version,
			[Required]
			[Aliases("ksp,k")]
			string ksp_version,
			[Required]
			[Aliases("lic,l")]
			string license,
			[Required]
			[Aliases("file,f")]
			string filePath
		)
		{
			Mod mod = new Mod(name, short_description, version, ksp_version, license);

			var loginDict = KerbalStuff.Login(username, password);

			if (loginDict == null)
			{
				WriteErrorLine("Could not complete login attempt: {0}", KerbalStuff.currentResponse.StatusDescription);
			}
			else if (loginDict.ContainsKey("error") && loginDict["error"].ToString().ToLower() == "true")
			{
				WriteErrorLine("Login failed: {0}", loginDict["reason"]);
			}
			else
			{
				var createDict = KerbalStuff.Create(mod, Path.GetFileName(filePath), filePath);

				if (createDict == null)
				{
					WriteErrorLine("Could not complete creation attempt: {0}", KerbalStuff.currentResponse.StatusDescription);
				}
				else if (createDict.ContainsKey("error") && createDict["error"].ToString().ToLower() == "true")
				{
					WriteErrorLine("Creation failed: {0}", createDict["message"]);
				}
				else
				{
					WriteOutLine("New mod '{0}' created with id #{2}!  You can view and publish the mod at {1}",
						createDict["name"],
						string.Concat(KerbalStuff.RootUri, createDict["url"]),
						createDict["id"]
					);
				}
			}
		}

		[Verb(Aliases = "update,up")]
		public static void UpdateMod(
			[Required]
			[Aliases("user,u")]
			string username,
			[Required]
			[Aliases("pass,p")]
			string password,
			[Required]
			[Aliases("mod,m")]
			long modId,
			[Required]
			[Aliases("ver,v")]
			string version,
			[Required]
			[Aliases("ksp,k")]
			string ksp_version,
			[Aliases("log,l")]
			string changelog,
			[DefaultValue(false)]
			[Aliases("notify,n")]
			bool notifyFollowers,
			[Required]
			[Aliases("file,f")]
			string filePath
		)
		{
			ModVersion ver;
			if (changelog != string.Empty)
			{
				ver = new ModVersion(version, ksp_version, changelog);
			}
			else
			{
				ver = new ModVersion(version, ksp_version);
			}

			var loginDict = KerbalStuff.Login(username, password);

			if (loginDict == null)
			{
				WriteErrorLine("Could not complete login attempt: {0}", KerbalStuff.currentResponse.StatusDescription);
			}
			else if (loginDict.ContainsKey("error") && loginDict["error"].ToString().ToLower() == "true")
			{
				WriteErrorLine("Login failed: {0}", loginDict["reason"]);
			}
			else
			{
				var updateDict = KerbalStuff.Update(modId, ver, notifyFollowers, Path.GetFileName(filePath), filePath);

				if (updateDict == null)
				{
					WriteErrorLine("Could not complete creation attempt: {0}", KerbalStuff.currentResponse.StatusDescription);
				}
				else if (updateDict.ContainsKey("error") && updateDict["error"].ToString().ToLower() == "true")
				{
					WriteErrorLine("Creation failed: {0}", updateDict["message"]);
				}
				else
				{
					WriteOutLine("Mod #{0}!  You can view the update at {1}",
						updateDict["id"],
						string.Concat(KerbalStuff.RootUri, updateDict["url"])
					);
				}
			}
		}
			
		[Empty, Help, Global]
		public static void Help(string help)
		{
			Console.Error.WriteLine(string.Format("Usage: {0} <ACTION> /OPTION[:ARG] [/OPTION[:ARG]...]", appName));
			Console.Error.WriteLine("\nActions:");
			Console.Error.WriteLine(parser.GetHelpString());
		}

		[Error]
		public static void HandleError(ExceptionContext context)
		{
			if (context.Exception is VerbNotFoundException)
			{
				var ex = context.Exception as VerbNotFoundException;
				WriteErrorLine("Action '{0}' does not exist.", ex.Verb);
			}
			else if (context.Exception is MissingDefaultVerbException)
			{
				WriteErrorLine("An action is required.");
			}
			else if (context.Exception is UnhandledParametersException)
			{
				var ex = context.Exception as UnhandledParametersException;
				var keys = ex.UnhandledParameters.Keys;

				WriteError("invalid option");

				if (keys.Count > 1)
				{
					Console.Error.Write("s");
				}

				Console.Error.Write(string.Format(" for action {0}: '{1}'", args[0], string.Join(", ", keys.ToArray())));
			}
			else if (context.Exception is MissingArgumentPrefixException)
			{
				var ex = context.Exception as MissingArgumentPrefixException;

				WriteErrorLine("{0}: option {1}", args[0], ex.Message);
			}
			else if (context.Exception is MissingArgumentValueException)
			{
				var ex = context.Exception as MissingArgumentValueException;

				WriteErrorLine("{0}: option '{1}' requires an argument.", args[0], ex.ParameterName);
			}
			else if (context.Exception is TypeConvertionException)
			{
				var ex = context.Exception as TypeConvertionException;
				WriteErrorLine("Invalid argument for {2}: '{0}' cannot be converted to {1}.\n", ex.Value, ex.Type.HumanName(), args[0]);
				Help(string.Empty);
			}
			else if (context.Exception is CommandLineParserException)
			{
				WriteErrorLine(context.Exception.Message);
			}
			else
			{
				WriteErrorLine("An unexpected error occured.  {0}: {1}", context.Exception.GetType().Name, context.Exception.Message);
				Console.Error.WriteLine();
				WriteErrorLine(context.Exception.StackTrace);

				Console.Error.WriteLine();
				Console.Error.WriteLine("Response:");
				Console.Error.WriteLine();

				if (KerbalStuff.currentResponse != null)
				{
					Console.Error.WriteLine((new StreamReader(KerbalStuff.currentResponse.GetResponseStream())).ReadToEnd());
				}

				if (KerbalStuff.currentJson != null)
				{
					if (KerbalStuff.currentJson is Dictionary<string, object>)
					{
						var json = KerbalStuff.currentJson as Dictionary<string, object>;
						foreach (KeyValuePair<string, object> item in json)
						{
							Console.Error.WriteLine(string.Format("{0}: {1}", item.Key, item.Value));
						}
					}
				}

				return;
			}

			Console.Error.WriteLine();

			Help(string.Empty);
		}
	}
}

