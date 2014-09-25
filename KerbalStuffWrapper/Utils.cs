using CLAP;
using CLAP.Interception;
using CLAP.Validation;
using System;
using System.IO;
using System.Collections.Generic;

namespace KerbalStuff.Wrapper
{
	public static class Utils
	{
		public static string HumanName(this Type type)
		{
			switch (type.Name)
			{
				case "Int64":
				case "Int32":
				case "Int16":
					return "integer";
				case "Single":
				case "Double":
					return "decimal number";
				default:
					return type.Name.ToLower();
			}
		}
	}
}

