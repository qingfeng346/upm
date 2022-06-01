using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
namespace Scorpio.Net {
    using ActionSent = Action<SendData>;
    public class OpSent : OpMessage {
        public SendData sendData;
        public void Process (ScorpioSocket socket) {
            if (socket.OnSent != null) socket.OnSent (sendData);
        }
    }
    public class SendData {
        public byte[] data;
        public int type;
        public SendData (byte[] data, int type) {
            this.data = data;
            this.type = type;
        }
    }
    public partial class ScorpioSocket {
        protected object m_SendSync = new object ();                    // 发送消息线程锁
        protected SocketAsyncEventArgs m_SendEvent = null;              // 异步发送消息
        protected Queue<SendData> m_SendQueue = new Queue<SendData> (); // 发送消息队列
        private byte[] m_SendBuffer = new byte[MAX_BUFFER_LENGTH];      // 发送缓冲区
        public ActionSent OnSent;                                       // 发送消息回调
        private bool m_Sending = false;                                 // 正在发送消息
        private SendData m_SendingData = null;                          // 正在发送的协议
        void BeginSend () {
            m_Sending = false;
            m_SendingData = null;
            lock (m_SendSync) { m_SendQueue.Clear (); }
        }
        void SendInternal(byte[] data, int offset, int length) {
            try {
                if (data == null)
                    m_SendEvent.SetBuffer(offset, length);
                else
                    m_SendEvent.SetBuffer(data, offset, length);
                if (!m_Socket.SendAsync(m_SendEvent)) {
                    SendAsyncCompleted(this, m_SendEvent);
                }
            } catch (System.Exception ex) {
                Disconnect(SocketError.SocketError, "发送数据出错 : " + ex.Message);
            }
        }
        void SendAsyncCompleted(object sender, SocketAsyncEventArgs e) {
            if (e.SocketError != SocketError.Success) {
                Disconnect(e.SocketError, "发送数据出错 : " + e.SocketError);
                return;
            }
            lock (m_SendSync) {
                if (e.Offset + e.BytesTransferred < e.Count) {
                    SendInternal(null, e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred - e.Offset);
                } else {
                    EnqueueOpMessage(new OpSent() { sendData = m_SendingData });
                    m_Sending = false;
                    m_SendingData = null;
                    CheckSend();
                }
            }
        }
        void CheckSend() {
            lock (m_SendSync) {
                if (m_Sending || m_SendQueue.Count <= 0) return;
                m_Sending = true;
                lock (m_SendQueue) { m_SendingData = m_SendQueue.Dequeue(); }
                var data = m_SendingData.data;
                byte[] headBytes = null;
                switch (HeadLength) {
                    case 2:
                        headBytes = BitConverter.GetBytes(Convert.ToUInt16(data.Length));
                        break;
                    case 8:
                        headBytes = BitConverter.GetBytes(Convert.ToInt64(data.Length));
                        break;
                    default:
                        headBytes = BitConverter.GetBytes(Convert.ToInt32(data.Length));
                        break;
                }
                var totalLength = data.Length + headBytes.Length;
                while (totalLength >= m_SendBuffer.Length) {
                    m_SendBuffer = new byte[m_SendBuffer.Length * 2];
                }
                Array.Copy(headBytes, 0, m_SendBuffer, 0, headBytes.Length);
                Array.Copy(data, 0, m_SendBuffer, headBytes.Length, data.Length);
                SendInternal(m_SendBuffer, 0, totalLength);
            }
        }
        // void SendThread (object state) {
        //     for (; ; ) {
        //         Thread.Sleep(10);
        //         if (IsClosed) { break; }
        //         if (m_SendQueue.Count > 0) {
        //             try {
        //                 SendData sendData = null;
        //                 lock (m_SendSync) { sendData = m_SendQueue.Dequeue(); }
        //                 var data = sendData.data;
        //                 switch (HeadLength) {
        //                     case 2:
        //                         m_Socket.Send(BitConverter.GetBytes(Convert.ToUInt16(data.Length)));
        //                         break;
        //                     case 4:
        //                         m_Socket.Send(BitConverter.GetBytes(data.Length));
        //                         break;
        //                     case 8:
        //                         m_Socket.Send(BitConverter.GetBytes(Convert.ToInt64(data.Length)));
        //                         break;
        //                 }
        //                 m_Socket.Send(data);
        //                 EnqueueOpMessage(new OpSent() { sendData = sendData });
        //             } catch (System.Exception e) {
        //                 Disconnect(SocketError.SocketError, "Send is error : " + e.ToString());
        //             }
        //         }
        //     }
        // }
        //发送协议
        public void Send (byte[] data, int type) {
            if (m_State == State.None || m_State == State.Connecting) { return; }
            lock (m_SendSync) {
                m_SendQueue.Enqueue (new SendData (data, type));
            }
            System.Threading.ThreadPool.QueueUserWorkItem((state) => {
                CheckSend();
            });
        }
    }
}