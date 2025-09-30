using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puissance4.Models
{
    public class ArrayBoard : Board
    {
        public NodeState[,] Grid { get; }
        public int MoveCount { get; }
        public int LastColumn { get; }
        public int LastRow { get; }
        
        public ArrayBoard(NodeState[,] grid, int moveCount = 0, NodeState lastPlayer = NodeState.Empty, int lastColumn = -1, int lastRow = -1)
        {
            Grid = (NodeState[,])grid.Clone();
            MoveCount = moveCount;
            LastPlayer = lastPlayer;
            LastColumn = lastColumn;
            LastRow = lastRow;
        }

        public override Board Clone()
        {
            return new ArrayBoard(Grid, MoveCount, LastPlayer, LastColumn, LastRow);
        }

        public override bool HasWinner()
        {
            if (LastRow == -1 || LastColumn == -1) return false;
            return CheckVictoryAtPosition(LastRow, LastColumn, LastPlayer);
        }

        public override bool IsFull()
        {
            return MoveCount >= Rows * Cols;
        }

        public override List<int> GetAvailableColumns()
        {
            List<int> moves = new List<int>();
            for (int c = 0; c < Cols; c++)
            {
                if (Grid[0, c] == NodeState.Empty)
                    moves.Add(c);
            }
            return moves;
        }
        
        public override KeyValuePair<int, int> ApplyMove(int column, NodeState state)
        {
            if (column < 0 || column >= Cols || Grid[0, column] != NodeState.Empty)
                return new KeyValuePair<int, int>(-1, -1);

            // Find the lowest empty row in the column
            for (int row = Rows - 1; row >= 0; row--)
            {
                if (Grid[row, column] == NodeState.Empty)
                {
                    Grid[row, column] = state;
                    return new KeyValuePair<int, int>(row, column);
                }
            }
            
            return new KeyValuePair<int, int>(-1, -1);
        }
        
        public override bool CheckVictory(int row, int column)
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Cols)
                return false;
            
            NodeState state = Grid[row, column];
            if (state == NodeState.Empty)
                return false;
                
            return CheckVictoryAtPosition(row, column, state);
        }
        
        public override int Evaluate(NodeState state)
        {
            return EvaluatePosition(state);
        }
        
        public override NodeState GetCellState(int row, int column)
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Cols)
                return NodeState.Empty;
            return Grid[row, column];
        }
        
        public override bool IsColumnFull(int column)
        {
            if (column < 0 || column >= Cols)
                return true;
            return Grid[0, column] != NodeState.Empty;
        }

        public ArrayBoard MakeMove(int column, NodeState player)
        {
            if (column < 0 || column >= Cols || Grid[0, column] != NodeState.Empty)
                return null;

            var newGrid = (NodeState[,])Grid.Clone();
            int row = -1;
            
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (newGrid[r, column] == NodeState.Empty)
                {
                    newGrid[r, column] = player;
                    row = r;
                    break;
                }
            }

            return row == -1 ? null : new ArrayBoard(newGrid, MoveCount + 1, player, column, row);
        }

        private bool CheckVictoryAtPosition(int row, int col, NodeState state)
        {
            (int, int)[] directions = new[] { (0, 1), (1, 0), (1, 1), (1, -1) };
            
            foreach (var (dr, dc) in directions)
            {
                int count = 1;
                count += CountInDirection(row, col, dr, dc, state);
                count += CountInDirection(row, col, -dr, -dc, state);
                
                if (count >= 4) return true;
            }
            
            return false;
        }

        private int CountInDirection(int row, int col, int dr, int dc, NodeState state)
        {
            int count = 0;
            int r = row + dr;
            int c = col + dc;
            
            while (r >= 0 && r < Rows && c >= 0 && c < Cols && Grid[r, c] == state)
            {
                count++;
                r += dr;
                c += dc;
            }
            
            return count;
        }

        public int EvaluatePosition(NodeState maximizingPlayer)
        {
            if (HasWinner())
            {
                return LastPlayer == maximizingPlayer ? 100000 : -100000;
            }

            if (IsFull())
            {
                return 0;
            }

            int score = 0;
            (int, int)[] directions = new[] { (0, 1), (1, 0), (1, 1), (1, -1) };

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == NodeState.Empty) continue;

                    foreach (var (dr, dc) in directions)
                    {
                        score += EvaluatePattern(r, c, dr, dc, maximizingPlayer);
                    }
                }
            }

            // Center column preference
            for (int r = 0; r < Rows; r++)
            {
                if (Grid[r, Cols / 2] == maximizingPlayer)
                    score += 3;
                else if (Grid[r, Cols / 2] != NodeState.Empty)
                    score -= 3;
            }

            return score;
        }

        private int EvaluatePattern(int row, int col, int dr, int dc, NodeState maximizingPlayer)
        {
            List<NodeState> window = new List<NodeState>();
            
            for (int i = 0; i < 4; i++)
            {
                int r = row + i * dr;
                int c = col + i * dc;
                
                if (r >= 0 && r < Rows && c >= 0 && c < Cols)
                    window.Add(Grid[r, c]);
                else
                    return 0;
            }

            return ScoreWindow(window, maximizingPlayer);
        }

        private int ScoreWindow(List<NodeState> window, NodeState maximizingPlayer)
        {
            int score = 0;
            NodeState opponent = maximizingPlayer == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            
            int playerCount = window.Count(s => s == maximizingPlayer);
            int opponentCount = window.Count(s => s == opponent);
            int emptyCount = window.Count(s => s == NodeState.Empty);

            if (playerCount == 4)
                score += 100;
            else if (playerCount == 3 && emptyCount == 1)
                score += 10;
            else if (playerCount == 2 && emptyCount == 2)
                score += 2;

            if (opponentCount == 3 && emptyCount == 1)
                score -= 80;

            return score;
        }
    }
}