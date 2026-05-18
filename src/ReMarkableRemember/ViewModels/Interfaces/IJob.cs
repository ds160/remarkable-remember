using System;
using ReMarkableRemember.ViewModels.Enumerations;

namespace ReMarkableRemember.ViewModels.Interfaces;

internal interface IJob : IDisposable
{
    void Done();

    Boolean Is(Jobs job);
}
