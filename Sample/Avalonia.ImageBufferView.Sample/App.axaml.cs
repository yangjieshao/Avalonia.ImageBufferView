using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ImageBufferView.Sample.ViewModels;
using Avalonia.ImageBufferView.Sample.Views;
using Avalonia.Markup.Xaml;

namespace Avalonia.ImageBufferView.Sample
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}