using Puissance4.AI;
using Puissance4.Models;
using System.Windows;
using System.Windows.Controls;

namespace Puissance4
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new MenuPage(this));
        }

        public void NavigateToGame(bool withAI, AIDifficulty difficulty = AIDifficulty.Medium, AIType aiType = AIType.MinimaxGraph, bool useGraphBoard = true)
        {
            MainFrame.Navigate(new GamePage(withAI, difficulty, aiType, useGraphBoard));
        }

        public void NavigateToAIvsAIGame(AIType ai1Type, AIType ai2Type)
        {
            MainFrame.Navigate(new GamePage(ai1Type, ai2Type));
        }

    }
}

