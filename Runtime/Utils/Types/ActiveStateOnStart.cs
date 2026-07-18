using UnityEngine;

namespace Conkist.GDK.Utils
{
	public class ActiveStateOnStart : MonoBehaviour
	{
		public bool Active;
		[MustBeAssigned] public GameObject Target;

		private void Awake()
		{
			Target.gameObject.SetActive(Active);
		}
	}
}