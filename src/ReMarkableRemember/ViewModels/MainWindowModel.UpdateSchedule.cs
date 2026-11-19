using System;
using System.Threading.Tasks;
using ReactiveUI.Primitives.Concurrency;

namespace ReMarkableRemember.ViewModels;

public partial class MainWindowModel
{
    private sealed class UpdateSchedule : IWorkItem
    {
        private readonly MainWindowModel model;

        public UpdateSchedule(MainWindowModel model)
        {
            this.model = model;
        }

        void IWorkItem.Execute()
        {
            this.Update();
        }

        private async void Update()
        {
            while (true)
            {
                Boolean updated = await this.model.UpdateItems().ConfigureAwait(true);

                await Task.Delay(TimeSpan.FromSeconds(updated ? 10 : 1)).ConfigureAwait(true);
            }
        }
    }
}
