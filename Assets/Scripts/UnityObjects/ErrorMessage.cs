using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
    [SerializeField]
    private TMP_Text errorText;
    
    public void SetErrorMessage(string message)
    {
        errorText.text = message;
    }

    public void CloseErrorWindow()
    {
        Destroy(gameObject);
    }
}
