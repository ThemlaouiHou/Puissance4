using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Puissance4.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private string _statusText = "Player turn (Rouge)";
        private string _matchupText = "Player vs Player";
        private Visibility _replayButtonVisibility = Visibility.Collapsed;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public string MatchupText
        {
            get => _matchupText;
            set
            {
                _matchupText = value;
                OnPropertyChanged();
            }
        }

        public Visibility ReplayButtonVisibility
        {
            get => _replayButtonVisibility;
            set
            {
                _replayButtonVisibility = value;
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