using Puissance4.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Puissance4.AI
{
    public enum AIType
    {
        MinimaxArray,
        MinimaxGraph,
        MonteCarloArray,
        MonteCarloGraph
    }

    public class AIManager
    {
        private readonly Dictionary<AIType, Func<NodeState, AIDifficulty, IConnectFourAI>> _aiFactories;

        public AIManager()
        {
            _aiFactories = new Dictionary<AIType, Func<NodeState, AIDifficulty, IConnectFourAI>>
            {
                { AIType.MinimaxArray, (player, diff) => new MinimaxArrayAI(player, diff) },
                { AIType.MinimaxGraph, (player, diff) => new MinimaxGraphAI(player, diff) },
                { AIType.MonteCarloArray, (player, diff) => new MonteCarloArrayAI(player, diff) },
                { AIType.MonteCarloGraph, (player, diff) => new MonteCarloGraphAI(player, diff) },
            };
        }

        public IConnectFourAI CreateAI(AIType type, NodeState player, AIDifficulty difficulty = AIDifficulty.Medium)
        {
            if (_aiFactories.TryGetValue(type, out Func<NodeState, AIDifficulty, IConnectFourAI> factory))
            {
                return factory(player, difficulty);
            }

            throw new ArgumentException($"Unknown AI type: {type}");
        }

        public List<AIType> GetAllAITypes()
        {
            return Enum.GetValues<AIType>().ToList();
        }

        public string GetAIDescription(AIType type)
        {
            return type switch
            {
                AIType.MinimaxArray => "Minimax Algorithm with Alpha-Beta pruning using Array representation",
                AIType.MinimaxGraph => "Minimax Algorithm with graph-based analysis using Graph representation",
                AIType.MonteCarloArray => "Monte Carlo with random simulations using Array representation",
                AIType.MonteCarloGraph => "Monte Carlo with graph-based simulations",
                _ => "Unknown AI type"
            };
        }
    }
    

}