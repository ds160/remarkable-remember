using System;
using System.Threading;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal abstract class CommunicationBase : IDisposable
{
    private readonly SemaphoreSlim semaphore;

    protected CommunicationBase(SemaphoreSlim semaphore)
    {
        this.semaphore = semaphore;
    }

    public virtual void Dispose()
    {
        this.semaphore.Release();
    }
}
