using System;
using System.Threading;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal abstract class CommunicationBase : IDisposable
{
    protected const String IP = "10.11.99.1";

    private readonly SemaphoreSlim semaphore;

    protected CommunicationBase(SemaphoreSlim semaphore)
    {
        this.semaphore = semaphore;
    }

    protected abstract void OnDisposing();

    void IDisposable.Dispose()
    {
        try
        {
            this.OnDisposing();
        }
        catch
        {
            // Ignore exception
        }
        finally
        {
            this.semaphore.Release();
        }
    }
}
