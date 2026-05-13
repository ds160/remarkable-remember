using System;
using System.Threading.Tasks;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;

public interface IMyScriptCommunication
{
    void Configuration(IHandWritingRecognitionConfiguration configuration);

    Task<IMyScriptResponse> Recognize(String hmac, String jsonRequest);
}
