using System;
using Xamarin.Forms;

namespace MiniLauncher.Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new Playground(), true);
        }
    }
}