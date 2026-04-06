using UnityEngine;

public enum LayerState
{
    X,
    O
}

public class GridClassicController : MonoBehaviour, IClickCell
{
    [SerializeField] private int AmountCells = 15;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private GameObject GridClassic;
    [SerializeField] private LayerState currentLayer;

    private void Awake()
    {
        currentLayer = LayerState.X;
        GridClassic = this.gameObject;
        SpawnCell();
    }

    private void SpawnCell()
    {
        for (int i = 0; i < AmountCells; i++)
        {
            for (int j = 0; j < AmountCells; j++)
            {
                var cell = Instantiate(cellPrefab, GridClassic.transform);
                cell.SetInfoCell(this, 0, new Vector2(i, j));
            }
        }
    }

    public void OnCellClicked(Cell clickedCell)
    {
        if (currentLayer == LayerState.X)
        {
            clickedCell.SetText(true);
            currentLayer = LayerState.O;
        }
        else
        {
            clickedCell.SetText(false);
            currentLayer = LayerState.X;
        }
    }
}
