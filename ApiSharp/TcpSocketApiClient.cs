namespace ApiSharp;

/// <summary>
/// Tcp Socket Api Client
/// </summary>
public abstract class TcpSocketApiClient : BaseClient
{
    /// <summary>
    /// Client Options
    /// </summary>
    public TcpSocketApiClientOptions ClientOptions { get { return (TcpSocketApiClientOptions)base._options; } }

    /// <summary>
    /// Bytes Sent
    /// </summary>
    public long BytesSent { get; private set; }

    /// <summary>
    /// Bytes Received
    /// </summary>
    public long BytesReceived { get; private set; }

    /// <summary>
    /// Packets Sent
    /// </summary>
    public long PacketsSent { get; private set; }

    /// <summary>
    /// Packets Received
    /// </summary>
    public long PacketsReceived { get; private set; }

    /// <summary>
    /// Called when a connection is successfully established.
    /// </summary>
    /// <remarks>This method is invoked to handle logic that should occur immediately after a connection is
    /// established. Derived classes must implement this method to define specific behavior upon connection.</remarks>
    protected abstract void OnConnected();

    /// <summary>
    /// Executes custom logic when a disconnection occurs.
    /// </summary>
    /// <remarks>This method is called when the connection is terminated. Derived classes must override this
    /// method to define specific behavior that should occur upon disconnection. The implementation should handle any
    /// necessary cleanup or state updates related to the disconnection.</remarks>
    protected abstract void OnDisconnected();

    /// <summary>
    /// Invoked when an error occurs during the execution of the derived class.
    /// </summary>
    /// <remarks>This method must be implemented by derived classes to define custom error-handling behavior.
    /// It is called automatically when an error condition is detected.</remarks>
    protected abstract void OnError();

    /// <summary>
    /// Handles the processing of a received packet based on its type and content.
    /// </summary>
    /// <remarks>Derived classes must implement this method to define the specific behavior for handling
    /// packets of various types. The implementation should account for the expected format and meaning of the <paramref
    /// name="dataBody"/> based on the <paramref name="dataType"/>.</remarks>
    /// <param name="dataType">The type of the packet, represented as a byte. This value determines how the packet should be processed.</param>
    /// <param name="dataBody">The content of the packet, represented as a byte array. This array contains the data associated with the packet.</param>
    protected abstract void OnPacketReceived(byte dataType, byte[] dataBody);

    /// <summary>
    /// Handles the timer's elapsed event to perform a periodic operation.
    /// </summary>
    /// <remarks>This method is abstract and must be implemented by a derived class to define the specific
    /// behavior that should occur when the timer elapses.</remarks>
    /// <param name="source">The source of the timer event, typically the timer instance that triggered the event.</param>
    /// <param name="e">The event data containing information about the elapsed event.</param>
    protected abstract void PingTimer(object source, System.Timers.ElapsedEventArgs e);

    // Private Properties
    private readonly System.Timers.Timer _hbTimer;
    private readonly List<byte> _socketBuffer = [];
    private readonly BlockingCollection<byte[]> _packetBuffer = [];
    private readonly TcpSocketClient _socketClient;

    /// <summary>
    /// Gets or sets the <see cref="CancellationTokenSource"/> used to signal cancellation for ongoing operations.
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; set; } = new();

    /// <summary>
    /// Gets or sets the <see cref="System.Threading.CancellationToken"/> used to propagate notification  that
    /// operations should be canceled.
    /// </summary>
    protected CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    /// <summary>
    /// Constructor
    /// </summary>
    protected TcpSocketApiClient() : this(null, new())
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    protected TcpSocketApiClient(ILogger? logger, TcpSocketApiClientOptions options) : base(logger, options ?? new())
    {
        // Tcp Client
        _socketClient = new TcpSocketClient(ClientOptions.ServerHost, ClientOptions.ServerPort);
        _socketClient.OnConnected += Client_OnConnected;
        _socketClient.OnDisconnected += Client_OnDisconnected;
        _socketClient.OnDataReceived += Client_OnDataReceived;
        _socketClient.OnError += Client_OnError;
        _socketClient.KeepAlive = true;

        // Heart Beat
        this._hbTimer = new System.Timers.Timer(ClientOptions.HeartBeatInterval);
        this._hbTimer.Elapsed += PingTimer;
    }

    /// <summary>
    /// Connect
    /// </summary>
    public void Connect()
    {
        if (!_socketClient.Connected)
            _socketClient.Connect();
    }

    /// <summary>
    /// Disconnect
    /// </summary>
    public void Disconnect()
    {
        if (_socketClient.Connected)
            _socketClient.Disconnect();
    }

    /// <summary>
    /// Send
    /// </summary>
    /// <param name="bytes"></param>
    protected void Send(byte[] bytes)
    {
        if (_socketClient.Connected)
        {
            _socketClient.SendBytes(bytes);
            BytesSent += bytes.Length;
            PacketsSent++;
        }
    }

    private void Client_OnConnected(object sender, OnClientConnectedEventArgs e)
    {
        // Check Point
        if (!_socketClient.Connected) return;

        // Cancellation Token
        this.CancellationTokenSource = new CancellationTokenSource();
        this.CancellationToken = this.CancellationTokenSource.Token;

        // Packet Consumer
        Task.Factory.StartNew(PacketConsumer, TaskCreationOptions.LongRunning);

        // Heart Beat
        if (ClientOptions.HeartBeatEnabled)
            this._hbTimer.Start();

        // Execute OnConnected Logic
        this.OnConnected();
    }

    private void Client_OnDisconnected(object sender, OnClientDisconnectedEventArgs e)
    {
        // Cancellation Token
        if (!this.CancellationTokenSource.IsCancellationRequested)
            this.CancellationTokenSource.Cancel();

        // Disable Heart Beat
        if (ClientOptions.HeartBeatEnabled)
            this._hbTimer.Stop();

        // Execute OnDisconnected Logic
        this.OnDisconnected();
    }

    private void Client_OnDataReceived(object sender, OnClientDataReceivedEventArgs e)
    {
        BytesReceived += e.Data.Length;
        OnDataIn(e.Data);
    }

    private void Client_OnError(object sender, OnClientErrorEventArgs e)
    {
        // Execute OnError Logic
        this.OnError();
    }

    private void OnDataIn(byte[] bytes, int connectionId = 0)
    {
        try
        {
            // Gelen verileri buffer'a ekle ve bu halini "buff" olarak al. Sonrasında bufferı temizle
            _socketBuffer.AddRange(bytes);
            var buff = _socketBuffer.ToArray();

            // Minimum paket uzunluğu 8 byte
            // * SYNC     : 2 Bytes
            // * Length   : 2 Bytes
            // * Data Type: 1 Byte
            // * Content  : 1 Byte(Minimum)
            // * CRC16    : 2 Bytes
            // * CRC32    : 4 Bytes

            var crcLength = 0;
            var syncLength = ClientOptions.HeaderBytes.Length;
            var lengthLength = 2;
            var dataTypeLength = 1;
            var minimumDataLength = 1;
            var minimumPacketLength = syncLength + lengthLength + dataTypeLength + crcLength + minimumDataLength;
            if (ClientOptions.SocketSecurity == TcpSocketSecurity.CRC16) crcLength = 2;
            else if (ClientOptions.SocketSecurity == TcpSocketSecurity.CRC32) crcLength = 4;

            if (buff.Length >= minimumPacketLength)
            {
                var indexOf = buff.IndexOf(ClientOptions.HeaderBytes);
                if (indexOf == -1) _socketBuffer.Clear();
                else if (indexOf == 0) // SYNC Bytes
                {
                    // lenghtValue = Data Type (1) + Content (X)
                    // lenghtValue CRC bytelarını kapsamıyor.
                    var lenghtValue = BitConverter.ToUInt16(buff, lengthLength);

                    // Paket yeterki kadar büyük mü? 
                    // Paketin gereğinden fazla büyük olması sorun değil.
                    var packetLength = syncLength + lengthLength + lenghtValue + crcLength;
                    if (buff.Length >= packetLength)
                    {
                        // CRC-Body'i ayarlayalım
                        var crcBody = new byte[lenghtValue];
                        var preBytesLength = syncLength + lengthLength;
                        Array.Copy(buff, preBytesLength, crcBody, 0, lenghtValue);

                        // Check CRC & Consume
                        try
                        {
                            // Check Point
                            var consume = false;
                            if (ClientOptions.SocketSecurity == TcpSocketSecurity.None)
                            {
                                consume = true;
                            }
                            else if (ClientOptions.SocketSecurity == TcpSocketSecurity.CRC16)
                            {
                                var crcBytes = new byte[crcLength];
                                Array.Copy(buff, lenghtValue + preBytesLength, crcBytes, 0, crcLength);
                                var crcValue = BitConverter.ToUInt16(crcBytes, 0);
                                consume = CRC16.CheckChecksum(crcBody, crcValue);
                            }
                            else if (ClientOptions.SocketSecurity == TcpSocketSecurity.CRC32)
                            {
                                var crcBytes = new byte[crcLength];
                                Array.Copy(buff, lenghtValue + preBytesLength, crcBytes, 0, crcLength);
                                var crcValue = BitConverter.ToUInt32(crcBytes, 0);
                                consume = CRC32.CheckChecksum(crcBody, crcValue);
                            }

                            // Consume
                            if (consume)
                            {
                                OnPacketReady(crcBody, connectionId);
                            }
                        }
                        catch { }

                        // Consume edilen veriyi buffer'dan at
                        var bufferLength = _socketBuffer.Count;
                        _socketBuffer.RemoveRange(0, packetLength);

                        // Arta kalanları veri için bu methodu yeniden çalıştır
                        if (bufferLength > packetLength)
                            OnDataIn([], connectionId);
                    }
                }
                else
                {
                    _socketBuffer.RemoveRange(0, indexOf);
                    OnDataIn([], connectionId);
                }
            }
        }
        catch { }
    }

    private void OnPacketReady(byte[] bytes, int connectionId = 0)
    {
        PacketsReceived++;
        _packetBuffer.TryAdd(bytes);
    }

    private void PacketConsumer()
    {
        try
        {
            foreach (var item in _packetBuffer.GetConsumingEnumerable(this.CancellationToken))
            {
                try
                {
                    // Minimum 2 bytes
                    // Data Type (1) + Content (X)
                    if (item.Length < 2)
                        continue;

                    // Parse Bytes
                    var dataType = item[0];
                    var dataBody = new byte[item.Length - 1];

                    // Execute OnPacketReceived Logic
                    this.OnPacketReceived(dataType, dataBody);
                }
                catch { }
            }
        }
        catch { }
    }

}