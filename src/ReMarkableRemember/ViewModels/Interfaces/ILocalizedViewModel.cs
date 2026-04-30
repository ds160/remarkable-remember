using System.ComponentModel;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.ViewModels.Interfaces;

public interface ILocalizedViewModel : INotifyPropertyChanged
{
    ILocalStrings LocalStrings { get; }
}
