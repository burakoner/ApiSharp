namespace ApiSharp.TcpSocket;

/// <summary>
/// TCP Socket Server Client
/// </summary>
public class TcpSocketServerClient
{
    /// <summary>
    /// Client that is connected to the server.
    /// </summary>
    public TcpClient Client { get; internal set; }

    /// <summary>
    /// ConnectionId of the client.
    /// </summary>
    public long ConnectionId { get; internal set; }

    /// <summary>
    /// Checks if the client is connected to the server.
    /// </summary>
    public bool Connected { get { return this.Client != null && this.Client.Connected; } }

    /// <summary>
    /// Accepts data from the client. If set to false, the server will not invoke OnDataReceived for this client.
    /// </summary>
    public bool AcceptData { get; internal set; } = true;

    /// <summary>
    /// Bytes received from the client.
    /// </summary>
    public long BytesReceived { get; private set; }

    /// <summary>
    /// Bytes sent to the client.
    /// </summary>
    public long BytesSent { get; private set; }

    /* Reference Fields */
    private readonly TcpSocketServer _server;

    /* Private Fields */
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;

    internal TcpSocketServerClient(TcpSocketServer server, TcpClient client, long connectionId)
    {
        this.Client = client;
        this.ConnectionId = connectionId;

        this._server = server;

        this._thread = new Thread(async () => await ConnectionHandler());
        this._cancellationTokenSource = new CancellationTokenSource();
        this._cancellationToken = this._cancellationTokenSource.Token;
    }

    internal void StartReceiving()
    {
        this._thread.Start();
    }

    internal void StopReceiving()
    {
        this._cancellationTokenSource.Cancel();
    }

    private async Task ConnectionHandler()
    {
        var stream = this.Client.GetStream();
        var buffer = new byte[this.Client.ReceiveBufferSize];
#if RELEASE
        try
        {
#endif
            var bytesCount = 0;
            while ((bytesCount = await stream.ReadAsync(buffer, 0, buffer.Length, this._cancellationToken)) != 0)
            {
                // Increase BytesReceived
                BytesReceived += bytesCount;
                this._server.AddReceivedBytes(bytesCount);

                // Invoke OnDataReceived
                if (this.AcceptData)
                {
                    var bytesReceived = new byte[bytesCount];
                    Array.Copy(buffer, bytesReceived, bytesCount);
                    this._server.InvokeOnDataReceived(new OnServerDataReceivedEventArgs
                    {
                        Client = this.Client,
                        ConnectionId = this.ConnectionId,
                        Data = bytesReceived
                    });
                }
            }
#if RELEASE
        }
        catch (Exception ex)
        {
            // Invoke OnError
            this._server.InvokeOnError(new OnServerErrorEventArgs
            {
                Client = this.Client,
                ConnectionId = this.ConnectionId,
                Exception = ex
            });

            // Disconnect
            this._server.Disconnect(this.ConnectionId, TcpSocketDisconnectReason.Exception);
        }
#endif
    }

    /// <summary>
    /// Sends bytes to the client.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public long SendBytes(byte[] bytes)
    {
        if (!this.Connected) return 0;

        this.BytesSent += bytes.Length;
        this._server.AddSentBytes(bytes.Length);

        return this.Client.Client.Send(bytes);
    }

    /// <summary>
    /// Sends a string to the client using UTF8 encoding.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public long SendString(string data)
    {
        if (!this.Connected) return 0;

        var bytes = Encoding.UTF8.GetBytes(data);
        this.BytesSent += bytes.Length;
        this._server.AddSentBytes(bytes.Length);

        return this.Client.Client.Send(bytes);
    }

    /// <summary>
    /// Sends a string to the client using the specified encoding.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public long SendString(string data, Encoding encoding)
    {
        if (!this.Connected) return 0;

        var bytes = encoding.GetBytes(data);
        this.BytesSent += bytes.Length;
        this._server.AddSentBytes(bytes.Length);

        return this.Client.Client.Send(bytes);
    }

    /// <summary>
    /// Sends a file to the client.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public long SendFile(string filePath)
    {
        // Check Point
        if (!this.Connected) return 0;
        if (!File.Exists(filePath)) return 0;

        // FileInfo
        var fileInfo = new FileInfo(filePath);
        if (fileInfo == null) return 0;

        // Action
        this.Client.Client.SendFile(filePath);
        this.BytesSent += fileInfo.Length;
        this._server.AddSentBytes(fileInfo.Length);

        // Return
        return fileInfo.Length;
    }

    /// <summary>
    /// Sends a file to the client with pre and post buffers.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="preBuffer"></param>
    /// <param name="postBuffer"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public long SendFile(string filePath, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
    {
        // Check Point
        if (!this.Connected) return 0;
        if (!File.Exists(filePath)) return 0;

        // FileInfo
        var fileInfo = new FileInfo(filePath);
        if (fileInfo == null) return 0;

        // Action
        this.Client.Client.SendFile(filePath, preBuffer, postBuffer, flags);
        this.BytesSent += fileInfo.Length;
        this._server.AddSentBytes(fileInfo.Length);

        // Return
        return fileInfo.Length;
    }

}
