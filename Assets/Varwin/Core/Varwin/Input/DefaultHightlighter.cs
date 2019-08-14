using UnityEngine;
using Varwin.Public;

namespace Varwin
{
	public class DefaultHightlighter : MonoBehaviour, IHighlightAware
	{

		public HightLightConfig HightLightConfig()
		{
			var defColor = new Color(0f,255f,255f,255f);
			
			HightLightConfig config = new HightLightConfig(true,
				0.3f,
				defColor, 
				false,
				0.2f,
				0.1f,
				0.3f,
				Color.cyan,
				true,
				0f,
				0f,
				Color.red,
				false,
				0f,
				1.0f,
				Color.red);
			
			return config;
		}
	}
}