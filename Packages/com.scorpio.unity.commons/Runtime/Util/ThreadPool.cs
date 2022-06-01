using System;
public delegate void ThreadHandler();
public class ThreadPool {
    public static ThreadPool CreateThread(ThreadHandler handler) {
        return new ThreadPool(handler);
    }
    public static ThreadPool CreatePeriodicThread(ThreadHandler handler, int periodic) {
        return new ThreadPool(handler, periodic);
    }
    private ThreadHandler mHandler = null;
    private int mPeriodic = 0;
    private System.Threading.Thread mThread = null;
    private ThreadPool(ThreadHandler handler) {
        mHandler = handler;
        mThread = new System.Threading.Thread(Run);
        mThread.Start();
    }
    private ThreadPool(ThreadHandler handler, int periodic) {
        mHandler = handler;
        mPeriodic = Math.Min(periodic, 1);
        mThread = new System.Threading.Thread(PeriodicRun);
        mThread.Start();
    }
    public void Destroy() {
        if (mThread != null && mThread.IsAlive) {
            mThread.Abort();
        }
        mThread = null;
    }
    void Run() {
        if (mHandler != null) mHandler();
    }
    void PeriodicRun() {
        for (;;) {
            System.Threading.Thread.Sleep(mPeriodic);
            if (mHandler != null) mHandler();
        }
    }
}