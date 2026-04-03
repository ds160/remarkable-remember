using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ReactiveUI;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.ViewModels;

public abstract class ViewModelBase : ReactiveObject, ILocalizedViewModel, INotifyDataErrorInfo
{
    private readonly Dictionary<String, List<ValidationResult>> errors;

    protected ViewModelBase()
    {
        this.errors = new Dictionary<String, List<ValidationResult>>();
    }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public Boolean HasErrors { get { return this.errors.Count > 0; } }

    public ILocalStrings LocalStrings { get { return Language.Current; } }

    protected void AddError(String propertyName, String errorMessage)
    {
        if (!this.errors.TryGetValue(propertyName, out List<ValidationResult>? propertyErrors))
        {
            propertyErrors = new List<ValidationResult>();
            this.errors.Add(propertyName, propertyErrors);
        }

        propertyErrors.Add(new ValidationResult(errorMessage));

        this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        this.RaisePropertyChanged(nameof(this.HasErrors));
    }

    protected void ClearErrors(String? propertyName = null)
    {
        if (String.IsNullOrEmpty(propertyName))
        {
            this.errors.Clear();
        }
        else
        {
            this.errors.Remove(propertyName);
        }

        this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        this.RaisePropertyChanged(nameof(this.HasErrors));
    }

    public IEnumerable GetErrors(String? propertyName)
    {
        if (String.IsNullOrEmpty(propertyName))
        {
            return this.errors.Values.SelectMany(static errors => errors);
        }

        if (this.errors.TryGetValue(propertyName, out List<ValidationResult>? result))
        {
            return result;
        }

        return Array.Empty<ValidationResult>();
    }
}
