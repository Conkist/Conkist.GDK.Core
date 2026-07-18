using System;

namespace Conkist.GDK.Utils
{
	[AttributeUsage(AttributeTargets.Class)]
	public class RequireTagAttribute : Attribute
	{
		public string Tag;

		public RequireTagAttribute(string tag)
		{
			Tag = tag;
		}
	}
}