using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ReMarkableRemember.ViewModels;

public interface IAppModel : ILocalizedViewModel, INotifyPropertyChanged
{
    ICommand CommandAbout { get; }

    ICommand CommandSettings { get; }

    String ApplicationTheme { get; set; }
}
