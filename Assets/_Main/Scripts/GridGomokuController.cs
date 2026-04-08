using System.Collections;
using System.Collections.Generic;
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
    [Header("Board")]
    [SerializeField] private int ColRow = 15;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("Turn")]
    [SerializeField] private LayerState currentLayer = LayerState.X;

    [Header("AI Settings")]
    [SerializeField] private bool playWithAI = true;
    [SerializeField] private float aiThinkDelay = 0.05f;
    [SerializeField] private int searchDepth = 2;
    [SerializeField] private int candidateRadius = 2;
    [SerializeField] private int maxCandidateMoves = 12;

    private LayerState[,] boardState;
    private Cell[,] cells;
    private int totalCell;
    private bool isGameOver;
    private bool isAITurn;

    private readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // ngang
        new Vector2Int(1, 0),   // dọc
        new Vector2Int(1, 1),   // chéo \
        new Vector2Int(1, -1)   // chéo /
    };

    private const int WIN_SCORE = 100000000;

    private void Awake()
    {
        currentLayer = LayerState.X;
        totalCell = 0;
        isGameOver = false;
        isAITurn = false;

        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponent<GridLayoutGroup>();

        gridLayoutGroup.constraintCount = ColRow;

        boardState = new LayerState[ColRow, ColRow];
        cells = new Cell[ColRow, ColRow];

        SpawnCell();
    }

    private void SpawnCell()
    {
        for (int i = 0; i < ColRow; i++)
        {
            for (int j = 0; j < ColRow; j++)
            {
                Cell cell = Instantiate(cellPrefab, gridLayoutGroup.transform);
                cell.SetInfoCell(this, 0, new Vector2(i, j));
                cells[i, j] = cell;
                boardState[i, j] = LayerState.Empty;
            }
        }
    }

    public void OnCellClicked(Cell clickedCell)
    {
        if (isGameOver || isAITurn)
            return;

        if (currentLayer != LayerState.X)
            return;


        int x = (int)clickedCell.GetPositionCell.x;
        int y = (int)clickedCell.GetPositionCell.y;

        if (!IsInside(x, y))
            return;

        if (boardState[x, y] != LayerState.Empty)
            return;

        MakeMove(clickedCell, x, y, LayerState.X);

        if (isGameOver)
            return;

        if (playWithAI)
            StartCoroutine(AITurnCoroutine());

        ManageSound.Instance.PlaySoundClick(clickedCell);
    }

    private IEnumerator AITurnCoroutine()
    {
        isAITurn = true;
        currentLayer = LayerState.O;

        yield return new WaitForSecondsRealtime(aiThinkDelay);

        Vector2Int bestMove = FindBestMove();

        if (bestMove.x >= 0 && bestMove.y >= 0)
        {
            Cell aiCell = cells[bestMove.x, bestMove.y];
            MakeMove(aiCell, bestMove.x, bestMove.y, LayerState.O);
        }

        if (!isGameOver)
            currentLayer = LayerState.X;

        isAITurn = false;
    }

    private void MakeMove(Cell cell, int x, int y, LayerState player)
    {
        boardState[x, y] = player;
        cell.SetText(player);
        totalCell++;

        if (CheckWin(x, y, player))
        {
            isGameOver = true;
            Debug.Log(player == LayerState.X ? "Player X wins!" : "AI O wins!");
            return;
        }

        if (totalCell >= ColRow * ColRow)
        {
            isGameOver = true;
            Debug.Log("It's a draw!");
        }
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < ColRow && y >= 0 && y < ColRow;
    }

    private bool CheckWin(int row, int col, LayerState player)
    {
        foreach (Vector2Int dir in directions)
        {
            int count = 1;
            count += CountPieces(row, col, dir.x, dir.y, player);
            count += CountPieces(row, col, -dir.x, -dir.y, player);

            if (count >= 5)
                return true;
        }

        return false;
    }

    private int CountPieces(int row, int col, int dirX, int dirY, LayerState player)
    {
        int count = 0;
        int r = row + dirX;
        int c = col + dirY;

        while (IsInside(r, c) && boardState[r, c] == player)
        {
            count++;
            r += dirX;
            c += dirY;
        }

        return count;
    }

    private bool IsBoardFullByState()
    {
        for (int i = 0; i < ColRow; i++)
        {
            for (int j = 0; j < ColRow; j++)
            {
                if (boardState[i, j] == LayerState.Empty)
                    return false;
            }
        }
        return true;
    }

    // =========================
    // AI
    // =========================

    private Vector2Int FindBestMove()
    {
        List<Vector2Int> moves = GenerateCandidateMoves();

        if (moves.Count == 0)
            return new Vector2Int(ColRow / 2, ColRow / 2);

        int bestScore = int.MinValue;
        Vector2Int bestMove = moves[0];

        foreach (Vector2Int move in moves)
        {
            boardState[move.x, move.y] = LayerState.O;

            int score;
            if (CheckWin(move.x, move.y, LayerState.O))
            {
                score = WIN_SCORE;
            }
            else
            {
                score = MiniMax(searchDepth - 1, false, int.MinValue, int.MaxValue);
            }

            boardState[move.x, move.y] = LayerState.Empty;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int MiniMax(int depth, bool isMaximizing, int alpha, int beta)
    {
        int eval = EvaluateBoard();

        if (depth <= 0 || Mathf.Abs(eval) >= WIN_SCORE / 2 || IsBoardFullByState())
            return eval;

        List<Vector2Int> moves = GenerateCandidateMoves();
        if (moves.Count == 0)
            return eval;

        if (isMaximizing)
        {
            int maxEval = int.MinValue;

            foreach (Vector2Int move in moves)
            {
                boardState[move.x, move.y] = LayerState.O;

                int score;
                if (CheckWin(move.x, move.y, LayerState.O))
                    score = WIN_SCORE - (searchDepth - depth);
                else
                    score = MiniMax(depth - 1, false, alpha, beta);

                boardState[move.x, move.y] = LayerState.Empty;

                if (score > maxEval)
                    maxEval = score;

                if (score > alpha)
                    alpha = score;

                if (beta <= alpha)
                    break;
            }

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (Vector2Int move in moves)
            {
                boardState[move.x, move.y] = LayerState.X;

                int score;
                if (CheckWin(move.x, move.y, LayerState.X))
                    score = -WIN_SCORE + (searchDepth - depth);
                else
                    score = MiniMax(depth - 1, true, alpha, beta);

                boardState[move.x, move.y] = LayerState.Empty;

                if (score < minEval)
                    minEval = score;

                if (score < beta)
                    beta = score;

                if (beta <= alpha)
                    break;
            }

            return minEval;
        }
    }

    private List<Vector2Int> GenerateCandidateMoves()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        HashSet<Vector2Int> unique = new HashSet<Vector2Int>();

        bool hasPiece = false;

        for (int x = 0; x < ColRow; x++)
        {
            for (int y = 0; y < ColRow; y++)
            {
                if (boardState[x, y] == LayerState.Empty)
                    continue;

                hasPiece = true;

                for (int dx = -candidateRadius; dx <= candidateRadius; dx++)
                {
                    for (int dy = -candidateRadius; dy <= candidateRadius; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (!IsInside(nx, ny))
                            continue;

                        if (boardState[nx, ny] != LayerState.Empty)
                            continue;

                        Vector2Int move = new Vector2Int(nx, ny);

                        if (!unique.Contains(move))
                        {
                            unique.Add(move);
                            result.Add(move);
                        }
                    }
                }
            }
        }

        if (!hasPiece)
        {
            result.Add(new Vector2Int(ColRow / 2, ColRow / 2));
            return result;
        }

        result.Sort((a, b) =>
        {
            int scoreA = QuickMoveScore(a);
            int scoreB = QuickMoveScore(b);
            return scoreB.CompareTo(scoreA);
        });

        if (result.Count > maxCandidateMoves)
            result = result.GetRange(0, maxCandidateMoves);

        return result;
    }

    private int QuickMoveScore(Vector2Int move)
    {
        // thử AI đi
        boardState[move.x, move.y] = LayerState.O;
        int scoreAI = EvaluateBoard();
        boardState[move.x, move.y] = LayerState.Empty;

        // thử người chơi đi
        boardState[move.x, move.y] = LayerState.X;
        int scorePlayer = Mathf.Abs(EvaluateBoard());
        boardState[move.x, move.y] = LayerState.Empty;

        return scoreAI + scorePlayer;
    }

    // =========================
    // Evaluate
    // =========================

    private int EvaluateBoard()
    {
        int aiScore = EvaluatePlayer(LayerState.O);
        int playerScore = EvaluatePlayer(LayerState.X);

        if (aiScore >= WIN_SCORE / 2)
            return WIN_SCORE;

        if (playerScore >= WIN_SCORE / 2)
            return -WIN_SCORE;

        return aiScore - playerScore;
    }

    private int EvaluatePlayer(LayerState player)
    {
        int totalScore = 0;

        for (int x = 0; x < ColRow; x++)
        {
            for (int y = 0; y < ColRow; y++)
            {
                if (boardState[x, y] != player)
                    continue;

                foreach (Vector2Int dir in directions)
                {
                    int prevX = x - dir.x;
                    int prevY = y - dir.y;

                    // tránh đếm trùng chuỗi
                    if (IsInside(prevX, prevY) && boardState[prevX, prevY] == player)
                        continue;

                    totalScore += EvaluateLineFrom(x, y, dir.x, dir.y, player);
                }
            }
        }

        return totalScore;
    }

    private int EvaluateLineFrom(int x, int y, int dx, int dy, LayerState player)
    {
        int count = 0;
        int curX = x;
        int curY = y;

        while (IsInside(curX, curY) && boardState[curX, curY] == player)
        {
            count++;
            curX += dx;
            curY += dy;
        }

        bool openEnd1 = false;
        bool openEnd2 = false;

        int prevX = x - dx;
        int prevY = y - dy;

        if (IsInside(prevX, prevY) && boardState[prevX, prevY] == LayerState.Empty)
            openEnd1 = true;

        if (IsInside(curX, curY) && boardState[curX, curY] == LayerState.Empty)
            openEnd2 = true;

        int openEnds = 0;
        if (openEnd1) openEnds++;
        if (openEnd2) openEnds++;

        return GetPatternScore(count, openEnds);
    }

    private int GetPatternScore(int count, int openEnds)
    {
        if (count >= 5) return WIN_SCORE;
        if (openEnds == 0) return 0;

        switch (count)
        {
            case 4:
                if (openEnds == 2) return 1000000; // open four
                if (openEnds == 1) return 100000;  // closed four
                break;

            case 3:
                if (openEnds == 2) return 10000;   // open three
                if (openEnds == 1) return 1000;    // closed three
                break;

            case 2:
                if (openEnds == 2) return 500;     // open two
                if (openEnds == 1) return 100;     // closed two
                break;

            case 1:
                if (openEnds == 2) return 10;
                break;
        }

        return 0;
    }
}