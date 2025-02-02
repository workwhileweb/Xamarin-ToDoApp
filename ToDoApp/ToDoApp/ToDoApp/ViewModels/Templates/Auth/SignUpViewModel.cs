﻿using Prism.Events;
using Prism.Navigation;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;
using System.Windows.Input;
using ToDoApp.Auth;
using ToDoApp.Events;
using ToDoApp.Helpers;
using ToDoApp.Helpers.Validations;
using ToDoApp.Helpers.Validations.Rules;
using ToDoApp.Views.Dialogs;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace ToDoApp.ViewModels.Templates.Auth
{
    public class SignUpViewModel : BaseRegionViewModel
    {
        #region Private & Protected

        private IDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;

        #endregion

        #region Properties
        public ValidatableObject<string> Username { get; set; }
        public ValidatableObject<string> Email { get; set; }
        public ValidatableObject<string> Password { get; set; }
        public ValidatableObject<string> ConfirmPassword { get; set; }

        #endregion

        #region Commands

        public ICommand SignUpCommand { get; set; }
        public ICommand ValidateCommand { get; set; }

        #endregion

        #region Constructors

        public SignUpViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            IEventAggregator eventAggregator) : base(navigationService)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;

            SignUpCommand = new Command(SignUpCommandHandler);

            ValidateCommand = new Command<string>(ValidateCommandHandler);

            AddValidations();
        }

        #endregion

        #region Validation Handlers

        private void ValidateCommandHandler(string field)
        {
            switch (field)
            {
                case "username": Username.Validate(); break;
                case "email": Email.Validate(); break;
                case "password": Password.Validate(); break;
                case "confirmPassword": ConfirmPassword.Validate(); break;
            }
        }

        #endregion

        #region Command Handlers

        private async void SignUpCommandHandler()
        {
            try
            {
                MainState = LayoutState.Loading;
                if (ValidateSignUpData())
                {
                    var auth = DependencyService.Get<IFirebaseAuthentication>();
                    var created = await auth.RegisterWithEmailAndPassword(Username.Value, Email.Value, Password.Value);

                    if (created)
                    {
                        _eventAggregator.GetEvent<SwitchViewEvent>().Publish("Login");
                        ClearAuthData();
                        Debug.WriteLine("User Created");
                    }
                    else
                    {
                        var param = new DialogParameters
                        {
                            { "message", Constants.Errors.GeneralError }
                        };
                        _dialogService.ShowDialog(nameof(ErrorDialog), param);
                    }
                }
            }
            catch (Exception ex)
            {
                var param = new DialogParameters
                {
                    { "message", Constants.Errors.GeneralError }
                };
                _dialogService.ShowDialog(nameof(ErrorDialog), param);
                Debug.WriteLine(ex);
            }
            finally
            {
                MainState = LayoutState.None;
            }
        }

        #endregion

        #region Private Functionality

        private void AddValidations()
        {
            Username = new ValidatableObject<string>();
            Email = new ValidatableObject<string>();
            Password = new ValidatableObject<string>();
            ConfirmPassword = new ValidatableObject<string>();

            Username.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "A username is required." });
            Email.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "A email is required." });
            Email.Validations.Add(new IsEmailRule<string> { ValidationMessage = "Email format is not correct." });
            Password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "A password is required." });
            ConfirmPassword.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "A confirm password is required." });
        }

        private bool ValidateSignUpData()
        {
            if (Username.IsValid == false ||
                Email.IsValid == false ||
                Password.IsValid == false ||
                ConfirmPassword.IsValid == false ||
                !Password.Value.Equals(ConfirmPassword.Value))
                return false;
            return true;
        }

        private void ClearAuthData()
        {
            Username.Value = Email.Value = Password.Value = ConfirmPassword.Value = string.Empty;
        }

        #endregion
    }
}
