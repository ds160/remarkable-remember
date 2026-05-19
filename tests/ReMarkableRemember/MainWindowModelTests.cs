using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Services.TabletService.Models.Enumerations;
using ReMarkableRemember.Tests.Fakes;
using ReMarkableRemember.ViewModels;
using ReMarkableRemember.ViewModels.Enumerations;
using ReMarkableRemember.ViewModels.Interfaces;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class MainWindowModelInitialStateTests
{
    private MainWindowModelFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new MainWindowModelFixture();
    }

    [Test]
    public void Constructor_RunsUpdate_AndPopulatesConnectionStatusFromTabletService()
    {
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 2, 1));
        this.fixture.ConnectionStatus = MainWindowModelFixture.MakeStatus(info, null);

        MainWindowModel vm = this.fixture.Build();

        vm.ConnectionStatus.IsConnected.Should().BeTrue();
        vm.ConnectionStatus.Tablet.Should().Contain("reMarkable 2");
    }

    [Test]
    public void Constructor_DefaultsApplicationThemeFromSettingsConfiguration()
    {
        this.fixture.Services.SettingsConfiguration.SetupGet(c => c.ApplicationTheme).Returns("Dark");

        MainWindowModel vm = this.fixture.Build();

        vm.ApplicationTheme.Should().Be("Dark");
    }

    [Test]
    public void Constructor_ResolvesHandWritingRecognitionLanguageFromConfiguration()
    {
        this.fixture.HwrConfiguration.SetupProperty(c => c.Language, "de_DE");

        MainWindowModel vm = this.fixture.Build();

        vm.HandWritingRecognitionLanguage.Code.Should().Be("de_DE");
    }

    [Test]
    public void Constructor_ListsAllSupportedHandWritingLanguages()
    {
        MainWindowModel vm = this.fixture.Build();

        vm.HandWritingRecognitionLanguages.Should().Contain(l => l.Code == "en_US");
        vm.HandWritingRecognitionLanguages.Should().Contain(l => l.Code == "de_DE");
    }

    [Test]
    public void Constructor_AllCommandsAreCreated()
    {
        MainWindowModel vm = this.fixture.Build();

        vm.CommandAbout.Should().NotBeNull();
        vm.CommandBackup.Should().NotBeNull();
        vm.CommandDownloadFile.Should().NotBeNull();
        vm.CommandExecute.Should().NotBeNull();
        vm.CommandHandwritingRecognition.Should().NotBeNull();
        vm.CommandInstallLamyEraser.Should().NotBeNull();
        vm.CommandManageTemplates.Should().NotBeNull();
        vm.CommandOpenItem.Should().NotBeNull();
        vm.CommandSettings.Should().NotBeNull();
        vm.CommandSync.Should().NotBeNull();
        vm.CommandSyncTargetDirectory.Should().NotBeNull();
        vm.CommandUploadFile.Should().NotBeNull();
        vm.CommandUploadTemplate.Should().NotBeNull();
    }

    [Test]
    public void Constructor_InitialJobsIsNone_SoJobsTextIsNull()
    {
        MainWindowModel vm = this.fixture.Build();

        vm.JobsText.Should().BeNull();
    }

    [Test]
    public void Constructor_InitialItemsTreeIsEmpty()
    {
        MainWindowModel vm = this.fixture.Build();

        vm.ItemsTree.Should().NotBeNull();
        vm.ItemsTree.Items.Should().BeEmpty();
    }

    [Test]
    public void Title_IncludesApplicationVersion()
    {
        String title = MainWindowModel.Title;

        title.Should().StartWith("reMarkable Remember - ");
        title.Should().MatchRegex(@"\d+\.\d+\.\d+$");
    }

    [Test]
    public void Constructor_PopulatesItemsTreeFromGetItems()
    {
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 0));
        this.fixture.ConnectionStatus = MainWindowModelFixture.MakeStatus(info, null);
        TabletItem doc = new TabletItem("doc-1", "1700000000000", String.Empty, "DocumentType", "MyDoc");
        this.fixture.Items = new TabletItems(new[] { doc }, new System.Collections.Generic.Dictionary<String, Exception>());

        MainWindowModel vm = this.fixture.Build();

        vm.ItemsTree.Items.Should().ContainSingle().Which.Id.Should().Be("doc-1");
    }

    [Test]
    public void Constructor_TrashedItemsAreNotShown()
    {
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 0));
        this.fixture.ConnectionStatus = MainWindowModelFixture.MakeStatus(info, null);
        TabletItem trashed = new TabletItem("trash-doc", "1700000000000", "trash", "DocumentType", "Trashed");
        TabletItem live = new TabletItem("live-doc", "1700000000000", String.Empty, "DocumentType", "Live");
        this.fixture.Items = new TabletItems(new[] { trashed, live }, new System.Collections.Generic.Dictionary<String, Exception>());

        MainWindowModel vm = this.fixture.Build();

        vm.ItemsTree.Items.Select(i => i.Id).Should().BeEquivalentTo(["live-doc"]);
    }
}

[TestFixture]
public sealed class MainWindowModelCanExecuteTests
{
    private MainWindowModelFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new MainWindowModelFixture();
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 2, 1));
        this.fixture.ConnectionStatus = MainWindowModelFixture.MakeStatus(info, null);
    }

    [Test]
    public void CommandSettings_AllowedWhenJobsIsNone()
    {
        MainWindowModel vm = this.fixture.Build();

        vm.CommandSettings.CanExecute(null).Should().BeTrue();
    }

    [Test]
    public void CommandSettings_BlockedWhenABusyJobIsActive()
    {
        MainWindowModel vm = this.fixture.Build();
        using IJob job = vm.CreateJobForTesting(Jobs.Backup);

        vm.CommandSettings.CanExecute(null).Should().BeFalse();
    }

    [Test]
    public void CommandBackup_BlockedWhenNoBackupDirectoryConfigured()
    {
        // HasBackupDirectory derives from Path.Exists(services.Tablet.Configuration.Backup);
        // the default fixture leaves Backup as empty -> doesn't exist.
        MainWindowModel vm = this.fixture.Build();

        vm.CommandBackup.CanExecute(null).Should().BeFalse();
    }

    [Test]
    public void CommandBackup_BlockedWhenItemsTreeIsEmpty()
    {
        String tempBackup = CreateTempDir();
        try
        {
            this.fixture.Services.TabletConfiguration.SetupGet(c => c.Backup).Returns(tempBackup);

            MainWindowModel vm = this.fixture.Build();

            vm.CommandBackup.CanExecute(null).Should().BeFalse("ItemsTree is empty -> no work to back up");
        }
        finally
        {
            Directory.Delete(tempBackup, true);
        }
    }

    [Test]
    public void CommandSync_BlockedWhenUsbDisconnected()
    {
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 0));
        this.fixture.ConnectionStatus = MainWindowModelFixture.MakeStatus(info, TabletError.UsbNotConnected);

        MainWindowModel vm = this.fixture.Build();

        vm.CommandSync.CanExecute(null).Should().BeFalse();
    }

    [Test]
    public void CommandInstallLamyEraser_BlockedOnUnsupportedHardware()
    {
        TabletInformation paperPro = new TabletInformation(TabletType.rMPaperPro, new Version(3, 0));
        this.fixture.ConnectionStatus = MainWindowModelFixture.MakeStatus(paperPro, null);

        MainWindowModel vm = this.fixture.Build();

        vm.CommandInstallLamyEraser.CanExecute(null).Should().BeFalse();
    }

    [Test]
    public void CommandInstallLamyEraser_AllowedOnRm2()
    {
        MainWindowModel vm = this.fixture.Build();

        vm.CommandInstallLamyEraser.CanExecute(null).Should().BeTrue();
    }

    [Test]
    public void CommandDownloadFile_BlockedWhenNoItemSelected()
    {
        MainWindowModel vm = this.fixture.Build();

        vm.CommandDownloadFile.CanExecute(null).Should().BeFalse();
    }

    private static String CreateTempDir()
    {
        String dir = Path.Combine(Path.GetTempPath(), "rmr-main-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}

[TestFixture]
public sealed class MainWindowModelBehaviorTests
{
    private MainWindowModelFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new MainWindowModelFixture();
    }

    [Test]
    public void ShowException_HandlesShowDialogWithErrorMessage()
    {
        MainWindowModel vm = this.fixture.Build();
        DialogWindowModel? captured = null;
        using IDisposable _ = vm.ShowDialog.RegisterHandler(ctx => { captured = ctx.Input; ctx.SetOutput(false); });

        vm.ShowException(new InvalidOperationException("boom"));

        captured.Should().BeOfType<MessageViewModel>();
        ((MessageViewModel)captured!).Message.Should().Be("boom");
    }

    [Test]
    public void SaveHandWritingRecognitionLanguage_SwitchingLanguage_PersistsToConfiguration()
    {
        MainWindowModel vm = this.fixture.Build();
        HandWritingRecognitionLanguageViewModel german = vm.HandWritingRecognitionLanguages.Single(l => l.Code == "de_DE");

        vm.HandWritingRecognitionLanguage = german;

        this.fixture.HwrConfiguration.Object.Language.Should().Be("de_DE");
        this.fixture.HwrConfiguration.Verify(c => c.Save(), Times.AtLeastOnce);
    }

    [Test]
    public void Jobs_TogglesJobsText()
    {
        MainWindowModel vm = this.fixture.Build();

        using IJob job = vm.CreateJobForTesting(Jobs.Backup);

        vm.JobsText.Should().NotBeNullOrEmpty();
        vm.JobsText.Should().Be(Jobs.Backup.GetDisplayText());
    }

    [Test]
    public void Jobs_Combined_DisplaysJoinedText()
    {
        MainWindowModel vm = this.fixture.Build();

        using IJob job = vm.CreateJobForTesting(Jobs.Backup | Jobs.Sync);

        vm.JobsText.Should().Contain(Jobs.Backup.GetDisplayText());
        vm.JobsText.Should().Contain(Jobs.Sync.GetDisplayText());
    }

    [Test]
    public void Jobs_Class_AddsAndRemovesFlagOnDispose()
    {
        MainWindowModel vm = this.fixture.Build();

        using IDisposable job = vm.CreateJobForTesting(Jobs.Backup);

        vm.Jobs.Should().Be(Jobs.Backup);

        job.Dispose();

        vm.Jobs.Should().Be(Jobs.None);
    }

    [Test]
    public void Jobs_Class_DoneShortCircuitsDispose()
    {
        MainWindowModel vm = this.fixture.Build();

        using IJob job = vm.CreateJobForTesting(Jobs.Sync);
        vm.Jobs.Should().Be(Jobs.Sync);

        job.Done();
        vm.Jobs.Should().Be(Jobs.None, "Done() clears the flag");

        // Subsequent Dispose() must be a no-op (otherwise the XOR would toggle the flag back on).
        job.Dispose();
        vm.Jobs.Should().Be(Jobs.None);
    }

    [Test]
    public void Jobs_Class_IsReturnsTrueForOwnFlag()
    {
        MainWindowModel vm = this.fixture.Build();

        using IJob job = vm.CreateJobForTesting(Jobs.UploadTemplate);

        job.Is(Jobs.UploadTemplate).Should().BeTrue();
        job.Is(Jobs.Backup).Should().BeFalse();
    }
}
