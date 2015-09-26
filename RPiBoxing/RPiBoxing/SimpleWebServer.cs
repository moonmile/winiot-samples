using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RPiGripperArm
{
/// <summary>
/// 簡易WebServerクラス
/// 単純な GET コマンドのみ対応する
/// </summary>
public class SimpleWebServer
{
    StreamSocketListener listener;
    StreamSocket socket;
    public event Action<string> OnReceived;

    // public Windows.Networking.HostName LOCALHOST { get; set; }
    public int PORT { get; set; }
    public SimpleWebServer()
    {
        // this.LOCALHOST = NetworkInformation.GetHostNames().Where(n => n.Type == Windows.Networking.HostNameType.Ipv4).First();
        // this.LOCALHOST = new Windows.Networking.HostName("localhost");
        this.PORT = 8081;
    }

    /// <summary>
    /// 受付開始
    /// </summary>
    public async void Start()
    {
        listener = new StreamSocketListener();
        listener.ConnectionReceived += Listener_ConnectionReceived;
        // await listener.BindEndpointAsync(LOCALHOST, PORT.ToString());
        await listener.BindServiceNameAsync(PORT.ToString());
    }
    private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        socket = args.Socket;
        var dr = new DataReader(socket.InputStream);

        /// GET ヘッダを取り出し
        StringBuilder request = new StringBuilder();
        uint BufferSize = 1024;
        using (IInputStream input = socket.InputStream)
        {
            byte[] data = new byte[BufferSize];
            IBuffer buffer = data.AsBuffer();
            uint dataRead = BufferSize;
            while (dataRead == BufferSize)
            {
                await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                dataRead = buffer.Length;
            }
        }
        // GET method を取り出し
        string requestMethod = request.ToString().Split('\n')[0];
        string[] requestParts = requestMethod.Split(' ');
        var text = requestParts[1];

        /// GETコマンドの受信イベント
        if (this.OnReceived != null)
        {
            OnReceived(text);
        }
    }
    /// <summary>
    /// レスポンスを返す
    /// </summary>
    /// <param name="text"></param>
    public async void SendResponse( string text )
    {
        if (socket == null) return;

        byte[] bodyArray = Encoding.UTF8.GetBytes(text);
        MemoryStream stream = new MemoryStream(bodyArray);
        string header = String.Format("HTTP/1.1 200 OK\r\n" +
                            "Content-Length: {0}\r\n" +
                            "Connection: close\r\n\r\n",
                            stream.Length);
        var dw = new DataWriter(socket.OutputStream);
        dw.WriteString(header);
        dw.WriteString(text);
        await dw.StoreAsync();
    }

        /// <summary>
        /// 受付停止
        /// </summary>
        public void Stop()
        {
            listener.Dispose();
            listener = null;
        }
        /// <summary>
        /// 受付終了
        /// </summary>
        public void Close()
        {
            this.Stop();
        }

    }
}
