using UnityEngine;
using UnityEngine.UI;
using System;

public class TeamCardUI : MonoBehaviour
{
    [Header("References")]
    public Image logoImage;
    public Image kitImage;
    public Text teamNameText;
    public Text strengthText;
    public Button selectButton;

    private TeamData teamData;
    private Action<TeamData> onSelected;

    public void Setup(TeamData data, Action<TeamData> callback)
    {
        teamData = data;
        onSelected = callback;

        teamNameText.text = data.teamName;
        strengthText.text = $"STR: {data.shooterStrength}  GK: {data.keeperStrength}";

        if (data.logo != null) logoImage.sprite = data.logo;
        if (data.kit != null) kitImage.sprite = data.kit;

        logoImage.color = data.primaryColor;
        selectButton.onClick.AddListener(() => onSelected?.Invoke(teamData));
    }
}
