using CFWebServerMobile.Utilities;
using CFWebServerMobile.ViewModels;

namespace CFWebServerMobile
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageModel _model;

        int count = 0;

        public MainPage(MainPageModel model)
        {
            InitializeComponent();

            InternalUtilities.Log("MainPage.Constructor : Setting model");

            _model = model;
            this.BindingContext = _model;

            InternalUtilities.Log("MainPage.Constructor : Set model");
        }

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}
    }

}
