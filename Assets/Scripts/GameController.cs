using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public Cell[] cells; // Atribuir no Inspector
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI scoreTextX;
    public TextMeshProUGUI scoreTextO;

    private string[] board = new string[9];
    private string currentPlayer = "X";

    private int scoreX = 0;
    private int scoreO = 0;

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = "";
            cells[i].ResetCell();
        }

        currentPlayer = "X";
        statusText.text = "Vez do jogador X";
        UpdateScoreUI();
    }

    public void CellClicked(int index)
    {
        if (board[index] != "")
            return;

        board[index] = currentPlayer;
        cells[index].SetSymbol(currentPlayer);

        if (CheckWin(currentPlayer))
        {
            statusText.text = $"Jogador {currentPlayer} venceu!";

            if (currentPlayer == "X")
                scoreX++;
            else
                scoreO++;

            UpdateScoreUI();
            DisableAllCells();
        }
        else if (IsBoardFull())
        {
            statusText.text = "Deu velha!";
        }
        else
        {
            currentPlayer = currentPlayer == "X" ? "O" : "X";
            statusText.text = $"Vez do jogador {currentPlayer}";
        }
    }

    bool CheckWin(string player)
    {
        int[,] wins = new int[,] {
            {0,1,2}, {3,4,5}, {6,7,8},
            {0,3,6}, {1,4,7}, {2,5,8},
            {0,4,8}, {2,4,6}
        };

        for (int i = 0; i < wins.GetLength(0); i++)
        {
            if (board[wins[i, 0]] == player &&
                board[wins[i, 1]] == player &&
                board[wins[i, 2]] == player)
            {
                return true;
            }
        }

        return false;
    }

    bool IsBoardFull()
    {
        foreach (string cell in board)
            if (cell == "") return false;
        return true;
    }

    void DisableAllCells()
    {
        foreach (var cell in cells)
            cell.button.interactable = false;
    }

    void UpdateScoreUI()
    {
        scoreTextX.text = $"Pontos de X: {scoreX}";
        scoreTextO.text = $"Pontos de O: {scoreO}";
    }

    // Chame isso a partir de um botão para reiniciar
    public void OnRestartButton()
    {
        StartGame();
    }


    // (Opcional) Chame isso para zerar o placar
    public void OnResetScores()
    {
        scoreX = 0;
        scoreO = 0;
        UpdateScoreUI();
    }
}
