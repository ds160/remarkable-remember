using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class UsbCommunication : CommunicationBase, IUsbCommunication
{
    private readonly HttpClient httpClient;
    private readonly HttpClient httpClientConnection;

    public UsbCommunication(HttpClient httpClient, HttpClient httpClientConnection, SemaphoreSlim semaphore)
        : base(semaphore)
    {
        this.httpClient = httpClient;
        this.httpClientConnection = httpClientConnection;
    }

    public async Task CheckConnection()
    {
        try
        {
            await this.httpClientConnection.GetStringAsync("/documents/").ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw ConvertToTabletException(exception);
        }
    }

    public async Task<Stream> Download(String id)
    {
        try
        {
            return await this.httpClient.GetStreamAsync($"/download/{id}/placeholder").ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw ConvertToTabletException(exception);
        }
    }

    public async Task Upload(FileInfo file, String? parentId)
    {
        try
        {
            await this.httpClient.GetStringAsync($"/documents/{parentId}").ConfigureAwait(false);

            String fileName = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(file.Name));
            String mediaType = UploadFileCheck(file);

            using StreamContent fileContent = new StreamContent(File.OpenRead(file.FullName));
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("name", "\"file\""));
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", $"\"{fileName}\""));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

            using MultipartFormDataContent multipartContent = new MultipartFormDataContent() { { fileContent } };
            HttpResponseMessage response = await this.httpClient.PostAsync("/upload", multipartContent).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            throw ConvertToTabletException(exception);
        }
    }

    private static TabletException ConvertToTabletException(Exception exception)
    {
        if (exception is HttpRequestException)
        {
            Exception? innerException = exception.InnerException;
            while (innerException != null)
            {
                if (innerException is SocketException socketException)
                {
                    if (socketException.SocketErrorCode is SocketError.ConnectionRefused)
                    {
                        return new TabletException(TabletError.UsbNotActived, Language.Current.TabletUsbNotActived, exception);
                    }

                    if (socketException.SocketErrorCode is SocketError.HostDown or SocketError.HostUnreachable or SocketError.NetworkDown or SocketError.NetworkUnreachable)
                    {
                        return new TabletException(TabletError.UsbNotConnected, Language.Current.TabletUsbNotConnected, exception);
                    }
                }

                innerException = innerException.InnerException;
            }

            return new TabletException(exception.Message, exception);
        }

        if (exception is TaskCanceledException)
        {
            return new TabletException(TabletError.UsbNotConnected, Language.Current.TabletUsbNotConnected, exception);
        }

        return new TabletException(exception.Message, exception);
    }

    private static String UploadFileCheck(FileInfo file)
    {
        if (file.Length >= 100 * 1024 * 1024) { throw new TabletException(Language.Current.TabletFileTooLarge); }

        return file.Extension.ToUpperInvariant() switch
        {
            ".PDF" => "application/pdf",
            ".EPUB" => "application/epub+zip",
            _ => throw new TabletException(Language.Current.TabletFileTypeNotSupported(file.Extension)),
        };
    }
}
