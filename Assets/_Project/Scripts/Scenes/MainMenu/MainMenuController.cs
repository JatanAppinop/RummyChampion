using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] TabGroup tabBar;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

    }

    public void onBackButtonPressed()
    {
        if (PopoverViewController.Instance.ViewStackCount > 0)
        {
            PopoverViewController.Instance.GoBack();
            return;
        }

        if (tabBar.GetSelectedTab.gameObject.name.ToLower().Equals("home"))
        {
            AlertSlider.Instance.Show("Are You sure you want to quit the game?", "Quit Game", "Cancel")
            .OnPrimaryAction(() => Application.Quit());
        }
        else
        {
            tabBar.NavigateTo("Home");
        }
    }
}
