using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nickNameText;

    [SerializeField]
    private TMP_Text pointsText;

    public void SetText(string nickName, int points)
    {
        nickNameText.text = nickName;
        pointsText.text = points.ToString();
    }
}
