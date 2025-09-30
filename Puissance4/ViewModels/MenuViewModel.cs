using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Puissance4.AI;

namespace Puissance4.ViewModels
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        private Visibility _gameTypePanelVisibility = Visibility.Visible;
        private Visibility _boardTypePanelVisibility = Visibility.Collapsed;
        private Visibility _algorithmPanelVisibility = Visibility.Collapsed;
        private Visibility _difficultyPanelVisibility = Visibility.Collapsed;
        private Visibility _aivsAIPanelVisibility = Visibility.Collapsed;
        private AIType? _selectedAI1Type;
        private AIType? _selectedAI2Type;

        public Visibility GameTypePanelVisibility
        {
            get => _gameTypePanelVisibility;
            set
            {
                _gameTypePanelVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility BoardTypePanelVisibility
        {
            get => _boardTypePanelVisibility;
            set
            {
                _boardTypePanelVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility AlgorithmPanelVisibility
        {
            get => _algorithmPanelVisibility;
            set
            {
                _algorithmPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility DifficultyPanelVisibility
        {
            get => _difficultyPanelVisibility;
            set
            {
                _difficultyPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility AIvsAIPanelVisibility
        {
            get => _aivsAIPanelVisibility;
            set
            {
                _aivsAIPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        public AIType? SelectedAI1Type
        {
            get => _selectedAI1Type;
            set
            {
                _selectedAI1Type = value;
                OnPropertyChanged();
            }
        }

        public AIType? SelectedAI2Type
        {
            get => _selectedAI2Type;
            set
            {
                _selectedAI2Type = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}