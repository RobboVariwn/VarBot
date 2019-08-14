using System;
using Varwin;
using Varwin.UI;
using UnityEngine;

#if UNITY_STANDALONE_WIN 
namespace ZenFulcrum.EmbeddedBrowser {

public class VRBrowserPanel : MonoBehaviour, INewWindowHandler {
	public Browser contentBrowser, 
	    controlBrowser;
	public Transform keyboardLocation;
    private UIMenu _uiMenu;

	public void Awake() {
#if !VRMAKER
		//If the content browser is externally closed, make sure we go too.
		var dd = contentBrowser.gameObject.AddComponent<DestroyDetector>();
		dd.onDestroy += CloseBrowser;

		contentBrowser.SetNewWindowHandler(Browser.NewWindowAction.NewBrowser, this);
		contentBrowser.onLoad += data => controlBrowser.CallFunction("setURL", data["url"]);

		//controlBrowser.RegisterFunction("demoNavForward", args => contentBrowser.GoForward());
		//controlBrowser.RegisterFunction("demoNavBack", args => contentBrowser.GoBack());
		//controlBrowser.RegisterFunction("demoNavRefresh", args => contentBrowser.Reload());
		//controlBrowser.RegisterFunction("demoNavClose", args => CloseBrowser());
		//controlBrowser.RegisterFunction("goTo", args => contentBrowser.LoadURL(args[0], false));

		VRMainControlPanel.instance.keyboard.onFocusChange += OnKeyboardOnOnFocusChange;
	    _uiMenu = FindObjectOfType<UIMenu>();

	    if (_uiMenu == null)
	    {
		    return;
	    }


		_uiMenu.InitBrowser(contentBrowser);
	    contentBrowser.onLoad += ContentBrowserOnOnLoad;
#endif
	    

    }

    private void ContentBrowserOnOnLoad(JSONNode jsonNode)
    {
        _uiMenu.BrowserLoaded();
    }

    public void OnDestroy() {
#if !VRMAKER
		VRMainControlPanel.instance.keyboard.onFocusChange -= OnKeyboardOnOnFocusChange;
#endif
	}

	private void OnKeyboardOnOnFocusChange(Browser browser, bool editable) {
#if !VRMAKER
		if (!editable || !browser) VRMainControlPanel.instance.MoveKeyboardUnder(null);
		else if (browser == contentBrowser || browser == controlBrowser) VRMainControlPanel.instance.MoveKeyboardUnder(this);
#endif
	}

	public void CloseBrowser() {
#if !VRMAKER
		if (!this || !VRMainControlPanel.instance) return;
	    BrowserDestructor.Instance.DestroyPad(this);
#endif
	}

	public Browser CreateBrowser(Browser parent) {
#if !VRMAKER
		var newPane = VRMainControlPanel.instance.OpenNewTab(this);
		newPane.transform.position = transform.position;
		newPane.transform.rotation = transform.rotation;
		return newPane.contentBrowser;
#endif
		return null;
	}

}

internal class DestroyDetector : MonoBehaviour {
	public event Action onDestroy = () => {};

	public void OnDestroy() {
		onDestroy();
	}

}

}
#endif