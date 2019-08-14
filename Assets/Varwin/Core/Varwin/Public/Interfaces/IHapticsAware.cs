using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin.Public
{
	public interface IHapticsAware
	{
		HapticsConfig HapticsOnTouch();
		HapticsConfig HapticsOnGrab();
		HapticsConfig HapticsOnUse();
	}

	public class HapticsConfig
	{
		public float Strength;
		public float Duration;
		public float Interval;
		public bool CancelOnUnaction;

		public HapticsConfig(float strength, float duration, float interval, bool cancel = true)
		{
			Strength = strength;
			Duration = duration;
			Interval = interval;
			CancelOnUnaction = cancel;
		}
	}
}
