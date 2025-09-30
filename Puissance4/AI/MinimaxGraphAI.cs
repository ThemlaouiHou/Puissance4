using Puissance4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Puissance4.AI
{
    public class MinimaxGraphAI : IConnectFourAI
    {
        public string Name => "Minimax Graph";
        public AIDifficulty Difficulty { get; }

        private readonly NodeState _player;
        private readonly int _maxDepth;
        private int _pruningCount = 0;

        public MinimaxGraphAI(NodeState player, AIDifficulty difficulty = AIDifficulty.Medium)
        {
            _player = player;
            Difficulty = difficulty;
            _maxDepth = difficulty switch
            {
                AIDifficulty.Easy => 3,
                AIDifficulty.Medium => 5,
                AIDifficulty.Hard => 7,
                _ => 5
            };
        }

        public int ChooseMove(Board board)
        {
            if (board is not GraphBoard graphBoard)
                throw new ArgumentException("MinimaxGraphAI requires GraphBoard");

            // Get valid moves
            List<int> legalMoves = graphBoard.GetAvailableColumns();
            if (!legalMoves.Any()) return -1;

            // Log setup
            Console.WriteLine($"\n[MinimaxGraphAI] Choosing move for player: {_player}, Depth: {_maxDepth}");
            Console.WriteLine("[MinimaxGraphAI] Legal moves: " + string.Join(", ", legalMoves));

            // Check if immediate win
            foreach (int column in legalMoves)
            {
                if (IsWinningMove(graphBoard, column, _player))
                {
                    Console.WriteLine($"[MinimaxGraphAI] Immediate win at column {column}");
                    return column;
                }
            }

            // Check if opponent immediate win and block
            NodeState opponent = _player == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            foreach (int column in legalMoves)
            {
                if (IsWinningMove(graphBoard, column, opponent))
                {
                    Console.WriteLine($"[MinimaxGraphAI] Block opponent at column {column}");
                    return column;
                }
            }

            // Start Minimax search
            int bestMove = legalMoves[0];
            int bestValue = int.MinValue;
            Dictionary<int, int> moveScores = new Dictionary<int, int>();
            _pruningCount = 0; // Reset pruning counter

            foreach (int move in legalMoves)
            {
                GraphBoard newBoard = graphBoard.MakeMove(move, _player);
                if (newBoard == null) continue;

                // Evaluate the move with Minimax (opponent will move next)
                int value = Minimax(newBoard, _maxDepth - 1, false, int.MinValue, int.MaxValue);
                moveScores[move] = value;
                Console.WriteLine($"[MinimaxGraphAI] Move {move} evaluated with score {value}");

                // Update best move
                if (value > bestValue)
                {
                    bestValue = value;
                    bestMove = move;
                }
            }

            // Display summary 
            Console.WriteLine("[MinimaxGraphAI] --- Summary ---");
            foreach (var kvp in moveScores.OrderByDescending(kvp => kvp.Value))
            {
                Console.WriteLine($"  Column {kvp.Key} => Score: {kvp.Value}");
            }

            Console.WriteLine($"[MinimaxGraphAI] Total pruning operations: {_pruningCount}");
            Console.WriteLine($"[MinimaxGraphAI] Best move selected: {bestMove} with score {bestValue}");
            
            return bestMove;
        }


        private int Minimax(GraphBoard board, int depth, bool maximizing, int alpha, int beta)
        {
            // Reached max depth or game has ended, evaluate the board
            if (depth == 0 || board.HasWinner() || board.IsFull())
            {
                return board.Evaluate(_player);
            }

            // Determine current player based on whether we're maximizing or minimizing
            NodeState opponent = _player == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            NodeState currentPlayer = maximizing ? _player : opponent;

            if (maximizing)
            {
                int maxEval = int.MinValue;

                // Try every valid move for the maximizing player
                foreach (int move in board.GetAvailableColumns())
                {
                    GraphBoard newBoard = board.MakeMove(move, currentPlayer);
                    if (newBoard == null) continue;

                    // Recursive call minimax for the opponent (minimizing)
                    int eval = Minimax(newBoard, depth - 1, false, alpha, beta);

                    maxEval = Math.Max(maxEval, eval); // Choose the best eval so far
                    alpha = Math.Max(alpha, eval);     // Update alpha for pruning

                    // Alpha-Beta pruning: skip other branches if we can already do better
                    if (beta <= alpha)
                    {
                        _pruningCount++;
                        break;
                    }
                }

                return maxEval;
            }
            else // minimizing player
            {
                int minEval = int.MaxValue;

                // Try every valid move for the minimizing player
                foreach (int move in board.GetAvailableColumns())
                {
                    GraphBoard newBoard = board.MakeMove(move, currentPlayer);
                    if (newBoard == null) continue;

                    // Recursively call minimax for the opponent (maximizing)
                    int eval = Minimax(newBoard, depth - 1, true, alpha, beta);

                    minEval = Math.Min(minEval, eval); // Choose the worst eval so far
                    beta = Math.Min(beta, eval);       // Update beta for pruning

                    // Alpha-Beta pruning: skip other branches if we can already do worse
                    if (beta <= alpha)
                    {
                        _pruningCount++;
                        break;
                    }
                }

                return minEval;
            }
        }

        private bool IsWinningMove(GraphBoard board, int column, NodeState player)
        {
            GraphBoard testBoard = board.MakeMove(column, player);
            if (testBoard == null) return false;

            return testBoard.HasWinner() && testBoard.LastPlayer == player;
        }


    }
}