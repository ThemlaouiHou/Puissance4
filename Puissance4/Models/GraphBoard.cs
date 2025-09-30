using Puissance4.AI;

namespace Puissance4.Models
{
    public class GraphBoard : Board
    {
        private readonly Node[,] grid;
        public int LastRow { get; set; } = -1;
        public int LastColumn { get; set; } = -1;

        public GraphBoard(bool initialize = true)
        {
            grid = new Node[Rows, Cols];
            if (initialize)
                InitializeBoard();
        }

        // Creates a deep copy of the board with the same node states and connections
        public override Board Clone()
        {
            var newBoard = new GraphBoard(false);

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var original = grid[row, col];
                    newBoard.grid[row, col] = new Node(row, col)
                    {
                        State = original.State
                    };
                }
            }

            newBoard.BuildGraph();
            newBoard.LastPlayer = this.LastPlayer;
            newBoard.LastRow = this.LastRow;
            newBoard.LastColumn = this.LastColumn;
            return newBoard;
        }

        // Initializes the board by instantiating all nodes and linking neighbors
        private void InitializeBoard()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    grid[row, col] = new Node(row, col);
                }
            }

            BuildGraph();
        }

        // Builds 8-directional neighbor links for each node (graph edges)
        public void BuildGraph()
        {
            int[] rowOffset = { 0, 1, -1, 1, -1, 0, 1, -1 };
            int[] colOffset = { 1, 1, 1, 0, 0, -1, -1, -1 };

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var currentNode = grid[row, col];
                    currentNode.Neighbors.Clear();

                    for (int i = 0; i < 8; i++)
                    {
                        int newRow = row + rowOffset[i];
                        int newCol = col + colOffset[i];

                        if (IsWithinBounds(newRow, newCol))
                        {
                            currentNode.Neighbors.Add(grid[newRow, newCol]);
                        }
                    }
                }
            }
        }

        private bool IsWithinBounds(int row, int col)
        {
            return row >= 0 && row < Rows && col >= 0 && col < Cols;
        }

        public override Node GetNode(int row, int col) => grid[row, col];

        public override NodeState GetCellState(int row, int column)
        {
            if (!IsWithinBounds(row, column))
                return NodeState.Empty;
            return grid[row, column].State;
        }

        // Get list of all nodes
        public List<Node> GetAllNodes()
        {
            var list = new List<Node>();
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    list.Add(grid[r, c]);
            return list;
        }

        // Checks win
        public override bool CheckVictory(int row, int col)
        {
            NodeState state = grid[row, col].State;
            return CountAlign(row, col, 1, 0, state) + CountAlign(row, col, -1, 0, state) > 2 ||  // vertical
                   CountAlign(row, col, 0, 1, state) + CountAlign(row, col, 0, -1, state) > 2 ||  // horizontal
                   CountAlign(row, col, 1, 1, state) + CountAlign(row, col, -1, -1, state) > 2 ||  // diagonal \
                   CountAlign(row, col, 1, -1, state) + CountAlign(row, col, -1, 1, state) > 2;    // diagonal /
        }

        // Counts number of aligned nodes in a given direction
        private int CountAlign(int row, int col, int dRow, int dCol, NodeState state)
        {
            int count = 0;
            int r = row + dRow;
            int c = col + dCol;

            while (IsWithinBounds(r, c) && grid[r, c].State == state)
            {
                count++;
                r += dRow;
                c += dCol;
            }

            return count;
        }

        public override bool IsFull()
        {
            for (int col = 0; col < Cols; col++)
                if (grid[0, col].State == NodeState.Empty)
                    return false;
            return true;
        }

        public override bool IsColumnFull(int column) => grid[0, column].State != NodeState.Empty;

        public override List<int> GetAvailableColumns()
        {
            List<int> columns = new List<int>();
            for (int c = 0; c < Cols; c++)
                if (!IsColumnFull(c))
                    columns.Add(c);
            return columns;
        }

        // winner check based on last move
        public override bool HasWinner()
        {
            if (LastRow != -1 && LastColumn != -1)
                return CheckVictory(LastRow, LastColumn);

            // Fallback full scan
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (grid[r, c].State != NodeState.Empty && CheckVictory(r, c))
                        return true;

            return false;
        }

        // Returns a clone with a move applied, or null if move invalid
        public GraphBoard MakeMove(int column, NodeState player)
        {
            GraphBoard clone = (GraphBoard)this.Clone();
            KeyValuePair<int, int> pos = clone.ApplyMove(column, player);
            if (pos.Key == -1)
                return null;

            clone.LastPlayer = player;
            clone.LastRow = pos.Key;
            clone.LastColumn = column;
            return clone;
        }

        // Applies a move to the lowest available cell in the column
        public override KeyValuePair<int, int> ApplyMove(int column, NodeState state)
        {
            if (column < 0 || column >= Cols)
                return new KeyValuePair<int, int>(-1, -1);

            for (int row = Rows - 1; row >= 0; row--)
            {
                if (grid[row, column].State == NodeState.Empty)
                {
                    grid[row, column].State = state;
                    LastPlayer = state;
                    LastRow = row;
                    LastColumn = column;
                    return new KeyValuePair<int, int>(row, column);
                }
            }

            return new KeyValuePair<int, int>(-1, -1); // Column full
        }

        // Heuristic evaluation of board state for a player
        public override int Evaluate(NodeState state)
        {
            int score = 0;
            (int, int)[] directions = new[] { (0, 1), (1, 0), (1, 1), (1, -1) }; // from top-left to bottom-right

            // Pattern-based scoring
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (grid[r, c].State == NodeState.Empty) continue;

                    foreach (var (dr, dc) in directions)
                        score += EvaluatePattern(r, c, dr, dc, state);
                }
            }

            // Center preference bonus
            for (int r = 0; r < Rows; r++)
            {
                if (grid[r, Cols / 2].State == state)
                    score += 3;
                else if (grid[r, Cols / 2].State != NodeState.Empty)
                    score -= 3;
            }

            // Graph-based bonus: size of largest connected component
            List<List<Node>> components = GetConnectedComponents(state);
            int maxChain = components.Count > 0 ? components.Max(group => group.Count) : 0;
            score += maxChain * 5;

            return score;
        }

        // Scans a 4-node segment and scores it
        private int EvaluatePattern(int row, int col, int dr, int dc, NodeState player)
        {
            // populate a window of 4 nodes
            List<NodeState> window = new List<NodeState>();
            for (int i = 0; i < 4; i++)
            {
                int r = row + i * dr;
                int c = col + i * dc;
                if (IsWithinBounds(r, c))
                    window.Add(grid[r, c].State);
                else
                    return 0;
            }

            return ScoreWindow(window, player);
        }

        // Assigns a score to a window based on how many player/opponent/empty cells it contains
        private int ScoreWindow(List<NodeState> window, NodeState player)
        {
            int score = 0;

            // determine opponent identity
            NodeState opponent = player == NodeState.Player ? NodeState.Opponent : NodeState.Player;

            // Count occurrences of player, opponent, and empty cells
            int playerCount = window.Count(s => s == player);
            int opponentCount = window.Count(s => s == opponent);
            int emptyCount = window.Count(s => s == NodeState.Empty);

            if (playerCount == 4) score += 100;
            else if (playerCount == 3 && emptyCount == 1) score += 10;
            else if (playerCount == 2 && emptyCount == 2) score += 2;

            if (opponentCount == 3 && emptyCount == 1) score -= 80;

            return score;
        }

        // Extracts all connected groups (components) of the same player nodes
        private List<List<Node>> GetConnectedComponents(NodeState state)
        {
            List<List<Node>> components = new List<List<Node>>();
            HashSet<Node> visited = new HashSet<Node>();

            foreach (var node in GetAllNodes())
            {
                if (node.State == state && !visited.Contains(node))
                {
                    List<Node> component = new List<Node>();
                    DFS(node, state, visited, component);
                    components.Add(component);
                }
            }

            return components;
        }

        // Graph DFS traversal to collect connected nodes
        private void DFS(Node node, NodeState state, HashSet<Node> visited, List<Node> component)
        {
            if (node == null || visited.Contains(node) || node.State != state) return;

            visited.Add(node);
            component.Add(node);

            foreach (var neighbor in node.Neighbors)
                DFS(neighbor, state, visited, component);
        }
    }
}
