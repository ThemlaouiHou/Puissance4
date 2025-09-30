using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Puissance4.AI;

namespace Puissance4.Models
{
    public class Game
    {
        private Board board;
        private NodeState Turn;
        public bool Ended;
        public bool UseGraphBoard { get; }

        private readonly IConnectFourAI? ai;
        private readonly AIManager aiManager;
        private readonly AIType aiType;

        public Game(AIDifficulty? difficulty = null, AIType aiType = AIType.MinimaxGraph, bool useGraphBoard = true)
        {
            UseGraphBoard = useGraphBoard;
            board = useGraphBoard ? new GraphBoard() : new ArrayBoard(new NodeState[Board.Rows, Board.Cols]);
            Turn = NodeState.Player;
            Ended = false;
            aiManager = new AIManager();
            this.aiType = aiType;

            if (difficulty.HasValue)
            {
                ai = aiManager.CreateAI(aiType, NodeState.Opponent, difficulty.Value);
            }
        }

        public void Reset()
        {
            board = UseGraphBoard ? new GraphBoard() : new ArrayBoard(new NodeState[Board.Rows, Board.Cols]);
            Turn = NodeState.Player;
            Ended = false;
        }

        public bool IsEnded()
        {
            return Ended;
        }

        public bool IsColumnFull(int column)
        {
            return board.IsColumnFull(column);
        }

        public bool IsBoardFull()
        {
            Ended = board.IsFull();
            return Ended;
        }

        public int GetCurrentPlayerNumber()
        {
            return Turn == NodeState.Player ? 1 : 2;
        }

        public Brush GetCurrentPlayerColor()
        {
            return Turn == NodeState.Player ? Brushes.Red : Brushes.Yellow;
        }

        public void NextTurn()
        {
            Turn = Turn == NodeState.Player ? NodeState.Opponent : NodeState.Player;
        }

        public bool CheckVictory(int row, int column)
        {
            Ended = board.CheckVictory(row, column);
            return Ended;
        }

        public KeyValuePair<int, int> ApplyMove(int column, NodeState state)
        {
            KeyValuePair<int, int> move = board.ApplyMove(column, state);

            if (state == NodeState.Player)
            {
                Console.WriteLine($"[Player] Chose column {column} -> row {move.Key}");
            }
            else if (ai != null)
            {
                Console.WriteLine($"[AI: {aiType}] Chose column {column} -> row {move.Key}");
            }

            return move;
        }

        public Node GetNode(int row, int column)
        {
            return board.GetNode(row, column);
        }
        
        public NodeState GetCellState(int row, int column)
        {
            return board.GetCellState(row, column);
        }

        // AI applies strategy
        public KeyValuePair<int, int> AIPlay()
        {
            // Player vs AI mode
            if (ai == null)
                throw new InvalidOperationException("AI not enabled.");

            if (Ended || board.IsFull())
                return new KeyValuePair<int, int>(-1, -1);

            int bestColumn = ai.ChooseMove(board);

            if (bestColumn == -1)
            {
                Ended = true;
                return new KeyValuePair<int, int>(-1, -1);
            }

            return ApplyMove(bestColumn, NodeState.Opponent);
        }

        public NodeState GetTurn()
        {
            return Turn;
        }

        public int Evaluate()
        {
            return board.Evaluate(NodeState.Opponent);
        }
        
        public Board GetBoard()
        {
            return board;
        }
    }
}
