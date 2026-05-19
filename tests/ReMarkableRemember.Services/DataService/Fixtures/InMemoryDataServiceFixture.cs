using System;
using System.IO;

namespace ReMarkableRemember.Services.DataService.Tests.Fixtures;

internal sealed class InMemoryDataServiceFixture : IDisposable
{
    private readonly String tempDirectory;

    public InMemoryDataServiceFixture()
    {
        this.tempDirectory = Path.Combine(Path.GetTempPath(), "rmr-data-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.tempDirectory);

        this.Service = DataServiceSqlite.Create(this.tempDirectory);
    }

    public DataServiceSqlite Service { get; }

    public void Dispose()
    {
        ((IDisposable)this.Service).Dispose();
        if (Directory.Exists(this.tempDirectory)) { Directory.Delete(this.tempDirectory, true); }
    }
}
