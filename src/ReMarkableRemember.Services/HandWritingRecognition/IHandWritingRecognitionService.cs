using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.HandWritingRecognition.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognition;

public interface IHandWritingRecognitionService
{
    IHandWritingRecognitionConfiguration Configuration { get; }

    IEnumerable<String> SupportedLanguages { get; }

    Task<IEnumerable<String>> Recognize(Notebook notebook);
}
