using System;
using System.Threading.Tasks;
using Puissance4.AI;

namespace Puissance4.Models
{
    public class AIvsAIGame
    {
        private Board board1; // For AI1
        private Board board2; // For AI2
        private NodeState currentTurn;
        public bool Ended { get; set; }

        private readonly IConnectFourAI ai1;
        private readonly IConnectFourAI ai2;
        private readonly AIType ai1Type;
        private readonly AIType ai2Type;
        private readonly AIManager aiManager;

        // Events for UI updates
        public event Action<int, int, bool>? OnAIMove; // row, column, isPlayerOne
        public event Action<string, bool>? OnGameEnd; // message, isDraw

        public AIvsAIGame(AIType ai1Type, AIType ai2Type)
        {
            this.ai1Type = ai1Type;
            this.ai2Type = ai2Type;
            aiManager = new AIManager();

            // Create separate boards for each AI
            board1 = RequiresGraphBoard(ai1Type) ? new GraphBoard() : new ArrayBoard(new NodeState[Board.Rows, Board.Cols]);
            board2 = RequiresGraphBoard(ai2Type) ? new GraphBoard() : new ArrayBoard(new NodeState[Board.Rows, Board.Cols]);

            currentTurn = NodeState.Player;
            Ended = false;

            ai1 = aiManager.CreateAI(ai1Type, NodeState.Player, GetMaxDifficulty(ai1Type));
            ai2 = aiManager.CreateAI(ai2Type, NodeState.Opponent, GetMaxDifficulty(ai2Type));
        }

        private bool RequiresGraphBoard(AIType aiType)
        {
            return aiType == AIType.MinimaxGraph ||
                   aiType == AIType.MonteCarloGraph;
        }

        private AIDifficulty GetMaxDifficulty(AIType aiType)
        {
            return aiType switch
            {
                AIType.MinimaxArray or AIType.MinimaxGraph => AIDifficulty.Hard, // Depth 7
                AIType.MonteCarloArray or AIType.MonteCarloGraph => AIDifficulty.Hard, // 10k simulations
                _ => AIDifficulty.Hard
            };
        }

        public async Task StartGameAsync()
        {
            // Small delay to let UI initialize
            await Task.Delay(1000);

            while (!Ended && !board1.IsFull() && !board2.IsFull())
            {
                if (Ended) break;

                bool isPlayerOne = currentTurn == NodeState.Player;
                IConnectFourAI currentAI = isPlayerOne ? ai1 : ai2;
                Board currentBoard = isPlayerOne ? board1 : board2;

                // AI makes move
                int column = currentAI.ChooseMove(currentBoard);

                if (column == -1 || IsColumnFull(column))
                {
                    // No valid move available
                    if (IsBoardFull())
                    {
                        OnGameEnd?.Invoke("Match nul !", true);
                    }
                    break;
                }

                // Apply move to both boards to keep them synchronized
                KeyValuePair<int, int> move1 = board1.ApplyMove(column, currentTurn);
                KeyValuePair<int, int> move2 = board2.ApplyMove(column, currentTurn);

                if (move1.Key == -1 || move2.Key == -1)
                {
                    // Move failed
                    break;
                }

                // Use the first board's result for UI
                int row = move1.Key;

                // Notify UI of the move
                OnAIMove?.Invoke(row, column, isPlayerOne);

                // Wait for UI to update
                await Task.Delay(1000);

                // Check victory on both boards (should be consistent)
                if (board1.CheckVictory(row, column) || board2.CheckVictory(row, column))
                {
                    Ended = true;
                    string message = $"Victoire du Joueur {(isPlayerOne ? 1 : 2)} !";
                    OnGameEnd?.Invoke(message, false);
                    break;
                }

                // Check draw
                if (IsBoardFull())
                {
                    Ended = true;
                    OnGameEnd?.Invoke("Match nul !", true);
                    break;
                }

                // Next turn
                currentTurn = currentTurn == NodeState.Player ? NodeState.Opponent : NodeState.Player;
            }
        }

        private bool IsColumnFull(int column)
        {
            return board1.IsColumnFull(column) || board2.IsColumnFull(column);
        }

        private bool IsBoardFull()
        {
            return board1.IsFull() || board2.IsFull();
        }

        public NodeState GetCellState(int row, int column)
        {
            // Use board1 as reference (identical as board2 normally)
            return board1.GetCellState(row, column);
        }

        public int GetCurrentPlayerNumber()
        {
            return currentTurn == NodeState.Player ? 1 : 2;
        }

        public void Reset()
        {
            board1 = RequiresGraphBoard(ai1Type) ? new GraphBoard() : new ArrayBoard(new NodeState[Board.Rows, Board.Cols]);
            board2 = RequiresGraphBoard(ai2Type) ? new GraphBoard() : new ArrayBoard(new NodeState[Board.Rows, Board.Cols]);
            currentTurn = NodeState.Player;
            Ended = false;
        }
    }
}