using Puissance4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Puissance4.AI
{
    public class MonteCarloGraphAI : IConnectFourAI
    {
        public string Name => "Monte Carlo Graph";
        public AIDifficulty Difficulty { get; }

        private readonly NodeState _player;
        private readonly int _simulationCount;
        private readonly Random _random;

        public MonteCarloGraphAI(NodeState player, AIDifficulty difficulty = AIDifficulty.Medium)
        {
            _player = player;
            Difficulty = difficulty;

            // Adjust number of simulations depending on difficulty
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
            if (board is not GraphBoard graphBoard)
                throw new ArgumentException("MonteCarloGraphAI requires GraphBoard");

            // Get valid moves
            List<int> legalMoves = graphBoard.GetAvailableColumns();
            if (!legalMoves.Any()) return -1;


            Console.WriteLine($"\n[MonteCarloGraphAI] Starting simulations: {_simulationCount}, Player: {_player}");
            Console.WriteLine("[MonteCarloGraphAI] Legal moves: " + string.Join(", ", legalMoves));

            // Check immediate win
            foreach (int column in legalMoves)
            {
                if (IsWinningMove(graphBoard, column, _player))
                {
                    Console.WriteLine($"[MonteCarloGraphAI] Immediate win move at column {column}");
                    return column;
                }
            }

            // check opponent win and block 
            NodeState opponent = _player == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            foreach (int column in legalMoves)
            {
                if (IsWinningMove(graphBoard, column, opponent))
                {
                    Console.WriteLine($"[MonteCarloGraphAI] Block opponent at column {column}");
                    return column;
                }
            }

            // Run Monte Carlo simulations
            Dictionary<int, double> moveScores = new Dictionary<int, double>();
            int simsPerMove = _simulationCount / legalMoves.Count;

            foreach (int move in legalMoves)
            {
                double totalScore = 0.0;

                for (int i = 0; i < simsPerMove; i++)
                {
                    // Clone board, apply move, and simulate randomly
                    GraphBoard newBoard = graphBoard.MakeMove(move, _player);
                    if (newBoard != null)
                    {
                        double simResult = Simulate(newBoard);
                        totalScore += simResult;
                    }
                }

                double avgScore = totalScore / simsPerMove;
                moveScores[move] = avgScore;

                Console.WriteLine($"[MonteCarloGraphAI] Move {move}: Avg score = {avgScore:F3} over {simsPerMove} simulations");
            }

            Console.WriteLine("[MonteCarloGraphAI] Final move scores:");
            foreach (var kvp in moveScores.OrderByDescending(kvp => kvp.Value))
            {
                Console.WriteLine($"  Column {kvp.Key}: Score {kvp.Value:F3}");
            }

            // Choose move with the highest average score
            int bestMove = moveScores.OrderByDescending(kvp => kvp.Value).First().Key;
            Console.WriteLine($"[MonteCarloGraphAI] Best move: {bestMove} with score {moveScores[bestMove]:F3}");
            return bestMove;
        }

        /// <summary>
        /// Simulates a random game to completion from the given board state.
        /// Returns:
        ///   1.0 if AI wins
        ///  -1.0 if opponent wins
        ///   0.0 for draw or incomplete
        /// </summary>
        private double Simulate(GraphBoard board)
        {
            GraphBoard currentBoard = board.Clone() as GraphBoard;
            NodeState opponent = _player == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            NodeState currentPlayer = opponent;

            // immediate win just happened
            if (currentBoard.HasWinner() && currentBoard.LastPlayer == _player)
                return 1.0;

            // Simulate until the game ends
            while (!currentBoard.IsFull() && !currentBoard.HasWinner())
            {
                List<int> legalMoves = currentBoard.GetAvailableColumns();
                if (!legalMoves.Any()) break;

                int move = legalMoves[_random.Next(legalMoves.Count)];
                GraphBoard nextBoard = currentBoard.MakeMove(move, currentPlayer);
                if (nextBoard == null) break;

                currentBoard = nextBoard;

                // Check winner
                if (currentBoard.HasWinner())
                {
                    return currentBoard.LastPlayer == _player ? 1.0 : -1.0;
                }

                // Alternate turns
                currentPlayer = currentPlayer == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            }

            // No winner — use graph evaluate to get a score
            int scoreSelf = currentBoard.Evaluate(_player);
            int scoreOpponent = currentBoard.Evaluate(opponent);

            // Normalize to [-1, 1] range
            return (scoreSelf - scoreOpponent) / 100.0;
        }

        /// <summary>
        /// Checks whether playing in a given column would result in a win
        /// for the specified player.
        /// </summary>
        private bool IsWinningMove(GraphBoard board, int column, NodeState player)
        {
            GraphBoard testBoard = board.MakeMove(column, player);
            if (testBoard == null) return false;

            return testBoard.HasWinner() && testBoard.LastPlayer == player;
        }
    }
}
