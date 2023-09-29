using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientInfoRow : MonoBehaviour
{
    [SerializeField]
    private TMP_Text clientNameText;

    [SerializeField]
    private TMP_Text clientIdText;

    public int id;

    public void SetText(int id, string name)
    {
        clientNameText.text = name;
        clientIdText.text = id.ToString();
    }
}
