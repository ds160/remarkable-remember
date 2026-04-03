using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.Enumerations;

[Flags]
public enum Jobs
{
    None = 0x0000,
    GetItems = 0x0001,
    Sync = 0x0002,
    Backup = 0x0004,
    HandwritingRecognition = 0x0008,
    Download = 0x0010,
    Upload = 0x0020,
    UploadTemplate = 0x0040,
    ManageTemplates = 0x0080,
    SetSyncTargetDirectory = 0x0100,
    InstallLamyEraser = 0x0200,
    Settings = 0x0400
}

public static class JobsExtensions
{
    public static String? GetDisplayText(this Jobs job)
    {
        List<String> jobs = new List<String>();

        if (job.HasFlag(Jobs.GetItems)) { jobs.Add(Language.Current.JobGetItems); }
        if (job.HasFlag(Jobs.Sync)) { jobs.Add(Language.Current.JobSync); }
        if (job.HasFlag(Jobs.Backup)) { jobs.Add(Language.Current.JobBackup); }
        if (job.HasFlag(Jobs.HandwritingRecognition)) { jobs.Add(Language.Current.JobHandwritingRecognition); }
        if (job.HasFlag(Jobs.Download)) { jobs.Add(Language.Current.JobDownload); }
        if (job.HasFlag(Jobs.Upload)) { jobs.Add(Language.Current.JobUpload); }
        if (job.HasFlag(Jobs.UploadTemplate)) { jobs.Add(Language.Current.JobUploadTemplate); }
        if (job.HasFlag(Jobs.ManageTemplates)) { jobs.Add(Language.Current.JobManageTemplates); }
        if (job.HasFlag(Jobs.InstallLamyEraser)) { jobs.Add(Language.Current.JobInstallLamyEraser); }

        return (jobs.Count > 0) ? String.Join(Language.Current.JobAndJoin, jobs) : null;
    }
}
