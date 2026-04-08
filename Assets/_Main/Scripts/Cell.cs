using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] private TMP_Text Text;
    [SerializeField] private Button ButtonCell;
    [SerializeField] private Color ColorX;
    [SerializeField] private Color ColorO;
    [SerializeField] private int Value;
    [SerializeField] private Vector2 PositionCell;

    private IClickCell _gridClassic;

    public Vector2 GetPositionCell => PositionCell;

    public void SetInfoCell(IClickCell gridClassic, int value, Vector2 positionCell)
    {
        _gridClassic = gridClassic;
        Value = value;
        PositionCell = positionCell;
        ClearCell();
    }

    public void SetText(bool currentCell)
    {
        if (currentCell)
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

    public void SetText(LayerState state)
    {
        switch (state)
        {
            case LayerState.X:
                Text.text = "X";
                Text.color = ColorX;
                break;

            case LayerState.O:
                Text.text = "O";
                Text.color = ColorO;
                break;

            default:
                ClearCell();
                break;
        }
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(Text.text);
    }

    public void ClearCell()
    {
        Text.text = "";
    }

    private void Start()
    {
        ButtonCell.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (!IsEmpty())
            return;

        _gridClassic?.OnCellClicked(this);
    }
}