using UnityEngine;
using UnityEngine.UI;

public enum LayerState
{
    Empty,
    X,
    O
}

public class GridGomokuController : MonoBehaviour, IClickCell
{
    [SerializeField] private int ColRow = 15;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private LayerState currentLayer;

    private LayerState[,] boardState;
    private int totalCell = 0;

    private void Awake()
    {
        currentLayer = LayerState.X;
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        gridLayoutGroup.constraintCount = ColRow;

        boardState = new LayerState[ColRow, ColRow];
        SpawnCell();
    }

    private void SpawnCell()
    {
        for (int i = 0; i < ColRow; i++)
        {
            for (int j = 0; j < ColRow; j++)
            {
                var cell = Instantiate(cellPrefab, gridLayoutGroup.transform);
                cell.SetInfoCell(this, 0, new Vector2(i, j));
            }
        }
    }

    public void OnCellClicked(Cell clickedCell)
    {
        CheckWin(clickedCell, currentLayer);
        Draw();
        LayerTurn(clickedCell);
    }


    private void LayerTurn(Cell clickedCell)
    {
        switch (currentLayer)
        {
            case LayerState.X:
                clickedCell.SetText(true);
                currentLayer = LayerState.O;
                break;
            case LayerState.O:
                clickedCell.SetText(false);
                currentLayer = LayerState.X;
                break;
        }
    }

    private void CheckWin(Cell clickedCell, LayerState currentLayer)
    {
        int x = (int)clickedCell.GetPositionCell.x;
        int y = (int)clickedCell.GetPositionCell.y;
        boardState[x, y] = currentLayer;
        if (DirectionLayer(x, y, currentLayer))
        {
            switch (currentLayer)
            {
                case LayerState.X:
                    Debug.Log("Player X wins!");
                    break;
                case LayerState.O:
                    Debug.Log("Player O wins!");
                    break;
            }
        }
    }

    private void Draw()
    {
        totalCell++;
        if (totalCell == ColRow * ColRow)
        {
            Debug.Log("It's a draw!");
        }
    }

    private bool DirectionLayer(int row, int col, LayerState currentLayer)
    {
        Vector2[] directions = new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(1, -1)
        };

        foreach (Vector2 dir in directions)
        {
            int count = 1;

            count += CountPieces(row, col, (int)dir.x, (int)dir.y, currentLayer);
            count += CountPieces(row, col, -(int)dir.x, -(int)dir.y, currentLayer);

            if (count >= 5)
            {
                return true;
            }
        }
        return false;
    }

    
    private int CountPieces(int row, int col, int dirX, int dirY, LayerState player)
    {
        int count = 0;
        int r = row + dirX;
        int c = col + dirY;

        while (r >= 0 && r < ColRow && c >= 0 && c < ColRow && boardState[r, c] == player)
        {
            count++;
            r += dirX;
            c += dirY;
        }

        return count;
    }
}