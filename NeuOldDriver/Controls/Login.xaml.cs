﻿using System;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using NeuOldDriver.Utils;
using NeuOldDriver.Global;
using NeuOldDriver.Models;

namespace NeuOldDriver.Controls {

    public sealed partial class Login : UserControl {

        public delegate Task<string> RefreshHandler(object sender, EventArgs e);
        public delegate Task<bool> LoginHandler(object sender, LoginData data);

        /// <summary>
        /// Fired when click on "确认" button
        /// </summary>
        public event LoginHandler Submit;

        /// <summary>
        /// Fired when click on captcha image
        /// </summary>
        public event RefreshHandler Refresh;

        private ICollection<string> names;
        private bool notLogged = true;

        public Login() {
            this.InitializeComponent();

            this.Loaded += async (sender, e) => {
                if (NotLogged)
                    await ClearState();
            };

            okButton.Click += DoLogin;

            captchaContainer.Click += async (sender, e) => {
                ImageSource = await Refresh?.Invoke(this, new EventArgs());
            };

            username.TextChanged += (sender, e) => {
                if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                    sender.ItemsSource = names.Where((item) => {
                        return item.StartsWith(sender.Text);
                    });
                    remember.IsChecked = false;
                    password.Password = "";
                }
            };

            username.SuggestionChosen += (sender, e) => {
                var username = e.SelectedItem as string;
                password.Password = Globals.Accounts[UsedFor][username];
                remember.IsChecked = true;
            };
        }

        /// <summary>
        /// Set control to initial state
        /// </summary>
        public async Task ClearState() {
            Animations.StartFadeAnimation(backdrop, this, FADE_DELAY);

            // get username and password of last logged user
            var accounts = Globals.Accounts[UsedFor];
            var username = accounts.Active;
            var password = accounts[username];

            if (CaptchaRequired)
                ImageSource = await Refresh?.Invoke(this, new EventArgs());

            names = accounts.Users;
            this.username.Text = username;
            this.username.ItemsSource = names;
            this.password.Password = password ?? "";
            this.remember.IsChecked = !String.IsNullOrEmpty(password);
            NotLogged = true;
        }

        private async void DoLogin(object sender, RoutedEventArgs e) {
            var data = new LoginData() {
                username = UserName, password = Password, remember = RememberMe
            };
            if (CaptchaRequired)
                data.captcha = Captcha;

            if (await Submit?.Invoke(this, data)) {
                NotLogged = false;

                Animations.StartFadeAnimation(backdrop, this, FADE_DELAY, false);

                var accounts = Globals.Accounts[UsedFor];
                accounts.Active = UserName;
                if (RememberMe || accounts[UserName] != null)
                    accounts[UserName] = Password;
            }
        }
    }

    // Describing attached properties
    public sealed partial class Login : UserControl, INotifyPropertyChanged {

        private const int FADE_DELAY = 200;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propname = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        /// <summary>
        /// Username from user's input
        /// </summary>
        public string UserName {
            get { return username.Text; }
        }

        /// <summary>
        /// Password from user's input
        /// </summary>
        public string Password {
            get { return password.Password; }
        }

        /// <summary>
        /// Save password when true
        /// </summary>
        public bool RememberMe {
            get { return remember.IsChecked ?? false; }
        }

        /// <summary>
        /// Captcha string from user's input. Use only when <c>CaptchaRequired</c> is true.
        /// </summary>
        public string Captcha {
            get { return captchaText.Text; }
        }

        public bool NotLogged {
            get { return notLogged; }
            private set { notLogged = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Main content
        /// </summary>
        public object MainContent {
            get { return GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.RegisterAttached(nameof(MainContent), typeof(object), typeof(Login), null);

        /// <summary>
        /// true if login requires captcha verification
        /// </summary>
        public bool CaptchaRequired {
            get { return (bool)GetValue(CaptchaRequiredProperty); }
            set { SetValue(CaptchaRequiredProperty, value); }
        }

        public static readonly DependencyProperty CaptchaRequiredProperty =
            DependencyProperty.RegisterAttached(nameof(CaptchaRequired), typeof(bool), typeof(Login), new PropertyMetadata(false));

        /// <summary>
        /// Source of captcha image
        /// </summary>
        public string ImageSource {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.RegisterAttached(nameof(ImageSource), typeof(string), typeof(Login), new PropertyMetadata(" "));

        /// <summary>
        /// Identifying this login control is used for which page
        /// </summary>
        public string UsedFor {
            get { return (string)GetValue(UsedForProperty); }
            set { SetValue(UsedForProperty, value); }
        }

        public static readonly DependencyProperty UsedForProperty =
            DependencyProperty.RegisterAttached(nameof(UsedFor), typeof(string), typeof(Login), null);
    };
}
