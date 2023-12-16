using Avalonia.Controls;
using ReactiveUI;

namespace ReMarkableRemember.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        RxApp.DefaultExceptionHandler = new MainWindowExceptionHandler(this);
    }
}
