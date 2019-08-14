using Varwin;
using Varwin.UI;
using SmartLocalization;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public class UIMode : UIButton
{
    public GameObject View;
    public GameObject Edit;
    private UIMenu _uiMenu;

    private void Start()
    {
        _uiMenu = FindObjectOfType<UIMenu>();
        ProjectData.GameModeChanged += OnGameModeChanged;
        OnGameModeChanged(ProjectData.GameMode);
    }

    private void OnDestroy()
    {
        ProjectData.GameModeChanged -= OnGameModeChanged;
    }

    private void OnGameModeChanged(GameMode newGameMode)
    {
        switch (newGameMode)
        {
            case GameMode.Edit:
                View.SetActive(false);
            break;
            
            case GameMode.Preview:
                _uiMenu?.BrowserWindow?.SetActive(false);
                View.SetActive(true);
            break;
            
            case GameMode.View:
                _uiMenu?.BrowserWindow?.SetActive(false);
                View.SetActive(false);
            break;
            
            case GameMode.Undefined: 
                View.SetActive(false);
            break;
        }
    }

    public override void OnClick()
    {
        if (ProjectData.GameMode == GameMode.View)
        {
            return;
        }

        if (ProjectData.GameMode == GameMode.Edit && ProjectData.ObjectsAreChanged)
        {
            Helper.AskUserToDo(LanguageManager.Instance.GetTextValue("GROUP_NOT_SAVED"),
                () =>
                {
                    Helper.SaveSceneObjects();
                    ApplyGameMode();
                },
                () =>
                {
                    ApplyGameMode();
                });
        }
        else
        {
            ApplyGameMode();
        }

    }

    private void ApplyGameMode()
    {
        if (ProjectData.GameMode == GameMode.Preview)
        {
            ProjectData.GameMode = GameMode.Edit;
            _uiMenu.ShowMenu();

            return;
        }

        if (ProjectData.GameMode == GameMode.Edit)
        {
            ProjectData.GameMode = GameMode.Preview;
        }
    }
}
