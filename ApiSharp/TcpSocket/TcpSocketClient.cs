namespace ApiSharp.TcpSocket;

/// <summary>
/// TCP Socket Client
/// </summary>
public class TcpSocketClient
{
    #region Public Properties
    /// <summary>
    /// Host name or IP address of the server to connect to.
    /// </summary>
    public string Host
    {
        get { return _host; }
        set
        {
            if (Connected)
                throw (new TcpSocketClientException("Socket Client is already connected. You cant change this property while connected."));

            _host = value;
        }
    }

    /// <summary>
    /// Port number of the server to connect to.
    /// </summary>
    public int Port
    {
        get { return _port; }
        set
        {
            if (Connected)
                throw (new TcpSocketClientException("Socket Client is already connected. You cant change this property while connected."));

            _port = value;
        }
    }

    /// <summary>
    /// NoDelay option for the socket, which disables Nagle's algorithm to send data immediately.
    /// </summary>
    public bool NoDelay
    {
        get { return _nodelay; }
        set
        {
            _nodelay = value;
            if (_socket != null) _socket.NoDelay = value;
        }
    }

    /// <summary>
    /// Keep-alive option for the socket, which enables TCP keep-alive packets to be sent.
    /// </summary>
    public bool KeepAlive
    {
        get { return _keepAlive; }
        set
        {
            if (Connected)
                throw (new TcpSocketClientException("Socket Client is already connected. You cant change this property while connected."));

            _keepAlive = value;
        }
    }

    /// <summary>
    /// Keep-alive time in seconds, which is the time the connection must be idle before keep-alive packets are sent.
    /// </summary>
    public int KeepAliveTime
    {
        get { return _keepAliveTime; }
        set
        {
            if (Connected)
                throw (new TcpSocketClientException("Socket Client is already connected. You cant change this property while connected."));

            _keepAliveTime = value;
        }
    }

    /// <summary>
    /// Keep-alive interval in seconds
    /// </summary>
    public int KeepAliveInterval
    {
        get { return _keepAliveInterval; }
        set
        {
            if (Connected)
                throw (new TcpSocketClientException("Socket Client is already connected. You cant change this property while connected."));

            _keepAliveInterval = value;
        }
    }

    /// <summary>
    /// Keep-alive retry count, which is the number of keep-alive packets to send before considering the connection dead.
    /// </summary>
    public int KeepAliveRetryCount
    {
        get { return _keepAliveRetryCount; }
        set
        {
            if (Connected)
                throw (new TcpSocketClientException("Socket Client is already connected. You cant change this property while connected."));

            _keepAliveRetryCount = value;
        }
    }

    /// <summary>
    /// Receive buffer size in bytes, which is the size of the buffer used for receiving data.
    /// </summary>
    public int ReceiveBufferSize
    {
        get { return _receiveBufferSize; }
        set
        {
            _receiveBufferSize = value;
            _recvBuffer = new byte[value];
            if (_socket != null) _socket.ReceiveBufferSize = value;
        }
    }

    /// <summary>
    /// Receive timeout in milliseconds, which is the time to wait for data before timing out.
    /// </summary>
    public int ReceiveTimeout
    {
        get { return _receiveTimeout; }
        set
        {
            _receiveTimeout = value;
            if (_socket != null) _socket.ReceiveTimeout = value;
        }
    }

    /// <summary>
    /// Send buffer size in bytes, which is the size of the buffer used for sending data.
    /// </summary>
    public int SendBufferSize
    {
        get { return _sendBufferSize; }
        set
        {
            _sendBufferSize = value;
            _sendBuffer = new byte[value];
            if (_socket != null) _socket.SendBufferSize = value;
        }
    }

    /// <summary>
    /// Send timeout in milliseconds, which is the time to wait for data to be sent before timing out.
    /// </summary>
    public int SendTimeout
    {
        get { return _sendTimeout; }
        set
        {
            _sendTimeout = value;
            if (_socket != null) _socket.SendTimeout = value;
        }
    }

    /// <summary>
    /// Bytes received and sent by the socket client.
    /// </summary>
    public long BytesReceived
    {
        get { return _bytesReceived; }
        internal set { _bytesReceived = value; }
    }

    /// <summary>
    /// Bytes sent by the socket client.
    /// </summary>
    public long BytesSent
    {
        get { return _bytesSent; }
        internal set { _bytesSent = value; }
    }

    /// <summary>
    /// Reconnect option, which indicates whether the client should attempt to reconnect if the connection is lost.
    /// </summary>
    public bool Reconnect
    {
        get { return _reconnect; }
        set { _reconnect = value; }
    }

    /// <summary>
    /// Reconnect delay in seconds, which is the time to wait before attempting to reconnect after a disconnection.
    /// </summary>
    public int ReconnectDelayInSeconds
    {
        get { return _reconnectDelay; }
        set { _reconnectDelay = value; }
    }

    /// <summary>
    /// Accept data option, which indicates whether the client should accept and process incoming data.
    /// </summary>
    public bool AcceptData
    {
        get { return _acceptData; }
        set { _acceptData = value; }
    }

    /// <summary>
    /// Checks if the socket client is currently connected to the server.
    /// </summary>
    public bool Connected { get { return this._socket != null && this._socket.Connected; } }
    #endregion

    #region Private Properties
    private string _host = "";
    private int _port;
    private bool _nodelay = true;
    private bool _keepAlive = false;
    private int _keepAliveTime = 900;
    private int _keepAliveInterval = 300;
    private int _keepAliveRetryCount = 5;
    private int _receiveBufferSize = 8192;
    private int _receiveTimeout = 0;
    private int _sendBufferSize = 8192;
    private int _sendTimeout = 0;
    private long _bytesReceived;
    private long _bytesSent;
    private bool _reconnect = false;
    private int _reconnectDelay = 5;
    private bool _acceptData = true;
    #endregion

    #region Public Events
    /// <summary>
    /// OnError event, raised when an error occurs in the TCP socket client.
    /// </summary>
    public event EventHandler<OnClientErrorEventArgs> OnError = delegate { };

    /// <summary>
    /// Occurs when a client successfully connects to the server.
    /// </summary>
    /// <remarks>This event is triggered after a client establishes a connection.  Subscribers can use this
    /// event to perform actions such as initializing resources  or sending a welcome message to the connected
    /// client.</remarks>
    public event EventHandler<OnClientConnectedEventArgs> OnConnected = delegate { };

    /// <summary>
    /// Occurs when a client disconnects from the server.
    /// </summary>
    public event EventHandler<OnClientDisconnectedEventArgs> OnDisconnected = delegate { };

    /// <summary>
    /// Occurs when data is received from the server.
    /// </summary>
    public event EventHandler<OnClientDataReceivedEventArgs> OnDataReceived = delegate { };
    #endregion

    #region Private Fields
    private System.Net.Sockets.Socket _socket;
    private byte[] _recvBuffer = [];
    private byte[] _sendBuffer = [];
    #endregion

    #region Receiver Thread
    private Thread _thread;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructs a new instance of the TcpSocketClient class with default host and port.
    /// </summary>
    public TcpSocketClient() : this("127.0.0.1", 1024)
    {
    }

    /// <summary>
    /// Constructs a new instance of the TcpSocketClient class with the specified host and port.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public TcpSocketClient(string host, int port)
    {
        this.Host = host;
        this.Port = port;
        this._cancellationTokenSource = new CancellationTokenSource();
        this._cancellationToken = this._cancellationTokenSource.Token;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Connects to the specified host and port using a TCP socket.
    /// </summary>
    /// <exception cref="TcpSocketClientException"></exception>
    public void Connect()
    {
        try
        {
            // Buffers
            this._recvBuffer = new byte[ReceiveBufferSize];
            this._sendBuffer = new byte[SendBufferSize];

            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            var serverIPHost = Dns.GetHostEntry(Host);
            if (serverIPHost.AddressList.Length == 0) throw new TcpSocketClientException("Unable to solve host address");
            var serverIPAddress = serverIPHost.AddressList[0];
            if (serverIPAddress.ToString() == "::1") serverIPAddress = new IPAddress(16777343); // 127.0.0.1
            var serverIPEndPoint = new IPEndPoint(serverIPAddress, Port);

            // Create a TCP/IP  socket.    
            this._socket = new System.Net.Sockets.Socket(serverIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Set Properties
            this._socket.NoDelay = this.NoDelay;
            this._socket.ReceiveBufferSize = this.ReceiveBufferSize;
            this._socket.ReceiveTimeout = this.ReceiveTimeout;
            this._socket.SendBufferSize = this.SendBufferSize;
            this._socket.SendTimeout = this.SendTimeout;

            /* Keep Alive */
            if (this.KeepAlive && this.KeepAliveInterval > 0)
            {
#if NETCOREAPP || NET5_0 || NET6_0 || NET7_0 || NET8_0_OR_GREATER
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, this.KeepAliveTime);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, this.KeepAliveInterval);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, this.KeepAliveRetryCount);
#elif NETFRAMEWORK
                // Get the size of the uint to use to back the byte array
                int size = Marshal.SizeOf((uint)0);

                // Create the byte array
                byte[] keepAlive = new byte[size * 3];

                // Pack the byte array:
                // Turn keepalive on
                Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, keepAlive, 0, size);

                // How long does it take to start the first probe (in milliseconds)
                Buffer.BlockCopy(BitConverter.GetBytes((uint)(KeepAliveTime*1000)), 0, keepAlive, size, size);

                // Detection time interval (in milliseconds)
                Buffer.BlockCopy(BitConverter.GetBytes((uint)(KeepAliveInterval*1000)), 0, keepAlive, size * 2, size);

                // Set the keep-alive settings on the underlying Socket
                _socket.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);
#elif NETSTANDARD
                // Set the keep-alive settings on the underlying Socket
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#endif
            }

            // Connect to Remote EndPoint
            _socket.Connect(serverIPEndPoint);

            // Start Receiver Thread
            this._cancellationTokenSource = new CancellationTokenSource();
            this._cancellationToken = this._cancellationTokenSource.Token;
            this._thread = new Thread(ReceiverThreadAction);
            this._thread.Start();

            // Invoke OnConnected
            this.InvokeOnConnected(new OnClientConnectedEventArgs
            {
                ServerHost = this.Host,
                ServerPort = this.Port,
            });
        }
        catch (Exception ex)
        {
            // Invoke OnError
            this.InvokeOnError(new OnClientErrorEventArgs
            {
                Exception = ex
            });
        }
    }

    /// <summary>
    /// Disconnects from the server and releases the socket resources.
    /// </summary>
    public void Disconnect()
    {
        this.Disconnect(TcpSocketDisconnectReason.None);
    }

    /// <summary>
    /// Sends a byte array to the server.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public long SendBytes(byte[] bytes)
    {
        // Check Point
        if (!this.Connected) return 0;

        // Action
        var sent = this._socket.Send(bytes);
        this.BytesSent += sent;

        // Return
        return sent;
    }

    /// <summary>
    /// Sends a byte array to the server with a specified offset and count.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public long SendString(string data)
    {
        // Check Point
        if (!this.Connected) return 0;

        // Action
        var bytes = Encoding.UTF8.GetBytes(data);
        var sent = this._socket.Send(bytes);
        this.BytesSent += sent;

        // Return
        return sent;
    }

    /// <summary>
    /// Sends a string to the server using the specified encoding.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public long SendString(string data, Encoding encoding)
    {
        // Check Point
        if (!this.Connected) return 0;

        // Action
        var bytes = encoding.GetBytes(data);
        var sent = this._socket.Send(bytes);
        this.BytesSent += sent;

        // Return
        return sent;
    }

    /// <summary>
    /// Sends a file to the server using the specified file path.
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
        this._socket.SendFile(filePath);
        this.BytesSent += fileInfo.Length;

        // Return
        return fileInfo.Length;
    }

    /// <summary>
    /// Sends a file to the server with optional pre and post buffers, and transmit file options.
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
        this._socket.SendFile(filePath, preBuffer, postBuffer, flags);
        this.BytesSent += fileInfo.Length;

        // Return
        return fileInfo.Length;
    }
    #endregion

    #region Private Methods
    //Burada bir tane de bağllantının açık olup olmadığını kontrol eden ayrı bir thread daha çalışmalı
    private void ReceiverThreadAction()
    {
        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                // Receive the response from the remote device.    
                var bytesCount = _socket.Receive(_recvBuffer);
                if (bytesCount > 0)
                {
                    BytesReceived += bytesCount;
                    if (this.AcceptData)
                    {
                        var bytes = new byte[bytesCount];
                        Array.Copy(_recvBuffer, bytes, bytesCount);

                        // Invoke OnDataReceived
                        this.InvokeOnDataReceived(new OnClientDataReceivedEventArgs
                        {
                            Data = bytes
                        });
                    }
                }
            }
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode == SocketError.ConnectionAborted)
            {
                Disconnect(TcpSocketDisconnectReason.ServerAborted);
            }
        }
        catch (Exception ex)
        {
            // Invoke OnError
            this.InvokeOnError(new OnClientErrorEventArgs
            {
                Exception = ex
            });

            // Disconnect
            Disconnect(TcpSocketDisconnectReason.Exception);
        }
    }

    private void Disconnect(TcpSocketDisconnectReason reason)
    {
        // Release the socket.    
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();

        // Stop Receiver Thread
        this._cancellationTokenSource.Cancel();

        // Invoke OnDisconnected
        this.InvokeOnDisconnected(new OnClientDisconnectedEventArgs());

        // Reconnect
        if (this.Reconnect)
        {
            while (!this.Connected)
            {
                Task.Delay(this.ReconnectDelayInSeconds * 1000);
                this.Connect();
            }
        }
    }
    #endregion

    #region Event Invokers
    internal void InvokeOnError(OnClientErrorEventArgs args)
    {
        this.OnError?.Invoke(this, args);
    }

    internal void InvokeOnConnected(OnClientConnectedEventArgs args)
    {
        this.OnConnected?.Invoke(this, args);
    }

    internal void InvokeOnDisconnected(OnClientDisconnectedEventArgs args)
    {
        this.OnDisconnected?.Invoke(this, args);
    }

    internal void InvokeOnDataReceived(OnClientDataReceivedEventArgs args)
    {
        this.OnDataReceived?.Invoke(this, args);
    }
    #endregion
}
