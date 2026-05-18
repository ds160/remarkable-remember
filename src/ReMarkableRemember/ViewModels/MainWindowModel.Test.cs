using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.ViewModels.Enumerations;
using ReMarkableRemember.ViewModels.Interfaces;

namespace ReMarkableRemember.ViewModels;

public partial class MainWindowModel
{
    internal static MainWindowModel CreateForTesting(IServices services)
    {
        MainWindowModel mainWindowModel = new MainWindowModel(services);
        mainWindowModel.UpdateItems().Wait();
        return mainWindowModel;
    }

    internal IJob CreateJobForTesting(Jobs job)
    {
        return new Job(job, this);
    }

    internal Jobs GetJobsForTesting()
    {
        return this.Jobs;
    }

    internal void SetJobsForTesting(Jobs jobs)
    {
        this.Jobs = jobs;
    }
}
