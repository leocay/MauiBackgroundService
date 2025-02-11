using Android.Content;
using MauiBackgroundService.Platforms.Android;

namespace MauiBackgroundService
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

            var intent = new Intent(Android.App.Application.Context, typeof(MauiBackgroundMusic));
            Android.App.Application.Context.StartForegroundService(intent);
        }
    }

}
