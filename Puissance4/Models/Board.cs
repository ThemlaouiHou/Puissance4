using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puissance4.Models
{
    public abstract class Board
    {
        public const int Rows = 6;
        public const int Cols = 7;
        public NodeState LastPlayer { get; set; } = NodeState.Empty;

        // Core board operations
        public abstract List<int> GetAvailableColumns();
        public abstract bool IsFull();
        public abstract bool HasWinner();
        public abstract Board Clone();
        
        // Movement operations
        public abstract KeyValuePair<int, int> ApplyMove(int column, NodeState state);
        public abstract bool CheckVictory(int row, int column);
        
        // Evaluation
        public abstract int Evaluate(NodeState state);
        
        // Cell access 
        public abstract NodeState GetCellState(int row, int column);
        public abstract bool IsColumnFull(int column);
        
        // Graph-specific methods
        public virtual Node GetNode(int row, int column)
        {
            throw new NotSupportedException("GetNode is only available for graph-based boards");
        }
    }
}
