namespace ApiSharp;

public abstract class SocketApiClient : BaseClient
{
    public new SocketApiClientOptions Options { get { return (SocketApiClientOptions)base.Options; } }

    // Public Properties
    public long BytesSent { get; private set; }
    public long BytesReceived { get; private set; }
    public long PacketsSent { get; private set; }
    public long PacketsReceived { get; private set; }

    // Protected Abstract Methods
    protected abstract void OnConnected();
    protected abstract void OnDisconnected();
    protected abstract void OnError();
    protected abstract void OnPacketReceived(byte dataType, byte[] dataBody);
    protected abstract void PingTimer(object source, System.Timers.ElapsedEventArgs e);

    // Private Properties
    private readonly System.Timers.Timer _hbTimer;
    private readonly List<byte> _socketBuffer = new();
    private readonly BlockingCollection<byte[]> _packetBuffer = new();
    private readonly TcpSharpSocketClient _socketClient;

    // Cancellation Token
    protected CancellationTokenSource CancellationTokenSource { get; set; }
    protected CancellationToken CancellationToken { get; set; }

    protected SocketApiClient() : this("", new())
    {
    }

    protected SocketApiClient(string name, SocketApiClientOptions options) : base(name, options ?? new())
    {
        // Tcp Client
        _socketClient = new TcpSharpSocketClient(Options.ServerHost, Options.ServerPort);
        _socketClient.OnConnected += Client_OnConnected;
        _socketClient.OnDisconnected += Client_OnDisconnected;
        _socketClient.OnDataReceived += Client_OnDataReceived;
        _socketClient.OnError += Client_OnError;
        _socketClient.KeepAlive = true;

        // Heart Beat
        this._hbTimer = new System.Timers.Timer(Options.HeartBeatInterval);
        this._hbTimer.Elapsed += PingTimer;
    }

    public void Connect()
    {
        if (!_socketClient.Connected)
            _socketClient.Connect();
    }

    public void Disconnect()
    {
        if (_socketClient.Connected)
            _socketClient.Disconnect();
    }

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
        if (Options.HeartBeatEnabled)
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
        if (Options.HeartBeatEnabled)
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
            var syncLength = Options.HeaderBytes.Length;
            var lengthLength = 2;
            var dataTypeLength = 1;
            var minimumDataLength = 1;
            var minimumPacketLength = syncLength + lengthLength + dataTypeLength + crcLength + minimumDataLength;
            if (Options.SocketSecurity == SocketSecurity.CRC16) crcLength = 2;
            else if (Options.SocketSecurity == SocketSecurity.CRC32) crcLength = 4;

            if (buff.Length >= minimumPacketLength)
            {
                var indexOf = buff.IndexOf(Options.HeaderBytes);
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
                            if (Options.SocketSecurity == SocketSecurity.None)
                            {
                                consume = true;
                            }
                            else if (Options.SocketSecurity == SocketSecurity.CRC16)
                            {
                                var crcBytes = new byte[crcLength];
                                Array.Copy(buff, lenghtValue + preBytesLength, crcBytes, 0, crcLength);
                                var crcValue = BitConverter.ToUInt16(crcBytes, 0);
                                consume = CRC16.CheckChecksum(crcBody, crcValue);
                            }
                            else if (Options.SocketSecurity == SocketSecurity.CRC32)
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
                        var bufferLenght = _socketBuffer.Count;
                        _socketBuffer.RemoveRange(0, packetLength);

                        // Arta kalanları veri için bu methodu yeniden çalıştır
                        if (bufferLenght > packetLength)
                            OnDataIn(Array.Empty<byte>(), connectionId);
                    }
                }
                else
                {
                    _socketBuffer.RemoveRange(0, indexOf);
                    OnDataIn(Array.Empty<byte>(), connectionId);
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