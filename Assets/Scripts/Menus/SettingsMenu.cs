using TMPro;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject MainMenuPrefab;

    [SerializeField]
    private TMP_InputField NickNameInputField;

    [SerializeField]
    private TMP_Dropdown dropdown;

    public void Start()
    {
        NickNameInputField.text = PlayerPrefs.GetString("NickName");

        if (PlayerPrefs.GetString("IP") == "109.87.235.191")
        {
            dropdown.value = 0;
        }
        else
        {
            dropdown.value = 1;
        }
    }

    public void Save()
    {
        if(dropdown.value == 0)
        {
            PlayerPrefs.SetString("IP", "109.87.235.191");
        }
        else
        {
            PlayerPrefs.SetString("IP", "26.210.70.154");
        }
        PlayerPrefs.SetString("NickName", NickNameInputField.text);
        PlayerPrefs.Save();
    }

    public void BackToMainMenu()
    {
        MainMenu settingsMenu = Instantiate(MainMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<MainMenu>();
        settingsMenu.transform.position = this.gameObject.transform.position;
        Destroy(this.gameObject);
    }
}
