using System;
using System.Collections.Generic;

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

        if (job.HasFlag(Jobs.GetItems)) { jobs.Add("Getting Items"); }
        if (job.HasFlag(Jobs.Sync)) { jobs.Add("Syncing"); }
        if (job.HasFlag(Jobs.Backup)) { jobs.Add("Backup"); }
        if (job.HasFlag(Jobs.HandwritingRecognition)) { jobs.Add("Handwriting Recognition"); }
        if (job.HasFlag(Jobs.Download)) { jobs.Add("Downloading File"); }
        if (job.HasFlag(Jobs.Upload)) { jobs.Add("Uploading File"); }
        if (job.HasFlag(Jobs.UploadTemplate)) { jobs.Add("Uploading Template"); }
        if (job.HasFlag(Jobs.ManageTemplates)) { jobs.Add("Managing Templates"); }
        if (job.HasFlag(Jobs.InstallLamyEraser)) { jobs.Add("Installing Lamy Eraser"); }

        return (jobs.Count > 0) ? String.Join(" and ", jobs) : null;
    }
}
