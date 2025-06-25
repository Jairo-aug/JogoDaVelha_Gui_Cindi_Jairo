using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum GameOptions
{
    PlayerVsPlayer = 0,
    PlayerVsBot = 1,
    BotVsBot = 2
}

public enum BotLevels
{
    Dumb = 0, // Joga aleatoriamente
    MachineLearning = 1, // Usa aprendizado de m��quina (n��o implementado)
    Minimax = 2 // Usa algoritmo Minimax
}

public class GameController : MonoBehaviour
{
    public Cell[] cells; // Atribuir no Inspector
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI scoreTextX;
    public TextMeshProUGUI scoreTextO;
    public GameOptions gameOption = GameOptions.PlayerVsPlayer; // Para ativar o bot, se necessário
    public BotLevels botLevel = BotLevels.Minimax; // N��vel do bot, se for o caso
    public GameObject menu; // Refer��ncia ao menu de sele����o de jogo
    public GameObject botLevelMenu; // Refer��ncia ao menu de sele����o de n��vel do bot
    public GameObject gameCanvas; // Refer��ncia ao canvas do jogo
    public int mlEpisodes = 1000; // N��mero de epis��dios para treinar o bot de aprendizado de m��quina
    private List<string> mlStates = new();
    private List<int> mlActions = new();

    public string[] board = new string[9];
    private string currentPlayer = "X";

    private int scoreX = 0;
    private int scoreO = 0;

    private QLearningBot mlBot = new QLearningBot();

    public void GameSelected(int gameOption)
    {
        switch (gameOption)
        {
            case (int)GameOptions.PlayerVsPlayer:
                this.gameOption = GameOptions.PlayerVsPlayer;
                break;
            case (int)GameOptions.PlayerVsBot:
                this.gameOption = GameOptions.PlayerVsBot;
                break;
            case (int)GameOptions.BotVsBot:
                this.gameOption = GameOptions.BotVsBot; // Implementar lógica de bot vs bot se necessário
                break;
        }
        menu.SetActive(false); // Esconder o menu de sele����o de jogo
        if (gameOption != (int)GameOptions.PlayerVsPlayer)
            botLevelMenu.SetActive(true); // Mostrar o canvas do jogo
        else
            gameCanvas.SetActive(true); // Mostrar o canvas do jogo
    }

    public void BotLevelSelected(int botLevel)
    {
        switch (botLevel)
        {
            case (int)BotLevels.Dumb:
                this.botLevel = BotLevels.Dumb;
                break;
            case (int)BotLevels.MachineLearning:
                this.botLevel = BotLevels.MachineLearning; // N��o implementado
                mlBot.Train(mlEpisodes); // Treina o bot com epis��dios
                break;
            case (int)BotLevels.Minimax:
                this.botLevel = BotLevels.Minimax;
                break;
        }
        botLevelMenu.SetActive(false); // Esconder o menu de sele����o de n��vel do bot
        gameCanvas.SetActive(true); // Mostrar o canvas do jogo

        if(gameOption == GameOptions.BotVsBot)
        {
            StartCoroutine(BotVsBotGame()); // Inicia o jogo Bot vs Bot
        }
        else
        {
            StartGame(); // Reinicia o jogo
        }
    }

    IEnumerator BotVsBotGame()
    {
        while (!IsBoardFull() && !CheckWin(currentPlayer))
        {
            BotAction();
            yield return new WaitForSeconds(1f); // Espera 1 segundo entre as jogadas
        }
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
            StopAllCoroutines(); // Para o jogo Bot vs Bot, se estiver rodando
            ApplyMachineLearningReward();
        }
        else if (IsBoardFull())
        {
            statusText.text = "Deu velha!";
            StopAllCoroutines(); // Para o jogo Bot vs Bot, se estiver rodando
            ApplyMachineLearningReward();
        }
        else
        {
            currentPlayer = currentPlayer == "X" ? "O" : "X";
            statusText.text = $"Vez do jogador {currentPlayer}";
            if (gameOption == GameOptions.PlayerVsBot && currentPlayer == "O")
            {
                BotAction(); // Chame a a����o do bot aqui, se implementada
            }
        }
    }
    
    public void ApplyMachineLearningReward()
    {
        if (botLevel == BotLevels.MachineLearning)
        {
            string winner = CheckWinner(board);
            double reward = 0;

            if (winner == "O") reward = 1; // bot venceu
            else if (winner == "X") reward = -1; // bot perdeu
            else if (IsBoardFull()) reward = 0.5; // empate

            mlBot.UpdateQTable(mlStates, mlActions, reward);
            mlStates.Clear();
            mlActions.Clear();
        }
    }

    public void BotAction()
    {
        int bestIndex = -1;
        if (botLevel == BotLevels.Dumb)
        {
            // Joga aleatoriamente
            bestIndex = Random.Range(0, 9);
            while (board[bestIndex] != "")
            {
                bestIndex = Random.Range(0, 9);
            }
        }
        else if (botLevel == BotLevels.Minimax)
        {
            // Usa o algoritmo Minimax
            bestIndex = GetBestMove();
        }
        else if (botLevel == BotLevels.MachineLearning)
        {
            bestIndex = mlBot.ChooseAction(board, currentPlayer);
            string currentState = mlBot.BoardToString(board);
            mlStates.Add(currentState);
            mlActions.Add(bestIndex);
        }
        Debug.Log($"Bot jogando na posi����o: {bestIndex}");
        CellClicked(bestIndex);
    }

    int GetBestMove()
    {
        string botSymbol = currentPlayer;
        string playerSymbol = botSymbol == "X" ? "O" : "X";

        int bestScore = int.MinValue;
        int move = -1;

        for (int i = 0; i < 9; i++)
        {
            if (board[i] == "")
            {
                board[i] = botSymbol;
                int score = Minimax(board, 0, false, botSymbol, playerSymbol);
                board[i] = "";
                if (score > bestScore)
                {
                    bestScore = score;
                    move = i;
                }
            }
        }
        return move;
    }

    int Minimax(string[] board, int depth, bool isMaximizing, string botSymbol, string playerSymbol)
    {
        string winner = CheckWinner(board);
        if (winner == botSymbol)
            return 10 - depth;
        if (winner == playerSymbol)
            return depth - 10;
        if (IsBoardFull(board))
            return 0;

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == "")
                {
                    board[i] = botSymbol;
                    int score = Minimax(board, depth + 1, false, botSymbol, playerSymbol);
                    board[i] = "";
                    bestScore = Mathf.Max(score, bestScore);
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == "")
                {
                    board[i] = playerSymbol;
                    int score = Minimax(board, depth + 1, true, botSymbol, playerSymbol);
                    board[i] = "";
                    bestScore = Mathf.Min(score, bestScore);
                }
            }
            return bestScore;
        }
    }
    bool IsBoardFull(string[] board)
    {
        for (int i = 0; i < 9; i++)
            if (board[i] == "")
                return false;
        return true;
    }
    string CheckWinner(string[] board)
    {
        int[,] winPatterns = new int[,]
        {
            {0,1,2}, {3,4,5}, {6,7,8}, // rows
            {0,3,6}, {1,4,7}, {2,5,8}, // cols
            {0,4,8}, {2,4,6}           // diags
        };

        for (int i = 0; i < winPatterns.GetLength(0); i++)
        {
            int a = winPatterns[i, 0], b = winPatterns[i, 1], c = winPatterns[i, 2];
            if (board[a] != "" && board[a] == board[b] && board[b] == board[c])
                return board[a];
        }
        return null;
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

    // Chame isso a partir de um bot�o para reiniciar
    public void OnRestartButton()
    {
        StartGame();
        if( gameOption == GameOptions.BotVsBot)
        {
            StartCoroutine(BotVsBotGame());
        }
    }


    // (Opcional) Chame isso para zerar o placar
    public void OnResetScores()
    {
        scoreX = 0;
        scoreO = 0;
        UpdateScoreUI();
    }
}
