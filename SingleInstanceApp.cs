using System;
using System.Threading;

namespace QwertyIPC;

public static class SingleInstanceApp
{
    static Mutex Mutex;

    public class BySignal
    {
        public static bool IsFirstInstance(string appName, Action<CancellationTokenSource> onSignalReceived, out CancellationTokenSource cancelTokenSource)
        {
            cancelTokenSource = null!;
            Mutex = new Mutex(true, $"{Environment.UserDomainName}-{Environment.UserName}-{appName}", out var prevInstance);
            if (prevInstance)
            {
                cancelTokenSource = new CancellationTokenSource();
                Transmitter.Signal.Receiver(appName, onSignalReceived, cancelTokenSource);
            }
            else
            {
                Transmitter.Signal.SendSignal(appName);
            }
            return prevInstance;
        }
        public static bool IsFirstInstance(string appName, Action onSignalReceived)
        {
            Mutex = new Mutex(true, $"{Environment.UserDomainName}-{Environment.UserName}-{appName}", out var prevInstance);
            if (prevInstance)
            {
                Transmitter.Signal.Receiver(appName, onSignalReceived);
            }
            else
            {
                Transmitter.Signal.SendSignal(appName);
            }
            return prevInstance;
        }
    }

    public class ByMessage
    {
        public static bool IsFirstInstance(string appName, string messageToFirstInstance, Action<string, CancellationTokenSource> onMessageReceived, out CancellationTokenSource cancelTokenSource)
        {
            cancelTokenSource = null!;
            Mutex = new Mutex(true, $"{Environment.UserDomainName}-{Environment.UserName}-{appName}", out var prevInstance);
            if (prevInstance)
            {
                cancelTokenSource = new CancellationTokenSource();
                Transmitter.Message.Receiver(appName, onMessageReceived, cancelTokenSource);
            }
            else
            {
                Transmitter.Message.SendMessage(appName, messageToFirstInstance);
            }
            return prevInstance;
        }
        public static bool IsFirstInstance(string appName, string message, Action<string> onMessageReceived)
        {
            Mutex = new Mutex(true, $"{Environment.UserDomainName}-{Environment.UserName}-{appName}", out var prevInstance);
            if (prevInstance)
            {
                Transmitter.Message.Receiver(appName, onMessageReceived);
            }
            else
            {
                Transmitter.Message.SendMessage(appName, message);
            }
            return prevInstance;
        }
    }
}