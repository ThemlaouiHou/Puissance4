using Puissance4.AI;
using Puissance4.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Puissance4
{
    public partial class MenuPage : UserControl
    {
        private readonly MainWindow main;
        private MenuViewModel ViewModel { get; set; }

        public MenuPage(MainWindow main)
        {
            InitializeComponent();
            ViewModel = new MenuViewModel();
            DataContext = ViewModel;
            this.main = main;
            SwitchStep(MenuStep.GameType);
        }

        // Track user selection
        private GameMode selectedMode;
        private BoardMode selectedBoard;
        private MenuAIType selectedAI;
        private AIDifficulty selectedDifficulty;

        private enum MenuStep { GameType, BoardType, Algorithm, Difficulty, AIvAISetup }
        private enum GameMode { PvP, PvAI, AIvAI }
        private enum BoardMode { Array, Graph }
        private enum MenuAIType { Minimax, MonteCarlo}

        private void SwitchStep(MenuStep step)
        {
            ViewModel.GameTypePanelVisibility = step == MenuStep.GameType ? Visibility.Visible : Visibility.Collapsed;
            ViewModel.BoardTypePanelVisibility = step == MenuStep.BoardType ? Visibility.Visible : Visibility.Collapsed;
            ViewModel.AlgorithmPanelVisibility = step == MenuStep.Algorithm ? Visibility.Visible : Visibility.Collapsed;
            ViewModel.DifficultyPanelVisibility = step == MenuStep.Difficulty ? Visibility.Visible : Visibility.Collapsed;
            ViewModel.AIvsAIPanelVisibility = step == MenuStep.AIvAISetup ? Visibility.Visible : Visibility.Collapsed;
        }

        // === Game Mode Selection ===
        private void PvP_Click(object sender, RoutedEventArgs e)
        {
            selectedMode = GameMode.PvP;
            main.NavigateToGame(false);
        }

        private void PvAI_Click(object sender, RoutedEventArgs e)
        {
            selectedMode = GameMode.PvAI;
            SwitchStep(MenuStep.BoardType);
        }

        private void AIvAI_Click(object sender, RoutedEventArgs e)
        {
            selectedMode = GameMode.AIvAI;
            SwitchStep(MenuStep.AIvAISetup);
        }


        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Made by Jeddi Youssef, Themlaoui Houssem, Charpié Ludovic - 2025", "Credits");
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // === Board Type Selection ===
        private void BoardArray_Click(object sender, RoutedEventArgs e)
        {
            selectedBoard = BoardMode.Array;
            SwitchStep(MenuStep.Algorithm);
        }

        private void BoardGraph_Click(object sender, RoutedEventArgs e)
        {
            selectedBoard = BoardMode.Graph;
            SwitchStep(MenuStep.Algorithm);
        }

        private void BackToGameType_Click(object sender, RoutedEventArgs e)
        {
            SwitchStep(MenuStep.GameType);
        }

        // === AI Algorithm Selection ===
        private void Minimax_Click(object sender, RoutedEventArgs e)
        {
            selectedAI = MenuAIType.Minimax;
            SwitchStep(MenuStep.Difficulty);
        }

        private void MonteCarlo_Click(object sender, RoutedEventArgs e)
        {
            selectedAI = MenuAIType.MonteCarlo;
            SwitchStep(MenuStep.Difficulty);
        }

        private void BackToBoardType_Click(object sender, RoutedEventArgs e)
        {
            SwitchStep(MenuStep.BoardType);
        }

        // === Difficulty Selection ===
        private void DifficultyEasy_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = AIDifficulty.Easy;
            LaunchGame();
        }

        private void DifficultyMedium_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = AIDifficulty.Medium;
            LaunchGame();
        }

        private void DifficultyHard_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = AIDifficulty.Hard;
            LaunchGame();
        }

        private void BackToAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            SwitchStep(MenuStep.Algorithm);
        }

        // === Final Game Launch ===
        private void LaunchGame()
        {
            // Map menu selections to AI types
            Puissance4.AI.AIType actualAIType = MapToActualAIType(selectedAI, selectedBoard);
            bool useGraphBoard = selectedBoard == BoardMode.Graph;
            
            // Launch game based on mode
            switch (selectedMode)
            {
                case GameMode.PvP:
                    main.NavigateToGame(false, AIDifficulty.Medium, Puissance4.AI.AIType.MinimaxGraph, true);
                    break;
                case GameMode.PvAI:
                    main.NavigateToGame(true, selectedDifficulty, actualAIType, useGraphBoard);
                    break;
                case GameMode.AIvAI:
                    // Shouldn't be reached normally
                    MessageBox.Show("Use AI vs AI Setup panel for this mode!", "Information", MessageBoxButton.OK);
                    break;
            }
        }
        
        // === AI vs AI Setup ===
        private void StartAIVsAIGame_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedAI1Type == null || ViewModel.SelectedAI2Type == null)
            {
                MessageBox.Show("Please select AI types for both players!", "Selection Required", MessageBoxButton.OK);
                return;
            }

            // Get AI selections
            Puissance4.AI.AIType ai1Type = ViewModel.SelectedAI1Type.Value;
            Puissance4.AI.AIType ai2Type = ViewModel.SelectedAI2Type.Value;

            // Launch AI vs AI
            main.NavigateToAIvsAIGame(ai1Type, ai2Type);
        }


        private Puissance4.AI.AIType MapToActualAIType(MenuAIType menuAI, BoardMode board)
        {
            return (menuAI, board) switch
            {
                (MenuAIType.Minimax, BoardMode.Array) => Puissance4.AI.AIType.MinimaxArray,
                (MenuAIType.Minimax, BoardMode.Graph) => Puissance4.AI.AIType.MinimaxGraph,
                (MenuAIType.MonteCarlo, BoardMode.Array) => Puissance4.AI.AIType.MonteCarloArray,
                (MenuAIType.MonteCarlo, BoardMode.Graph) => Puissance4.AI.AIType.MonteCarloGraph,
                _ => Puissance4.AI.AIType.MinimaxGraph // Default fallback
            };
        }
    }
}