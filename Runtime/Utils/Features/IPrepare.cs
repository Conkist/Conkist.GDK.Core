using System;

namespace Conkist.GDK.Utils
{
	/// <summary>
	/// Prepare() called on every MonoBehaviour by IPrepareFeature class. If Prepare() returns true, parent scene will be marked dirty 
	/// </summary>
	public interface IPrepare
	{
		bool Prepare();
	}
}
