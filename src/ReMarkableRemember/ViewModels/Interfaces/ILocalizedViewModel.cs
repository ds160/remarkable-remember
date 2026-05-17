using System.ComponentModel;
using ReMarkableRemember.Common.Localization.Interfaces;

namespace ReMarkableRemember.ViewModels.Interfaces;

public interface ILocalizedViewModel : INotifyPropertyChanged
{
    ILocalStrings LocalStrings { get; }
}
