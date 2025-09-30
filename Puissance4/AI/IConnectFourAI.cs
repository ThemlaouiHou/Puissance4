using Puissance4.Models;

namespace Puissance4.AI
{
    public enum AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public interface IConnectFourAI
    {
        string Name { get; }
        AIDifficulty Difficulty { get; }
        int ChooseMove(Board board);
    }
}