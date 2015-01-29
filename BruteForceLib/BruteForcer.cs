using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Diagnostics;

namespace BruteForceLib
{
    public class BruteForcer
    {
        public event FinishedEventHandler OnFinish;
        public delegate void FinishedEventHandler(FinishedEventArgs e);

        public class FinishedEventArgs : EventArgs
        {
            public long NumberOfAttempts;
            public TimeSpan ElapsedTime;
        }

        public event PasswordFoundEventHandler OnPasswordFound;
        public delegate void PasswordFoundEventHandler(PasswordFoundEventArgs e);

        public class PasswordFoundEventArgs : EventArgs
        {
            public string Password;
        }

        public event ProgressChangedEventHandler OnProgressChanged;
        public delegate void ProgressChangedEventHandler(ProgressChangedEventArgs e);

        public class ProgressChangedEventArgs : EventArgs
        {
            public double ProgressPercentage;
        }

        private long attempts = 0L;
        private int progress;
        private Stopwatch threadTimer = new Stopwatch();
        public TimeSpan EstimatedCompletionTime { get { return new TimeSpan(0, 0, (threadTimer.IsRunning && progress > 0) ? (int)((threadTimer.Elapsed.TotalSeconds / (100 * attempts / MaxAttempts) * 100) - threadTimer.Elapsed.TotalSeconds) : 0); } }
        private string chars = "abcdefghijklmnopqrstuvwxyz";
        private EncryptionType encryptionType;
        private bool isRunning;
        public bool IsRunning { get { return isRunning; } }
        private long MaxAttempts;
        private int[] array;
        private int minSize;
        private int maxSize;
        private string passToCrack;

        private long CalcMaxAttempts()
        {
            if (maxSize == minSize)
                return (long)Math.Pow(chars.Length, maxSize);
            long j = 0;
            for (int i = maxSize; i > minSize; i--)
                j += (long)Math.Pow(chars.Length, i);
            return j;
        }

        public BruteForcer(int MinSize, int MaxSize)
        {
            minSize = MinSize;
            maxSize = MaxSize;
            array = new int[MaxSize];
            for (int i = 0; i < MaxSize; i++)
            {
                if (i < MaxSize - MinSize)
                    array[i] = -1;
            }
        }

        public void SetChars(string Chars)
        {
            chars = Chars;
        }

        public void SetChars(params char[] Chars)
        {
            chars = string.Join("", Chars);
        }

        public void Start(string PassToCrack)
        {
            if (isRunning)
                throw new Exception("Bruteforcer is already running!");

            if (minSize > maxSize)
                throw new Exception("Minimum size is greater than Maximum size, BruteForcer cancelled.");

            passToCrack = PassToCrack;
            MaxAttempts = CalcMaxAttempts();
            isRunning = true;
            ThreadPool.QueueUserWorkItem(new WaitCallback(BruteForcerLoop));
        }

        public void Start(string PassToCrack, EncryptionType EncryptionType)
        {
            encryptionType = EncryptionType;
            Start(PassToCrack);
        }

        private string GetEncryption(string s)
        {
            switch (encryptionType)
            {
                case EncryptionType.Sha512:
                    return BitConverter.ToString(((System.Security.Cryptography.SHA512)new System.Security.Cryptography.SHA512Managed()).ComputeHash(System.Text.Encoding.ASCII.GetBytes(s))).Replace("-", "");
            }
            return s;
        }

        public void Stop()
        {
            isRunning = false;
        }

        private void BruteForcerLoop(object threadContext)
        {
            progress = 0;
            attempts = 0L;
            threadTimer = Stopwatch.StartNew();
            while (isRunning)
            {
                int percentage = (int)(100 * attempts / MaxAttempts);
                if (percentage > progress)
                {
                    progress = percentage;
                    if (OnProgressChanged != null)
                        OnProgressChanged(new ProgressChangedEventArgs() { ProgressPercentage = progress });
                }
                attempts++;
                string pass = GetPass();

                if (encryptionType != EncryptionType.None)
                {
                    pass = GetEncryption(pass);
                }
                if (pass == passToCrack)
                {
                    if (OnPasswordFound != null)
                        OnPasswordFound.Invoke(new PasswordFoundEventArgs() { Password = GetPass() });
                    isRunning = false;
                }
                NextPass();
            }
            threadTimer.Stop();
            if (OnFinish != null)
            {
                OnFinish.Invoke(new FinishedEventArgs() { NumberOfAttempts = attempts, ElapsedTime = threadTimer.Elapsed });
            }
        }

        private string GetPass()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] >= 0)
                    sb.Append(chars[array[i]]);
            }
            return sb.ToString();
        }

        private void NextPass()
        {
            for (int pos = array.Length - 1; pos >= 0; pos--)
            {
                if (array[pos] < chars.Length - 1)
                {
                    array[pos]++;
                    for (int i = pos + 1; i < array.Length; i++)
                        array[i] = 0;
                    break;
                }
                if (pos == 0 && array[0] == chars.Length - 1)
                    isRunning = false;
            }
        }
    }

    public enum EncryptionType
    {
        None,
        Sha512
    }
}
