using Puissance4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Puissance4.AI
{
    public class MonteCarloArrayAI : IConnectFourAI
    {
        public string Name => "Monte Carlo Array";
        public AIDifficulty Difficulty { get; }

        private readonly NodeState _player;
        private readonly int _simulationCount;
        private readonly Random _random;

        public MonteCarloArrayAI(NodeState player, AIDifficulty difficulty = AIDifficulty.Medium)
        {
            _player = player;
            Difficulty = difficulty;

            // Number of simulations with difficulty
            _simulationCount = difficulty switch
            {
                AIDifficulty.Easy => 1000,
                AIDifficulty.Medium => 5000,
                AIDifficulty.Hard => 10000,
                _ => 5000
            };

            _random = new Random();
        }

        public int ChooseMove(Board board)
        {
            if (board is not ArrayBoard arrayBoard)
                throw new ArgumentException("MonteCarloArrayAI requires ArrayBoard");

            // Get valid moves
            List<int> legalMoves = arrayBoard.GetAvailableColumns();
            if (!legalMoves.Any()) return -1;

            Console.WriteLine($"\n[MonteCarloArrayAI] Starting simulations: {_simulationCount}, Player: {_player}");
            Console.WriteLine("[MonteCarloArrayAI] Legal moves: " + string.Join(", ", legalMoves));

            Dictionary<int, double> moveScores = new Dictionary<int, double>();

            // Distribute simulations equally
            int simulationsPerMove = _simulationCount / legalMoves.Count;

            foreach (int move in legalMoves)
            {
                double totalScore = 0.0;

                for (int i = 0; i < simulationsPerMove; i++)
                {
                    // Simulate this move by applying it and running a random game from that state
                    ArrayBoard newBoard = arrayBoard.MakeMove(move, _player);
                    if (newBoard != null)
                    {
                        double result = Simulate(newBoard);
                        totalScore += result;
                    }
                }

                // Average score = expected value of this move 
                double avgScore = totalScore / simulationsPerMove;
                moveScores[move] = avgScore;
                Console.WriteLine($"[MonteCarloArrayAI] Move {move}: Avg score = {avgScore:F3} ({totalScore:F1}/{simulationsPerMove})");
            }

            Console.WriteLine("[MonteCarloArrayAI] Summary:");
            foreach (var kvp in moveScores.OrderByDescending(kvp => kvp.Value))
            {
                Console.WriteLine($"  Column {kvp.Key}: {kvp.Value:F3}");
            }

            // Choose move with the highest average score
            int bestMove = moveScores.OrderByDescending(kvp => kvp.Value).First().Key;
            Console.WriteLine($"[MonteCarloArrayAI] Best move: {bestMove} with score {moveScores[bestMove]:F3}");
            return bestMove;
        }

        /// <summary>
        /// Simulates a full random game starting from the given board.
        /// Returns:
        ///   1.0 if this AI wins
        ///  -1.0 if the opponent wins
        ///   0.0 if draw or no winner
        /// </summary>
        private double Simulate(ArrayBoard board)
        {
            ArrayBoard currentBoard = board.Clone() as ArrayBoard;

            // Alternate turns between players; start with the opponent
            NodeState opponent = _player == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            NodeState currentPlayer = opponent;

            // immediate win already happened
            if (currentBoard.HasWinner() && currentBoard.LastPlayer == _player)
                return 1.0;

            while (!currentBoard.IsFull() && !currentBoard.HasWinner())
            {
                List<int> legalMoves = currentBoard.GetAvailableColumns();
                if (!legalMoves.Any()) break;

                // Choose a random legal move
                int move = legalMoves[_random.Next(legalMoves.Count)];

                // Apply the move
                ArrayBoard nextBoard = currentBoard.MakeMove(move, currentPlayer);
                if (nextBoard == null) break;

                currentBoard = nextBoard;

                // Check for winner
                if (currentBoard.HasWinner())
                {
                    // AI wins = +1, Opponent wins = -1
                    return currentBoard.LastPlayer == _player ? 1.0 : -1.0;
                }

                // Switch to the other player
                currentPlayer = currentPlayer == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            }

            // Game ended in draw or incomplete
            return 0.0;
        }
    }
}