using System.Collections;
using UnityEngine;

#if UNITY_STANDALONE_WIN && !VRMAKER
namespace ZenFulcrum.EmbeddedBrowser {

/**
 * A simple set of examples of scripting/JavaScript interacting with a Browser.
 */
[RequireComponent(typeof(Browser))]
public class SimpleScripting : MonoBehaviour {
	private Browser browser;

	public void Start() {
		browser = GetComponent<Browser >();

		//Load some HTML. Normally you'd want to use BrowserAssets + localGame://, 
		//but let's just put everything here for simplicity/visibility right now.
	    browser.LoadHTML(@"

<p >&nbsp;</p >
<p >&nbsp;</p >
<table style=""margin - left: auto; margin - right: auto; "" >
	        <tbody>
	        <tr>
	        <td><strong> Trigger </strong></td>
	        <td><strong> Display </strong></td>
	        <td><strong> Valve </strong></td>
	        </tr>
	        <tr>
	        <td><img onclick = ""spawnObject(1)"" src = ""http://192.168.0.105:3000/v1/objects/resources/c9/c949de70-7e44-42b7-b34e-b84efd1afbf1/bundle.png"" /></td>
	        <td><img onclick = ""spawnObject(2)"" src = ""http://192.168.0.105:3000/v1/objects/resources/c9/c949de70-7e44-42b7-b34e-b84efd1afbf2/bundle.png"" /></td>
	        <td><img onclick = ""spawnObject(4)"" src = ""http://192.168.0.105:3000/v1/objects/resources/c9/c949de70-7e44-42b7-b34e-b84efd1afbf5/bundle.png"" /></td>
	        </tr>
	        <tr>
	        <td><strong> Zone </strong></td>
	        <td><strong> Animated </strong></td>
	        <td><strong> Liquid </strong></td>
	        </tr>
	        <tr>
	        <td><img onclick = ""spawnObject(5)"" src = ""http://192.168.0.105:3000/v1/objects/resources/c9/c949de70-7e44-42b7-b34e-b84efd1afbf6/bundle.png"" /></td>
	        <td><img onclick = ""spawnObject(7)"" src = ""http://192.168.0.105:3000/v1/objects/resources/a7/a759f163-146d-40b1-a1b6-11dd01063faa/bundle.png"" /></td>
	        <td><img onclick = ""spawnObject(13)"" src = ""http://192.168.0.105:3000/v1/objects/resources/d0/d0277246-8289-4439-9732-c438f16648cc/bundle.png"" /></td>
	        </tr>
	        </tbody>
	        </table>
                                                                      ");

		//Set up a function. Notice how the <button > above calls this function when it's clicked.
		browser.RegisterFunction("spawnObject", args => {
			//Args is an array of arguments passed to the function.
			//args[n] is a JSONNode. When you use it, it will implicitly cast to the type at hand.
			int xPos = args[0];
			//int yPos = args[1];

			//Note that if, say, args[0] was a string instead of an integer we'd get default(int) above.
			//See JSONNode.cs for more information.

			Debug.Log("The <color=green >green</color > button was clicked at " + xPos + ", " );
		});
	}


	
	/** Fetches the username and logs it to the Unity console. */
	public void GetUsername() {
		browser.EvalJS("document.getElementById('username').value").Then(username => {
			Debug.Log("The username is: " + username);
		}).Done();

		//Note that the fetch above is asynchronous, this line of code will happen before the Debug.Log above:
		Debug.Log("Fetching username");
	}


	private int colorIdx;
	private Color[] colors = {
		new Color(1, 0, 0), 
		new Color(1, 1, 0), 
		new Color(1, 1, 1), 
		new Color(1, 1, 0), 
	};

	/** Changes the color of the box on the page by calling a function in the page. */
	public void ChangeColor() {
		var color = colors[colorIdx++ % colors.Length];
		browser.CallFunction("changeColor", color.r, color.g, color.b, "Selection Number " + colorIdx).Done();
	}



	/** Fetches the username and logs it to the Unity console (alternate method). */
	public void GetUsername2() {
		StartCoroutine(_GetUsername2());
	}

	private IEnumerator _GetUsername2() {
		//This method is more useful if you are already in a coroutine, or you have a lot of logic 
		//you need to work with that is ugly/painful to express with .Then() chaining.

		var promise = browser.EvalJS("document.getElementById('username').value");

		Debug.Log("Fetching username");

		//Waits for the JS to run and get the result back to us.
		yield return promise.ToWaitFor();

		Debug.Log("The username is: " + promise.Value);
	}
}

}
#endif