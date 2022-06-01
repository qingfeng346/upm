using System;
using System.Net;
using System.Net.Sockets;

namespace Scorpio.Net {
    using ActionConnected = System.Action<bool, string>;
    public class OpConnected : OpMessage<ClientSocket> {
        public bool success;
        public string error;
        public override void Process (ClientSocket socket) {
            if (socket.OnConnected != null) socket.OnConnected (string.IsNullOrEmpty(error), error);
        }
    }
    public class ClientSocket : ScorpioSocket {
        public ActionConnected OnConnected; // 连接回调
        private SocketAsyncEventArgs m_ConnectEvent; // 异步连接消息
        public ClientSocket () {
            m_ConnectEvent = new SocketAsyncEventArgs ();
            m_ConnectEvent.Completed += ConnectionAsyncCompleted;
        }
        public void Connect (string host, int port) {
            if (m_State != State.None) return;
            m_State = State.Connecting;
            try {
#if SCORPIO_DNSENDPOINT
                m_ConnectEvent.RemoteEndPoint = new DnsEndPoint (m_Host, m_Port);
#else
                if (host == "localhost") {
                    m_ConnectEvent.RemoteEndPoint = new IPEndPoint (IPAddress.Parse ("127.0.0.1"), port);
                } else if (IPAddress.TryParse (host, out var address)) {
                    m_ConnectEvent.RemoteEndPoint = new IPEndPoint (address, port);
                } else {
                    m_ConnectEvent.RemoteEndPoint = new IPEndPoint (Dns.GetHostEntry (host).AddressList[0], port);
                }
#endif
                m_Socket = new Socket (m_ConnectEvent.RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.NoDelay = false;
                m_Socket.SendTimeout = SendTimeOut;
                m_Socket.ReceiveTimeout = RecvTimeOut;
                m_Socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                if (!m_Socket.ConnectAsync (m_ConnectEvent)) {
                    ConnectionAsyncCompleted(this, m_ConnectEvent);
                    // OnConnectError ("连接服务器出错 " + host + ":" + port);
                }
            } catch (System.Exception ex) {
                OnConnectError ($"连接服务器出错Exception - [{host}:{port}] : {ex.ToString()}");
            };
        }
        void ConnectionAsyncCompleted (object sender, SocketAsyncEventArgs e) {
            if (e.SocketError != SocketError.Success) {
                OnConnectError ($"连接服务器出错SocketError - [{e.RemoteEndPoint.ToString()}] : {e.SocketError}");
                return;
            }
            m_State = State.Connected;
            Initialize ();
            EnqueueOpMessage (new OpConnected () { error = null });
        }
        void OnConnectError (string error) {
            m_State = State.None;
            EnqueueOpMessage (new OpConnected () { error = error });
        }
    }
}