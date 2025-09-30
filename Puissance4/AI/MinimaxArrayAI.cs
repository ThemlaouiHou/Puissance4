using Puissance4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Puissance4.AI
{
    public class MinimaxArrayAI : IConnectFourAI
    {
        public string Name => "Minimax Array";
        public AIDifficulty Difficulty { get; }

        private readonly NodeState _player;
        private readonly int _maxDepth;
        private int _pruningCount = 0;
        private Dictionary<int, int> _moveScores = new();

        public MinimaxArrayAI(NodeState player, AIDifficulty difficulty = AIDifficulty.Medium)
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
            if (board is not ArrayBoard arrayBoard)
                throw new ArgumentException("MinimaxArrayAI requires ArrayBoard");

            // Get valid moves
            List<int> legalMoves = arrayBoard.GetAvailableColumns();
            if (!legalMoves.Any()) return -1;

            // Log setup
            Console.WriteLine($"\n[MinimaxArrayAI] Choosing move for player: {_player}, Depth: {_maxDepth}");
            Console.WriteLine("[MinimaxArrayAI] Legal moves: " + string.Join(", ", legalMoves));

            // Check if immediate win
            foreach (int move in legalMoves)
            {
                ArrayBoard testBoard = arrayBoard.MakeMove(move, _player);
                if (testBoard != null && testBoard.HasWinner() && testBoard.LastPlayer == _player)
                {
                    Console.WriteLine($"[MinimaxArrayAI] Immediate win move at column {move}");
                    return move;
                }
            }

            // Check if opponent immediate win and block
            NodeState opponent = _player == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            foreach (int move in legalMoves)
            {
                ArrayBoard testBoard = arrayBoard.MakeMove(move, opponent);
                if (testBoard != null && testBoard.HasWinner() && testBoard.LastPlayer == opponent)
                {
                    Console.WriteLine($"[MinimaxArrayAI] Block opponent at column {move}");
                    return move;
                }
            }

            // Start Minimax search
            int bestMove = legalMoves[0];
            int bestValue = int.MinValue;
            _moveScores.Clear();   // Reset scores
            _pruningCount = 0;     // Reset pruning counter

            foreach (int move in legalMoves)
            {
                ArrayBoard newBoard = arrayBoard.MakeMove(move, _player);
                if (newBoard == null) continue;

                // Run minimax from this board state
                int value = Minimax(newBoard, _maxDepth - 1, false, int.MinValue, int.MaxValue);
                _moveScores[move] = value;

                Console.WriteLine($"[MinimaxArrayAI] Move {move} evaluated with score {value}");

                if (value > bestValue)
                {
                    bestValue = value;
                    bestMove = move;
                }
            }

            // Display summary
            Console.WriteLine("[MinimaxArrayAI] Final move scores:");
            foreach (var kvp in _moveScores)
            {
                Console.WriteLine($"  Column {kvp.Key}: Score {kvp.Value}");
            }

            Console.WriteLine($"[MinimaxArrayAI] Total pruning operations: {_pruningCount}");
            Console.WriteLine($"[MinimaxArrayAI] Best move: {bestMove} with score {bestValue}");

            return bestMove;
        }

        private int Minimax(ArrayBoard board, int depth, bool maximizing, int alpha, int beta)
        {
            // Reached max depth or game has ended, evaluate the board
            if (depth == 0 || board.HasWinner() || board.IsFull())
            {
                int eval = board.EvaluatePosition(_player);
                return eval;
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
                    ArrayBoard newBoard = board.MakeMove(move, currentPlayer);
                    if (newBoard == null) continue;

                    // Recursive call minimax for the opponent (minimizing)
                    int eval = Minimax(newBoard, depth - 1, false, alpha, beta);
                    maxEval = Math.Max(maxEval, eval); // Choose the best eval so far
                    alpha = Math.Max(alpha, eval); // Update alpha

                    // Alpha-beta pruning: skip other branches if we can already do better
                    if (beta <= alpha)
                    {
                        _pruningCount++;
                        break;
                    }
                }

                return maxEval;
            }
            else // Minimizing player
            {
                int minEval = int.MaxValue;

                // Try every valid move for the minimizing player
                foreach (int move in board.GetAvailableColumns())
                {
                    ArrayBoard newBoard = board.MakeMove(move, currentPlayer);
                    if (newBoard == null) continue;

                    // Recursive call minimax for the opponent (maximizing)
                    int eval = Minimax(newBoard, depth - 1, true, alpha, beta);
                    minEval = Math.Min(minEval, eval); // Choose the worst eval so far
                    beta = Math.Min(beta, eval); // Update beta

                    // Alpha-beta pruning: skip other branches if we can already do worse
                    if (beta <= alpha)
                    {
                        _pruningCount++;
                        break;
                    }
                }

                return minEval;
            }
        }




    }
}
