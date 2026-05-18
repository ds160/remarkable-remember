using System;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;

internal interface IMyScriptResponse : IDisposable
{
    Boolean RequestTooLarge { get; }

    Boolean Unauthorized { get; }

    Task<String> Read();
}
