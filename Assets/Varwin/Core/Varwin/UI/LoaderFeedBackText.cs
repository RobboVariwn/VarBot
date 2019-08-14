using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Varwin;

public class LoaderFeedBackText : MonoBehaviour
{
    // Start is called before the first frame update

    private Text _text;
    private TMP_Text _tmpText;
    void Start()
    {
        _text = GetComponent<Text>();
        _tmpText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_text != null)
        {
            _text.text = LoaderAdapter.FeedBackText;
        }
        
        if (_tmpText != null)
        {
            _tmpText.text = LoaderAdapter.FeedBackText;
        }
    }
}
