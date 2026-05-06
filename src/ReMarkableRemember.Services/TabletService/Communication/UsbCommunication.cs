using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class UsbCommunication : CommunicationBase
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
            await this.httpClientConnection.GetStringAsync(new Uri($"http://{CommunicationManager.IP}/documents/")).ConfigureAwait(false);
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
            return await this.httpClient.GetStreamAsync(new Uri($"http://{CommunicationManager.IP}/download/{id}/placeholder")).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw ConvertToTabletException(exception);
        }
    }

    public async Task Upload(String? parentId, MultipartFormDataContent content)
    {
        try
        {
            await this.httpClient.GetStringAsync(new Uri($"http://{CommunicationManager.IP}/documents/{parentId}")).ConfigureAwait(false);

            HttpResponseMessage response = await this.httpClient.PostAsync(new Uri($"http://{CommunicationManager.IP}/upload"), content).ConfigureAwait(false);
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
}
