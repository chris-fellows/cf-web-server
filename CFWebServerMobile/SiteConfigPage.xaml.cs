using CFWebServerMobile.ViewModels;

namespace CFWebServerMobile;

public partial class SiteConfigPage : ContentPage
{
    private readonly SiteConfigPageModel _model;

	public SiteConfigPage(SiteConfigPageModel model)
    {
		InitializeComponent();

        _model = model;
        this.BindingContext = _model;
    }
}