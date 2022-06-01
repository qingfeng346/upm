using System;
using System.Net.Sockets;
namespace Scorpio.Net {
    using ActionClosed = Action<SocketError, string>;
    public enum State {
        None,           //无状态
        Connecting,     //正在连接
        Connected,      //已连接状态
        Listened,       //监听状态
        Accepted,       //已连接状态
    }
    public class OpClosed : OpMessage {
        public SocketError error;
        public string errorMsg;
        public void Process(ScorpioSocket socket) {
            if (socket.OnClosed != null) socket.OnClosed(error, errorMsg);
        }
    }
    public abstract partial class ScorpioSocket {
        public static int SendTimeOut = 10000;
        public static int RecvTimeOut = 10000;
        public int HeadLength = 4;
        private const int MAX_BUFFER_LENGTH = 8192;                 // 缓冲区大小
        protected Socket m_Socket = null;                           // Socket句柄
        protected State m_State;                                    // 当前状态
        public ActionClosed OnClosed;                               // 链接关闭回调
        public ScorpioSocket() {
            m_RecvEvent = new SocketAsyncEventArgs();
            m_RecvEvent.SetBuffer(new byte[MAX_BUFFER_LENGTH], 0, MAX_BUFFER_LENGTH);
            m_RecvEvent.Completed += RecvAsyncCompleted;
            m_SendEvent = new SocketAsyncEventArgs();
            m_SendEvent.Completed += SendAsyncCompleted;
        }
        public State State { get { return m_State; } }
        public Socket Socket { get { return m_Socket; } }
        public bool IsConnected { get { return m_Socket.Connected; } }
        public bool IsClosed { get { return m_State == State.None; } }
        //初始化
        protected void Initialize() {
            m_RecvTotalSize = 0;
            Array.Clear(m_RecvTotalBuffer, 0, m_RecvTotalBuffer.Length);
            Array.Clear(m_RecvEvent.Buffer, 0, m_RecvEvent.Buffer.Length);
            BeginSend();
            BeginReceive();
        }
        public void Close() {
            Disconnect(SocketError.OperationAborted, "");
        }
        void Disconnect(SocketError error, string errorMsg) {
            if (m_State == State.None) { return; }
            m_State = State.None;
            try {
                using (m_Socket) {
                    m_Socket.Shutdown(SocketShutdown.Both);
                    m_Socket.Close();
                }
            } catch (System.Exception e) {
                LogWarn("Disconnect is error : " + e.ToString());
            } finally {
                EnqueueOpMessage(new OpClosed() { error = error, errorMsg = errorMsg });
            }
        }
        void LogError(string msg) {
            logger.error(msg);
        }
        void LogWarn(string msg) {
            logger.warn(msg);
        }
    }
}
