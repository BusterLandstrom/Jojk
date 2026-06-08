using Avalonia.Media;
using Jojk.Models;
using ReactiveUI;
using System;
using System.Reactive;

namespace Jojk.ViewModels
{
    public class ConnectViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWVM;

        private string _ipInput = "";
        public string IPInput
        {
            get => _ipInput;
            set => this.RaiseAndSetIfChanged(ref _ipInput, value);
        }

        private string _messageText = "";
        public string MessageText
        {
            get => _messageText;
            set => this.RaiseAndSetIfChanged(ref _messageText, value);
        }

        private SolidColorBrush _messageColor = new (Colors.White);
        public SolidColorBrush MessageColor
        {
            get => _messageColor;
            set => this.RaiseAndSetIfChanged(ref _messageColor, value);
        }

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

        public ConnectViewModel(MainWindowViewModel mwvm)
        {
            _mainWVM = mwvm;

            ConnectCommand = ReactiveCommand.Create(() =>
            {
                if (CheckURLValid(IPInput))
                {
                    SetMessage(false);
                    _mainWVM.CM = new ConnectionManager(IPInput);
                    _mainWVM.NavigateTo(new LoginViewModel(_mainWVM));
                }
                else
                {
                    SetMessage(true);
                }
            });

            ConnectCommand.ThrownExceptions.Subscribe(ex => SetMessage(true));
        }

        public static bool CheckURLValid(string strURL)
        {
            return Uri.IsWellFormedUriString(strURL, UriKind.RelativeOrAbsolute); ;
        }

        public void SetMessage(bool fail)
        {
            if (!fail)
            {
                MessageColor = GetResources.GetBrush("Neutral");
                MessageText = "Connecting...";
            }
            else
            {
                MessageColor = GetResources.GetBrush("Negative");
                MessageText = "Wrong URL";
            }
        }
    }
}
