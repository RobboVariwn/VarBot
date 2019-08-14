using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Varwin
{

	public class UIFadeInOutController : MonoBehaviour
	{
		public static UIFadeInOutController Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<UIFadeInOutController>();
				}

				return _instance;
			}
		}

		private static UIFadeInOutController _instance;
		
		public enum FadeInOutStatus
		{
			None = 0,
			FadingIn,
			FadingInComplete,
			FadingOut,
			FadingOutComplete
		}

		public float FadeInDuration = 1.0f;
		public float FadeOutDuration = 1.0f;

		public FadeInOutStatus FadeStatus { get; protected set; }

		private CanvasGroup _group;
		private GameObject _blackImage;
		
		
		protected float _fadeInSpeed;
		protected float _fadeOutSpeed;
		
		
		private void Start()
		{
			_instance = this;
			_group = GetComponent<CanvasGroup>();
			_blackImage = GetComponentInChildren<Image>(true).gameObject;
		}

		public virtual void FadeInNow()
		{
			FadeStatus = FadeInOutStatus.FadingInComplete;
			_group.alpha = 1;
		}

		private void Update()
		{

			if (FadeStatus == FadeInOutStatus.None || FadeStatus == FadeInOutStatus.FadingInComplete || FadeStatus == FadeInOutStatus.FadingOutComplete)
			{
				return;
			}

			if (FadeStatus == FadeInOutStatus.FadingIn)
			{
				_group.alpha += _fadeInSpeed * Time.deltaTime;

				_blackImage.SetActive(true);
				
				if (_group.alpha >= 1.0f)
				{
					_group.alpha = 1.0f;
					FadeStatus = FadeInOutStatus.FadingInComplete;

					return;
				}
			}
			else if (FadeStatus == FadeInOutStatus.FadingOut)
			{
				_group.alpha -= _fadeOutSpeed * Time.deltaTime;

				if (_group.alpha <= 0.0f)
				{
					_blackImage.SetActive(false);
					_group.alpha = 0.0f;
					FadeStatus = FadeInOutStatus.FadingOutComplete;

					return;
				}
			}
		}

		public virtual void FadeIn()
		{
			FadeStatus = FadeInOutStatus.FadingIn;
			CalculateSpeeds();
		}

		public virtual void FadeOut()
		{
			FadeStatus = FadeInOutStatus.FadingOut;
			CalculateSpeeds();
		}

		private void CalculateSpeeds()
		{
			_fadeInSpeed = 1.0f / FadeInDuration;
			_fadeOutSpeed = 1.0f / FadeOutDuration;
		}
	}
}
