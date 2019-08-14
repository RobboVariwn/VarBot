using System;
using UnityEngine;

namespace Varwin.Public
{
    public interface IHighlightAware
    {
        HightLightConfig HightLightConfig();
    }

    [Serializable]
    public class HightLightConfig
    {
        
        //Outline
        public bool Outline;
        public float OutlineWidth;
        public Color OutlineColor;

        //Glow
        public bool Glow;
        public float GlowWidth;
        public float GlowOffset;
        public float GlowAlpha;
        public Color GlowColor;

        //See through
        public bool SeeThrough;
        public float SeeThroughIntensity;
        public float SeeThroughAlpha;
        public Color SeeThroughColor;
        
        //Overlay
        public bool Overlay;
        public float OverlayAlpha;
        public Color OverlayColor;
        public float OverlayAnimationSpeed;

        public HightLightConfig(
            bool outline,
            float outlineWidth,
            Color? outlineColor,
            bool glow,
            float glowWidth,
            float glowOffset,
            float glowAlpha,
            Color? glowColor,
            bool seeThrough = false,
            float seeThroughIntensity = 0.8f,
            float seeThroughAlpha = 0.5f,
            Color? seeThroughColor = null,
            bool overlay = false,
            float overlayAlpha = 0.0f,
            float overlayAnimationSpeed = 0.0f,
            Color? overlayColor = null)
        {
            Outline = outline;
            OutlineColor = outlineColor ?? Color.clear;
            OutlineWidth = outlineWidth;

            Glow = glow;
            GlowWidth = glowWidth;
            GlowOffset = glowOffset;
            GlowAlpha = glowAlpha;
            GlowColor = glowColor ?? Color.clear;

            SeeThrough = seeThrough;
            SeeThroughIntensity = seeThroughIntensity;
            SeeThroughAlpha = seeThroughAlpha;
            SeeThroughColor = seeThroughColor ?? Color.clear;

            Overlay = overlay;
            OverlayAlpha = overlayAlpha;
            OverlayColor = overlayColor ?? Color.red;
            OverlayAnimationSpeed = overlayAnimationSpeed;
        }
        
    }


}
