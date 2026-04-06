using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class Cell : MonoBehaviour
{
    [SerializeField] private TMP_Text Text;
    [SerializeField] private Button ButtonCell;
    [SerializeField] private Color ColorX;
    [SerializeField] private Color ColorO;
    [SerializeField] private int Value;
    [SerializeField] private Vector2 PositionCell;
    private IClickCell _gridClassic;

    public void SetInfoCell(IClickCell gridClassic, int value, Vector2 positionCell)
    {
        this._gridClassic = gridClassic;
        this.Value = value;
        this.PositionCell = positionCell;
    }

    public void SetText(bool result)
    {
        if(result)
        {
            Text.text = "X";
            Text.color = ColorX;
        }
        else
        {
            Text.text = "O";
            Text.color = ColorO;
        }

    }

    private void Start()
    {
        ButtonCell.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _gridClassic.OnCellClicked(this);
    }
}
