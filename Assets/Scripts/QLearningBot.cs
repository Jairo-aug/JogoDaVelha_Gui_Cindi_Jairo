using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QLearningBot
{
    private Dictionary<string, Dictionary<int, double>> qTable = new();
    private System.Random rnd = new();
    private float learningRate = 0.1f;
    private float discountFactor = 0.95f;
    private float explorationRate = 0.1f;

    public int ChooseAction(string[] board, string player)
    {
        string state = BoardToString(board);
        var actions = GetAvailableMoves(board);

        if (!qTable.ContainsKey(state))
            qTable[state] = actions.ToDictionary(a => a, a => 0.0);

        // Exploração ou exploração
        if (Random.value < explorationRate)
            return actions[rnd.Next(actions.Count)];

        // Escolher ação com maior valor Q
        return qTable[state].OrderByDescending(x => x.Value).First().Key;
    }

    public void Train(int episodes)
    {
        for (int ep = 0; ep < episodes; ep++)
        {
            List<string> states = new();
            List<int> actions = new();
            string[] board = new string[9];
            string currentPlayer = "X";

            while (true)
            {
                string state = BoardToString(board);
                var possibleActions = GetAvailableMoves(board);
                if (possibleActions.Count == 0) break;

                if (!qTable.ContainsKey(state))
                    qTable[state] = possibleActions.ToDictionary(a => a, a => 0.0);

                int action = ChooseAction(board, currentPlayer);
                string[] nextBoard = (string[])board.Clone();
                nextBoard[action] = currentPlayer;

                states.Add(state);
                actions.Add(action);

                string winner = CheckWinner(nextBoard);
                double reward = 0;
                if (winner == currentPlayer)
                    reward = 1;
                else if (winner != null)
                    reward = -1;
                else if (!nextBoard.Contains("")) // empate
                    reward = 0.5;

                if (winner != null || !nextBoard.Contains(""))
                {
                    for (int i = states.Count - 1; i >= 0; i--)
                    {
                        var s = states[i];
                        var a = actions[i];
                        double oldQ = qTable[s][a];
                        qTable[s][a] = oldQ + learningRate * (reward - oldQ);
                        reward *= discountFactor;
                    }
                    break;
                }

                board = nextBoard;
                currentPlayer = currentPlayer == "X" ? "O" : "X";
            }
        }
    }

    private List<int> GetAvailableMoves(string[] board)
    {
        var moves = new List<int>();
        for (int i = 0; i < 9; i++)
            if (board[i] == "") moves.Add(i);
        return moves;
    }

    public string BoardToString(string[] board)
    {
        return string.Join("", board.Select(cell => cell == "" ? "_" : cell));
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

    public void UpdateQTable(List<string> states, List<int> actions, double finalReward)
    {
        for (int i = states.Count - 1; i >= 0; i--)
        {
            string s = states[i];
            int a = actions[i];

            if (!qTable.ContainsKey(s))
                qTable[s] = new Dictionary<int, double>();

            if (!qTable[s].ContainsKey(a))
                qTable[s][a] = 0;

            double oldQ = qTable[s][a];
            qTable[s][a] = oldQ + learningRate * (finalReward - oldQ);
            finalReward *= discountFactor;
        }
    }

    public void SaveQTable() { /* JSON ou PlayerPrefs */ }

    public void LoadQTable() { /* JSON ou PlayerPrefs */ }
}
