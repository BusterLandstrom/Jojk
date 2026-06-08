using Avalonia.Media;
using Jojk.Models;
using ReactiveUI;
using System;
using System.Reactive;

namespace Jojk.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWVM;

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        private string _username = "";
        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
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

        public LoginViewModel(MainWindowViewModel mwvm)
        {
            _mainWVM = mwvm;

            LoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                SetMessage(false);
                await _mainWVM.CM.AsyncAuth(Username, Password);
                _mainWVM.AM = new AudioManager(_mainWVM.CM);
                _mainWVM.StartSession();
            });

            LoginCommand.ThrownExceptions.Subscribe(ex => SetMessage(true));
        }

        public void SetMessage(bool fail) 
        {
            if (!fail) 
            {
                MessageColor = GetResources.GetBrush("Neutral"); // Scan resource here instead so it can be dynamically changed with themes 
                MessageText = "Logging in...";
            } else 
            {
                MessageColor = GetResources.GetBrush("Negative"); // Scan resource here instead so it can be dynamically changed with themes 
                MessageText = "Wrong credentials";
            }
        }
    }
}
