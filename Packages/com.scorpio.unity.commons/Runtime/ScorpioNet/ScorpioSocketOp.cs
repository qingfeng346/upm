using System.Collections.Generic;
namespace Scorpio.Net {
    public interface OpMessage {
        void Process(ScorpioSocket socket);
    }
    public abstract class OpMessage<T> : OpMessage where T : ScorpioSocket {
        public void Process(ScorpioSocket socket) { Process(socket as T); }
        public abstract void Process(T socket);
    }
    public partial class ScorpioSocket {
        private Queue<OpMessage> m_OpMessages = new Queue<OpMessage>();
        public bool AsyncCallback { get; set; } = true;     //异步回调，如果是非异步则需要调用ProcessOpMessage函数
        public void ProcessOpMessage() {
            try {
                while (m_OpMessages.Count > 0) {
                    OpMessage msg = null;
                    lock (m_OpMessages) { msg = m_OpMessages.Dequeue(); }
                    msg.Process(this);
                }
            } catch (System.Exception e) {
                LogError("ProcessOpMessage is error : " + e.ToString());
            }
        }
        protected void EnqueueOpMessage(OpMessage msg) {
            if (AsyncCallback) {
                msg.Process(this);
            } else {
                lock (m_OpMessages) {
                    m_OpMessages.Enqueue(msg);
                }
            }
        }
    }
}