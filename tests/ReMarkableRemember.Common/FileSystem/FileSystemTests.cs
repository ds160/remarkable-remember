using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Sut = ReMarkableRemember.Common.FileSystem.FileSystem;

namespace ReMarkableRemember.Common.FileSystem.Tests;

[TestFixture]
public sealed class FileSystemTests
{
    private String tempRoot = String.Empty;

    [SetUp]
    public void SetUp()
    {
        this.tempRoot = Path.Combine(Path.GetTempPath(), "rmr-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.tempRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.tempRoot))
        {
            Directory.Delete(this.tempRoot, true);
        }
    }

    [Test]
    public void Create_PathInExistingDirectory_ReturnsWritableStream()
    {
        String path = Path.Combine(this.tempRoot, "file.txt");

        using (FileStream stream = Sut.Create(path))
        {
            stream.CanWrite.Should().BeTrue();
            stream.WriteByte(0x42);
        }

        File.Exists(path).Should().BeTrue();
        File.ReadAllBytes(path).Should().Equal([0x42]);
    }

    [Test]
    public void Create_PathWithMissingParents_CreatesParentDirectories()
    {
        String path = Path.Combine(this.tempRoot, "a", "b", "c", "file.txt");

        using (FileStream stream = Sut.Create(path))
        {
            stream.Should().NotBeNull();
        }

        File.Exists(path).Should().BeTrue();
        Directory.Exists(Path.GetDirectoryName(path)).Should().BeTrue();
    }

    [Test]
    public void Create_ExistingFile_OverwritesIt()
    {
        String path = Path.Combine(this.tempRoot, "existing.txt");
        File.WriteAllText(path, "old content");

        using (FileStream stream = Sut.Create(path))
        {
            stream.WriteByte(0x01);
        }

        File.ReadAllBytes(path).Should().Equal(new Byte[] { 0x01 });
    }

    [Test]
    public void CreateApplicationDataFilePath_ReturnsPathUnderLocalApplicationData()
    {
        String result = Sut.CreateApplicationDataFilePath("test-marker.txt");

        try
        {
            String expectedRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReMarkableRemember");
            result.Should().StartWith(expectedRoot);
            result.Should().EndWith("test-marker.txt");
            Directory.Exists(Path.GetDirectoryName(result)).Should().BeTrue();
        }
        finally
        {
            String? dir = Path.GetDirectoryName(result);
            if (dir != null && Directory.Exists(dir) && Path.GetFileName(dir) == "ReMarkableRemember")
            {
                // Don't remove the actual app data dir; only assert it exists.
            }
        }
    }

    [Test]
    public void CreateApplicationDataFilePath_CalledTwice_IsIdempotent()
    {
        String first = Sut.CreateApplicationDataFilePath("idempotent.txt");
        String second = Sut.CreateApplicationDataFilePath("idempotent.txt");

        first.Should().Be(second);
    }

    [Test]
    public void Delete_ExistingFile_RemovesFile()
    {
        String path = Path.Combine(this.tempRoot, "to-delete.txt");
        File.WriteAllText(path, "x");

        Sut.Delete(path);

        File.Exists(path).Should().BeFalse();
    }

    [Test]
    public void Delete_ExistingDirectory_RemovesRecursively()
    {
        String dir = Path.Combine(this.tempRoot, "to-delete");
        Directory.CreateDirectory(Path.Combine(dir, "nested"));
        File.WriteAllText(Path.Combine(dir, "a.txt"), "x");
        File.WriteAllText(Path.Combine(dir, "nested", "b.txt"), "y");

        Sut.Delete(dir);

        Directory.Exists(dir).Should().BeFalse();
    }

    [Test]
    public void Delete_NonExistentPath_DoesNotThrow()
    {
        String path = Path.Combine(this.tempRoot, "never-existed.txt");

        Action act = () => Sut.Delete(path);

        act.Should().NotThrow();
    }

    [Test]
    public void Delete_EmptyDirectory_RemovesIt()
    {
        String dir = Path.Combine(this.tempRoot, "empty");
        Directory.CreateDirectory(dir);

        Sut.Delete(dir);

        Directory.Exists(dir).Should().BeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\t")]
    public void EnsureExists_NullOrWhitespace_ReturnsNull(String? input)
    {
        String? result = Sut.EnsureExists(input);

        result.Should().BeNull();
    }

    [Test]
    public void EnsureExists_ValidPath_CreatesDirectoryAndReturnsInput()
    {
        String path = Path.Combine(this.tempRoot, "new-dir", "file.txt");

        String? result = Sut.EnsureExists(path);

        result.Should().Be(path);
        Directory.Exists(Path.GetDirectoryName(path)).Should().BeTrue();
    }

    [Test]
    public void EnsureExists_DirectoryAlreadyExists_DoesNotThrowAndReturnsInput()
    {
        String path = Path.Combine(this.tempRoot, "existing", "file.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        String? result = Sut.EnsureExists(path);

        result.Should().Be(path);
    }
}
