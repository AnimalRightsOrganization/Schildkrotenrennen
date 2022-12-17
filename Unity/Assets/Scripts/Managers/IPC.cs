using System;
using System.Text;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class IPC
{
    private NamedPipeClientStream _client;

    public string PipeName { get; set; } = "UnityNamedPipe";

    public int ReceiveTimeout { get; set; }

    public async Task<string> Send(string message)
    {
        try
        {
            Connect();
            await SendToUnity(message);
            return await ReceiveFromUnity();
        }
        finally
        {
            if (_client != null)
            {
                //await _client.DisposeAsync(); //Net4.0无法异步
                _client.Dispose();
            }
        }
    }

    private void Connect()
    {
        _client = new NamedPipeClientStream(PipeName);
        //await _client.ConnectAsync(1000); //Net4.0无法异步
        _client.Connect();
    }

    private async Task SendToUnity(string message)
    {
        if (_client == null)
            throw new InvalidOperationException();
        var bytes = Encoding.UTF8.GetBytes(message);
        await _client.WriteAsync(bytes, 0, bytes.Length);
        await _client.FlushAsync();
    }

    private async Task<string> ReceiveFromUnity()
    {
        if (_client == null)
            throw new InvalidOperationException();
        var buffer = new byte[1024];
        using var cancel = new CancellationTokenSource();
        if (ReceiveTimeout > 0)
            cancel.CancelAfter(ReceiveTimeout);
        var len = await _client.ReadAsync(buffer, 0, buffer.Length, cancel.Token);
        return Encoding.UTF8.GetString(buffer, 0, len);
    }
}