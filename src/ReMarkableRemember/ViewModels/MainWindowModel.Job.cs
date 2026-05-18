using System;
using ReMarkableRemember.ViewModels.Enumerations;
using ReMarkableRemember.ViewModels.Interfaces;

namespace ReMarkableRemember.ViewModels;

public partial class MainWindowModel
{
    private sealed class Job : IJob
    {
        private Boolean done;
        private readonly Jobs job;
        private readonly MainWindowModel owner;

        public Job(Jobs job, MainWindowModel owner)
        {
            this.done = false;
            this.job = job;
            this.owner = owner;

            this.owner.Jobs |= this.job;
        }

        void IDisposable.Dispose()
        {
            if (!this.done)
            {
                this.done = true;
                this.owner.Jobs ^= this.job;
            }
        }

        public void Done()
        {
            (this as IDisposable).Dispose();
        }

        public Boolean Is(Jobs job)
        {
            return this.job.HasFlag(job);
        }
    }
}
