using System;
using System.Threading;
using System.Net.Sockets;

namespace Scorpio.Net {
    using ActionRecv = Action<byte[]>;
    public class OpRecv : OpMessage {
        public byte[] data;
        public void Process(ScorpioSocket socket) {
            if (socket.OnReceived != null) socket.OnReceived(data);
        }
    }
    public partial class ScorpioSocket {
        protected object m_RecvSync = new object();                             // 接收线程锁
        protected SocketAsyncEventArgs m_RecvEvent = null;                      // 异步接收消息

        private byte[] m_RecvTotalBuffer = new byte[MAX_BUFFER_LENGTH];         // 已经接收的数据总缓冲
        private int m_RecvTotalSize = 0;                                        // 接收总数据的长度
        public ActionRecv OnReceived;                                           // 接受消息回调

        //开始接收消息
        void BeginReceive() {
            Receive();
        }
        void Receive() {
            try {
                if (!m_Socket.ReceiveAsync(m_RecvEvent)) {
                    System.Threading.ThreadPool.QueueUserWorkItem((state) => {
                        RecvAsyncCompleted(this, m_RecvEvent);
                    });
                }
            } catch (System.Exception e) {
                Disconnect(SocketError.SocketError, "接收数据出错 : " + e.ToString());
            }
        }
        void RecvAsyncCompleted(object sender, SocketAsyncEventArgs e) {
            if (e.SocketError != SocketError.Success) {
                Disconnect(e.SocketError, "接收数据出错 : " + e.SocketError);
            } else if (e.BytesTransferred < 1) {
                Disconnect(SocketError.SocketError, "接收数据为0,断开链接");
            } else {
                while (m_RecvTotalSize + e.BytesTransferred >= m_RecvTotalBuffer.Length) {
                    var bytes = new byte[m_RecvTotalBuffer.Length * 2];
                    Array.Copy(m_RecvTotalBuffer, 0, bytes, 0, m_RecvTotalSize);
                    m_RecvTotalBuffer = bytes;
                }
                Array.Copy(e.Buffer, 0, m_RecvTotalBuffer, m_RecvTotalSize, e.BytesTransferred);
                m_RecvTotalSize += e.BytesTransferred;
                try {
                    ParsePackage();
                } catch (System.Exception ex) {
                    Disconnect(SocketError.SocketError, "解析数据出错 : " + ex.ToString());
                    return;
                }
                Receive();
            }
        }
        void ParsePackage() {
            for (; ; ) {
                if (m_RecvTotalSize < HeadLength) break;             //接收大小还不够协议头
                var length = BitConverter.ToInt32(m_RecvTotalBuffer, 0);
                var size = HeadLength + length;
                if (m_RecvTotalSize < size) break;          //协议还未接收完全
                var buffer = new byte[length];
                Array.Copy(m_RecvTotalBuffer, HeadLength, buffer, 0, length);
                EnqueueOpMessage(new OpRecv() {data = buffer});
                m_RecvTotalSize -= size;
                if (m_RecvTotalSize > 0) Array.Copy(m_RecvTotalBuffer, size, m_RecvTotalBuffer, 0, m_RecvTotalSize);
            }
        }
    }
}
