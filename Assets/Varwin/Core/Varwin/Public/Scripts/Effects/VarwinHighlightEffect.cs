using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;
using QualityLevel = HighlightPlus.QualityLevel;

namespace Varwin.Public
{
	public class VarwinHighlightEffect : MonoBehaviour
	{

		public HightLightConfig Configuration;
		
		private HighlightEffect _effectHolder;
		private HighlightEffect _effect
		{
			get
			{
				if (_effectHolder == null)
				{
					_effectHolder = gameObject.GetComponent<HighlightEffect>();

					if (!_effectHolder)
					{
						_effectHolder = gameObject.AddComponent<HighlightEffect>();
					}
				}

				return _effectHolder;
			}
		}

		public bool IsHighlightEnabled =>  _effect.highlighted;
		
		void Start()
		{
			SetupWithConfig(Configuration);
		}

		private void OnDestroy()
		{
			Destroy(_effectHolder);
		}


		private void SetupWithConfig(HightLightConfig config, HighlightEffect newEffect = null)
		{
			if (newEffect != null)
			{
				_effectHolder = newEffect;
			}
			
			if (config.Outline)
			{
				_effect.outline = 1;
				_effect.outlineWidth = config.OutlineWidth;
				_effect.outlineColor = config.OutlineColor;
			}
			else
			{
				_effect.outline = 0;
			}

			if (config.Glow)
			{
				_effect.glow = 1;
				_effect.glowWidth = config.GlowWidth;

				GlowPassData glowPass = new GlowPassData();
				glowPass.alpha = config.GlowAlpha;
				glowPass.offset = config.GlowOffset;
				glowPass.color = config.GlowColor;
				
				_effect.glowHQColor = config.GlowColor;

				_effect.glowPasses[0] = glowPass;

				_effect.glowDithering = false;
				_effect.glowAnimationSpeed = 0;
			}
			else
			{
				_effect.glow = 0;
			}

			if (config.SeeThrough)
			{
				_effect.seeThrough = SeeThroughMode.WhenHighlighted;
				_effect.seeThroughIntensity = config.SeeThroughIntensity;
				_effect.seeThroughTintAlpha = config.SeeThroughAlpha;
				_effect.seeThroughTintColor = config.SeeThroughColor;
			}
			else
			{
				_effect.seeThrough = SeeThroughMode.Never;
			}

			if (config.Overlay)
			{
				_effect.overlayColor = config.OverlayColor;
				_effect.overlayMinIntensity = 0.5f;
				_effect.overlay = config.OverlayAlpha;
				_effect.overlayAnimationSpeed = config.OverlayAnimationSpeed;
			}
			else
			{
				_effect.overlayMinIntensity = 0f;
				_effect.overlay = 0.0f;
			}

			_effect.outlineQuality = QualityLevel.High;
			_effect.outlineDownsampling = 1;
			
			_effect.Refresh();
		}
		
		public void SetConfiguration(HightLightConfig config, HighlightEffect newEffect = null)
		{
			Configuration = config;
			SetupWithConfig(config, newEffect);
		}

		public void SetHighlightEnabled(bool enabled)
		{
			_effect.highlighted = enabled;
		}
		
	}
}