using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognitionService;

public interface IHandWritingRecognitionService
{
    IHandWritingRecognitionConfiguration Configuration { get; }

    IEnumerable<String> SupportedLanguages { get; }

    Task<IEnumerable<String>> Recognize(Notebook notebook);
}
