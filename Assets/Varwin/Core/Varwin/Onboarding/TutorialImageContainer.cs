using SmartLocalization;
using UnityEngine;

public class TutorialImageContainer : MonoBehaviour
{
    public Sprite Use;
    public Sprite Teleport;

    [SerializeField] private Sprite _menuEn;
    [SerializeField] private Sprite _menuRu;

    public Sprite Menu
    {
        get
        {
            switch (LanguageManager.Instance.CurrentlyLoadedCulture.languageCode)
            {
                case "ru":
                    return _menuRu;
                default:
                    return _menuEn;
            }
        }
    }
    
    public Sprite Grab;
    public Sprite Eye;
    
    
}
