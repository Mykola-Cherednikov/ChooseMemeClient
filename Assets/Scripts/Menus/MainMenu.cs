using Assets.Scripts.DTO;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject LobbiesMenuPrefab;

    [SerializeField]
    private GameObject SettingsMenuPrefab;

    private void Start()
    {
        if(PlayerPrefs.GetString("IP") == null)
        {
            PlayerPrefs.SetString("IP", "109.87.235.191");
            PlayerPrefs.Save();
        }    
    }

    public void GoToLobbiesMenu()
    {
        LobbiesMenu lobbiesMenu = Instantiate(LobbiesMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<LobbiesMenu>();
        lobbiesMenu.transform.position = this.gameObject.transform.position;
        Destroy(this.gameObject);
    }

    public void GoToSettingsMenu()
    {
        SettingsMenu settingsMenu = Instantiate(SettingsMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<SettingsMenu>();
        settingsMenu.transform.position = this.gameObject.transform.position;
        Destroy(this.gameObject);
    }
}
