using UnityEngine;
using Varwin.Public;

namespace Varwin
{
	public class DefaultHaptics : MonoBehaviour, IHapticsAware
	{
		public HapticsConfig HapticsOnTouch()
		{
			return DefaultHaptics.GetDefaultInteractHaptics();
		}

		public HapticsConfig HapticsOnGrab()
		{
			return DefaultHaptics.GetDefaultInteractHaptics();
		}

		public HapticsConfig HapticsOnUse()
		{
			return DefaultHaptics.GetDefaultInteractHaptics();
		}

		public static HapticsConfig GetDefaultInteractHaptics()
		{
			return new HapticsConfig(0.1f, 0.1f, 0.05f);
		}
	}
}