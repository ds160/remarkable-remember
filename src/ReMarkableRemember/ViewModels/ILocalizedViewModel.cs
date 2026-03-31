using System.ComponentModel;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.ViewModels;

public interface ILocalizedViewModel : INotifyPropertyChanged
{
    ILocalStrings LocalStrings { get; }
}
