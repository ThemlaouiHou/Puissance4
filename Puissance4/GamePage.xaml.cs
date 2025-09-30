using Puissance4.AI;
using Puissance4.Models;
using Puissance4.ViewModels;
using System;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Puissance4
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private GameViewModel ViewModel { get; set; }
        
        public GamePage(bool withAI, AIDifficulty difficulty = AIDifficulty.Medium, AIType aiType = AIType.MinimaxGraph, bool useGraphBoard = true)
        {
            InitializeComponent();
            ViewModel = new GameViewModel();
            DataContext = ViewModel;

            AIOpponent = withAI;
            Game = withAI ? new Game(difficulty, aiType, useGraphBoard) : new Game(null, aiType, useGraphBoard);

            // Store settings for replay
            isAIvsAIMode = false;
            this.aiDifficulty = withAI ? difficulty : null;
            this.aiType = aiType;
            this.useGraphBoard = useGraphBoard;

            // Subscribe to page unload event for cleanup
            this.Unloaded += GamePage_Unloaded;

            InitArrows();
            InitGrid();
            UpdateMatchupDisplay();
        }

        // Constructor for AI vs AI mode
        public GamePage(AIType ai1Type, AIType ai2Type)
        {
            InitializeComponent();
            ViewModel = new GameViewModel();
            DataContext = ViewModel;

            AIOpponent = true; // AI vs AI counts as AI opponent for UI
            AIvsAIGame = new AIvsAIGame(ai1Type, ai2Type);
            Game = new Game(); // Default game for UI methods compatibility

            // Store settings for replay
            isAIvsAIMode = true;
            this.ai1Type = ai1Type;
            this.ai2Type = ai2Type;

            InitArrows();
            InitAIvsAIGrid();
            UpdateMatchupDisplay();
            
            // Subscribe to AI vs AI move events
            AIvsAIGame.OnAIMove += HandleAIMove;
            AIvsAIGame.OnGameEnd += HandleGameEnd;
            SetArrowButtonsEnabled(false); // Disable player interaction

            this.Unloaded += GamePage_Unloaded;
            
            // Start
            _ = AIvsAIGame.StartGameAsync();
        }

        private Game Game = new();
        private AIvsAIGame? AIvsAIGame;
        private bool down = true;
        private bool AIOpponent = false;

        // Game settings for replay
        private bool isAIvsAIMode = false;
        private AIDifficulty? aiDifficulty;
        private AIType? aiType;
        private bool useGraphBoard = true;
        private AIType? ai1Type;
        private AIType? ai2Type;
        
        // Display names for UI
        private string player1Name = "Player";
        private string player2Name = "Player";

        private readonly SoundPlayer dropSound = new("Assets/Sounds/drop.wav");
        private readonly SoundPlayer winSound = new("Assets/Sounds/win.wav");
        private readonly SoundPlayer tacSound = new("Assets/Sounds/tac.wav");
        private readonly SoundPlayer drawSound = new("Assets/Sounds/draw.wav");


        private void InitArrows()
        {
            ArrowPanel.Children.Clear();
            for (int col = 0; col < Board.Cols; col++)
            {
                Button arrow = new()
                {
                    Content = "▼",
                    Tag = col,
                    Style = (Style)FindResource("ArrowButtonStyle")
                };
                arrow.Click += ColumnButton_Click;
                ArrowPanel.Children.Add(arrow);
            }
            UpdateArrowColors();
        }

        private void InitGrid()
        {
            InitializeGameGrid(true);
        }

        private void InitAIvsAIGrid()
        {
            InitializeGameGrid(false);
        }

        private void InitializeGameGrid(bool enableArrows)
        {
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            if (enableArrows)
            {
                Game.Reset();
                SetArrowButtonsEnabled(true);
            }
            else
            {
                AIvsAIGame?.Reset();
                SetArrowButtonsEnabled(false);
            }

            for (int r = 0; r < Board.Rows; r++)
                GameGrid.RowDefinitions.Add(new RowDefinition());

            for (int c = 0; c < Board.Cols; c++)
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int row = 0; row < Board.Rows; row++)
            {
                for (int col = 0; col < Board.Cols; col++)
                {
                    Border border = new()
                    {
                        Margin = new Thickness(3),
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(1)
                    };

                    Ellipse disc = new()
                    {
                        Fill = Brushes.White,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };

                    border.Child = disc;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    GameGrid.Children.Add(border);
                }
            }

            UpdateTurnDisplay();
            UpdateArrowColors();
            ViewModel.ReplayButtonVisibility = Visibility.Collapsed;
            StopReplayAnimation();
        }

        private void ColumnButton_Click(object sender, RoutedEventArgs e)
        {
            if (Game.IsEnded() || (AIOpponent && Game.GetTurn() != NodeState.Player))
            {
                return;
            }
            int column = (int)((Button)sender).Tag;

            if (Game.IsColumnFull(column))
            {
                return;
            }

            SetArrowButtonsEnabled(false);

            for (int row = Board.Rows - 1; row >= 0; row--)
            {
                if (Game.GetCellState(row, column) == NodeState.Empty)
                {
                    AnimateDiscDrop(row, column, Game.GetCurrentPlayerNumber() == 1);
                    break;
                }
            }
        }

        private void SetArrowButtonsEnabled(bool isEnabled)
        {
            foreach (Button arrow in ArrowPanel.Children)
                arrow.IsEnabled = isEnabled;
        }

        private void AnimateDiscDrop(int targetRow, int targetCol, bool isPlayerOne)
        {
            double cellWidth = GameGrid.ActualWidth / Board.Cols;
            double cellHeight = GameGrid.ActualHeight / Board.Rows;
            double x = targetCol * cellWidth;
            double yStart = -cellHeight;
            double yEnd = targetRow * cellHeight;

            Ellipse animatedDisc = new Ellipse
            {
                Width = cellWidth - 6,
                Height = cellHeight - 6,
                Fill = isPlayerOne ? Brushes.Red : Brushes.Yellow
            };

            Canvas.SetLeft(animatedDisc, x + 7);
            Canvas.SetTop(animatedDisc, yStart);
            AnimationCanvas.Children.Add(animatedDisc);

            DoubleAnimation dropAnim = new DoubleAnimation
            {
                From = yStart,
                To = yEnd + (targetRow == Board.Rows - 1 ? -10 : 0),
                Duration = TimeSpan.FromMilliseconds((100 * targetRow) + 200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            if (targetRow == Board.Rows - 1)
            {
                down = true;
                DoubleAnimation bounce = new DoubleAnimation
                {
                    From = yEnd - 10,
                    To = yEnd,
                    Duration = TimeSpan.FromMilliseconds(200),
                    BeginTime = TimeSpan.FromMilliseconds((100 * targetRow) + 100),
                    EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2 }
                };

                Storyboard sb = new Storyboard();
                sb.Children.Add(dropAnim);
                sb.Children.Add(bounce);

                Storyboard.SetTarget(dropAnim, animatedDisc);
                Storyboard.SetTargetProperty(dropAnim, new PropertyPath("(Canvas.Top)"));
                Storyboard.SetTarget(bounce, animatedDisc);
                Storyboard.SetTargetProperty(bounce, new PropertyPath("(Canvas.Top)"));

                sb.Completed += (s, e) =>
                {
                    dropSound.Play();
                    AnimationCanvas.Children.Remove(animatedDisc);

                    PlaceDisc(targetRow, targetCol, isPlayerOne);
                };

                sb.Begin();
            }
            else
            {
                dropAnim.Completed += (s, e) =>
                {
                    AnimationCanvas.Children.Remove(animatedDisc);

                    PlaceDisc(targetRow, targetCol, isPlayerOne);
                };

                animatedDisc.BeginAnimation(Canvas.TopProperty, dropAnim);
            }
        }

        private void PlaceDisc(int row, int col, bool isPlayerOne)
        {
            // Update visual disc
            Border border = (Border)GameGrid.Children
                .Cast<UIElement>()
                .First(el => Grid.GetRow(el) == row && Grid.GetColumn(el) == col);

            Ellipse disc = (Ellipse)border.Child;
            disc.Fill = isPlayerOne ? Brushes.Red : Brushes.Yellow;

            if (down == false)
            {
                tacSound.Play();
            }
            else
            {
                dropSound.Play();
            }

            // In AI vs AI mode, game logic is handled by AIvsAIGame
            if (AIvsAIGame != null)
            {
                // AI vs AI mode - just update visuals
                down = false;
                return;
            }

            // Regular game mode logic
            if (!AIOpponent)
            {
                // PvP mode
                Game.ApplyMove(col, isPlayerOne ? NodeState.Player : NodeState.Opponent);
            }
            else if (isPlayerOne)
            {
                // PvAI mode - only apply move for player
                Game.ApplyMove(col, NodeState.Player);
            }

            if (Game.CheckVictory(row, col))
            {
                winSound.Play();
                ShowFirework();
                string winnerName = isPlayerOne ? player1Name : player2Name;
                ViewModel.StatusText = $"{winnerName} wins !";
                ViewModel.ReplayButtonVisibility = Visibility.Visible;
                StartReplayAnimation();
            }
            else if (Game.IsBoardFull())
            {
                ViewModel.StatusText = "Draw !";
                drawSound.Play();
                ViewModel.ReplayButtonVisibility = Visibility.Visible;
                StartReplayAnimation();
            }
            else
            {
                Game.NextTurn();
                if (Game.GetTurn() == NodeState.Player || !AIOpponent)
                {
                    SetArrowButtonsEnabled(true);
                }
                else
                {
                    if (Game.IsEnded())
                    {
                        return;
                    }

                    KeyValuePair<int, int> play = Game.AIPlay();

                    if (play.Key == -1 || play.Value == -1)
                    {
                        
                        if (Game.IsBoardFull())
                        {
                            ViewModel.StatusText = "Draw !";
                            drawSound.Play();
                            ViewModel.ReplayButtonVisibility = Visibility.Visible;
                            StartReplayAnimation();
                        }
                        else if (Game.IsEnded())
                        {
                            ViewModel.StatusText = $"{player1Name} wins !";
                            winSound.Play();
                            ShowFirework();
                            ViewModel.ReplayButtonVisibility = Visibility.Visible;
                            StartReplayAnimation();
                        }
                        
                        return;
                    }

                    AnimateDiscDrop(play.Key, play.Value, false);
                }
                UpdateTurnDisplay();
                UpdateArrowColors();
            }

            down = false;
        }

        private void ShowFirework()
        {
            Random rnd = new Random();

            for (int i = 0; i < 50; i++)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = rnd.Next(10, 20),
                    Height = rnd.Next(10, 20),
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)rnd.Next(180, 256),
                        (byte)rnd.Next(100, 256),
                        (byte)rnd.Next(100, 256)))
                };

                // Starting position at the center
                double centerX = GameGrid.TranslatePoint(new Point(0, 0), this).X + GameGrid.ActualWidth / 2;
                double centerY = GameGrid.TranslatePoint(new Point(0, 0), this).Y + GameGrid.ActualHeight / 2;

                double angle = rnd.NextDouble() * 2 * Math.PI;
                double radius = rnd.Next(350, 500); // explosion radius

                double targetX = centerX + radius * Math.Cos(angle);
                double targetY = centerY + radius * Math.Sin(angle);

                Canvas.SetLeft(ellipse, centerX);
                Canvas.SetTop(ellipse, centerY);
                FireworkCanvas.Children.Add(ellipse);

                DoubleAnimation animX = new DoubleAnimation
                {
                    To = targetX,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                };

                DoubleAnimation animY = new DoubleAnimation
                {
                    To = targetY,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                };

                DoubleAnimation fade = new DoubleAnimation
                {
                    To = 0,
                    BeginTime = TimeSpan.FromMilliseconds(500),
                    Duration = TimeSpan.FromMilliseconds(1000),
                    FillBehavior = FillBehavior.Stop
                };

                fade.Completed += (s, e) => FireworkCanvas.Children.Remove(ellipse);

                ellipse.BeginAnimation(Canvas.LeftProperty, animX);
                ellipse.BeginAnimation(Canvas.TopProperty, animY);
                ellipse.BeginAnimation(OpacityProperty, fade);
            }
        }

        private void UpdateArrowColors()
        {
            foreach (Button arrow in ArrowPanel.Children)
                arrow.Background = Game.GetCurrentPlayerColor();
        }

        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            StopReplayAnimation();
            ViewModel.ReplayButtonVisibility = Visibility.Collapsed;
            
            // Restart game with same settings
            RestartGameWithSameSettings();
        }

        private void RestartGameWithSameSettings()
        {
            CleanupGame();
            
            if (isAIvsAIMode)
            {
                // Restart AI vs AI game with same AI types
                if (ai1Type.HasValue && ai2Type.HasValue)
                {
                    AIvsAIGame = new AIvsAIGame(ai1Type.Value, ai2Type.Value);
                    
                    AIvsAIGame.OnAIMove += HandleAIMove;
                    AIvsAIGame.OnGameEnd += HandleGameEnd;
                    
                    InitAIvsAIGrid();
                    SetArrowButtonsEnabled(false);
                    UpdateMatchupDisplay();
                    
                    _ = AIvsAIGame.StartGameAsync();
                }
            }
            else
            {
                // Restart regular game (PvP or PvAI) with same settings
                if (aiType.HasValue)
                {
                    Game = AIOpponent && aiDifficulty.HasValue 
                        ? new Game(aiDifficulty.Value, aiType.Value, useGraphBoard)
                        : new Game(null, aiType.Value, useGraphBoard);
                }
                else
                {
                    Game = new Game();
                }
                
                InitGrid();
                SetArrowButtonsEnabled(true);
                UpdateMatchupDisplay();
            }
        }

        private void StartReplayAnimation()
        {
            Storyboard sb = ((Storyboard)FindResource("PulseAnimation")).Clone();
            Storyboard.SetTarget(sb, ReplayButton);
            sb.Begin();
        }

        private void StopReplayAnimation()
        {
            Storyboard sb = (Storyboard)FindResource("PulseAnimation");
            sb.Stop();
        }


        private void MIResetGame_Click(object sender, RoutedEventArgs e)
        {
            StopReplayAnimation();
            ViewModel.ReplayButtonVisibility = Visibility.Collapsed;
            RestartGameWithSameSettings();
        }

        private void MIAbout_Click(object sender, RoutedEventArgs e)
        {
            string gameMode;
            string aiInfo = "";
            
            if (isAIvsAIMode && ai1Type.HasValue && ai2Type.HasValue)
            {
                gameMode = "AI vs AI Mode";
                aiInfo = $"\nPlayer 1: {GetAIDisplayName(ai1Type.Value)}\nPlayer 2: {GetAIDisplayName(ai2Type.Value)}";
            }
            else if (AIOpponent && aiType.HasValue)
            {
                gameMode = "Player vs AI Mode";
                string difficulty = aiDifficulty?.ToString() ?? "Medium";
                aiInfo = $"\nAI: {GetAIDisplayName(aiType.Value)}\nDifficulty: {difficulty}";
            }
            else
            {
                gameMode = "Player vs Player Mode";
                aiInfo = "\nBoth players are human";
            }
            
            string message = $"Current Game Mode: {gameMode}{aiInfo}\n";
            
            MessageBox.Show(message, "About Current Game", MessageBoxButton.OK);
        }


        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            CleanupGame(isNavigating: true);

            if (Window.GetWindow(this) is MainWindow main)
            {
                main.MainFrame.Navigate(new MenuPage(main));
            }
        }

        private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupGame(isNavigating: true);
        }

        private void CleanupGame(bool isNavigating = false)
        {
            // Stop any running AI vs AI game
            if (AIvsAIGame != null)
            {
                // Unsubscribe
                AIvsAIGame.OnAIMove -= HandleAIMove;
                AIvsAIGame.OnGameEnd -= HandleGameEnd;
                
                // Mark game as ended
                AIvsAIGame.Ended = true;
                AIvsAIGame = null;
            }
            
            // Reset regular game
            Game?.Reset();
            
            StopReplayAnimation();
            
            if (isNavigating)
            {
                this.Unloaded -= GamePage_Unloaded;
            }
        }

        private string GetAIDisplayName(AIType aiType)
        {
            return aiType switch
            {
                AIType.MinimaxArray => "Minimax (Array)",
                AIType.MinimaxGraph => "Minimax (Graph)", 
                AIType.MonteCarloArray => "Monte Carlo (Array)",
                AIType.MonteCarloGraph => "Monte Carlo (Graph)",
                _ => "AI"
            };
        }

        private void UpdateMatchupDisplay()
        {
            if (isAIvsAIMode && ai1Type.HasValue && ai2Type.HasValue)
            {
                string ai1Name = GetAIDisplayName(ai1Type.Value);
                string ai2Name = GetAIDisplayName(ai2Type.Value);
                ViewModel.MatchupText = $"{ai1Name} vs {ai2Name}";
                
                player1Name = ai1Name;
                player2Name = ai2Name;
            }
            else if (AIOpponent && aiType.HasValue)
            {
                string aiName = GetAIDisplayName(aiType.Value);
                ViewModel.MatchupText = $"Player vs {aiName}";
                
                player1Name = "Player";
                player2Name = aiName;
            }
            else
            {
                ViewModel.MatchupText = "Player vs Player";
                player1Name = "Player";
                player2Name = "Player";
            }
        }

        private void UpdateTurnDisplay()
        {
            if (AIvsAIGame != null)
            {
                int currentPlayer = AIvsAIGame.GetCurrentPlayerNumber();
                string playerName = currentPlayer == 1 ? player1Name : player2Name;
                string color = currentPlayer == 1 ? "Red" : "Yellow";
                ViewModel.StatusText = $"{playerName} turn ({color})";
            }
            else if (Game != null)
            {
                int currentPlayer = Game.GetCurrentPlayerNumber();
                string playerName = currentPlayer == 1 ? player1Name : player2Name;
                string color = currentPlayer == 1 ? "Red" : "Yellow";
                ViewModel.StatusText = $"{playerName} turn ({color})";
            }
        }

        private void HandleAIMove(int row, int column, bool isPlayerOne)
        {
            // Update UI
            Dispatcher.Invoke(() =>
            {
                PlaceDisc(row, column, isPlayerOne);
                UpdateTurnDisplay();
                UpdateArrowColors();
            });
        }

        private void HandleGameEnd(string message, bool isDraw)
        {
            // Handle game end
            Dispatcher.Invoke(() =>
            {
                if (isAIvsAIMode && !isDraw && message.Contains("Joueur"))
                {
                    if (message.Contains("Joueur 1"))
                        ViewModel.StatusText = $"{player1Name} wins !";
                    else if (message.Contains("Joueur 2"))
                        ViewModel.StatusText = $"{player2Name} wins !";
                    else
                        ViewModel.StatusText = message;
                }
                else
                {
                    ViewModel.StatusText = message;
                }
                
                if (isDraw)
                {
                    drawSound.Play();
                }
                else
                {
                    winSound.Play();
                    ShowFirework();
                }
                ViewModel.ReplayButtonVisibility = Visibility.Visible;
                StartReplayAnimation();
            });
        }

    }
}

