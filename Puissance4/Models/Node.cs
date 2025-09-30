namespace Puissance4.Models
{
    public enum NodeState 
    { 
        Empty, 
        Player, 
        Opponent 
    }
    public class Node
    {
        public int Row { get; }
        public int Col { get; }
        public NodeState State { get; set; }
        public List<Node> Neighbors { get; }

        public Node(int row, int col)
        {
            Row = row;
            Col = col;
            State = NodeState.Empty;
            Neighbors = new List<Node>();
        }
    }
}