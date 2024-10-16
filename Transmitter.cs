﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace QwertyIPC;

public static class Transmitter
{
    public static class Signal
    {
        public static Task Receiver(string transmitterName, Action action) =>
            Task.Run(() => ReceiverProcessing(transmitterName, action));
        public static Task Receiver(string transmitterName, Action<CancellationTokenSource> action, CancellationTokenSource cancelTokenSource) =>
            ReceiverProcessingAsync(transmitterName, action, cancelTokenSource);


        static void ReceiverProcessing(string transmitterName, Action action)
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream($"{Environment.UserDomainName}-{Environment.UserName}-{transmitterName}");
                    server.WaitForConnection();
                    using var reader = new StreamReader(server);
                }
                finally
                {
                    action?.Invoke();
                }
            }
        }
        static async Task ReceiverProcessingAsync(string transmitterName, Action<CancellationTokenSource> action, CancellationTokenSource cancelTokenSource)
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream($"{Environment.UserDomainName}-{Environment.UserName}-{transmitterName}");
                    await server.WaitForConnectionAsync(cancelTokenSource.Token);
                    using var reader = new StreamReader(server);
                    action?.Invoke(cancelTokenSource);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        public static void SendSignal(string pipeName)
        {
            using var client = new NamedPipeClientStream($"{Environment.UserDomainName}-{Environment.UserName}-{pipeName}");
            client.Connect();
            using var writer = new StreamWriter(client);
        }
    }

    public static class Message
    {
        public static Task Receiver(string transmitterName, Action<string> action) =>
            Task.Run(() => ReceiverProcessing(transmitterName, action));
        public static Task Receiver(string transmitterName, Action<string, CancellationTokenSource> action, CancellationTokenSource cancelTokenSource) =>
            ReceiverProcessingAsync(transmitterName, action, cancelTokenSource);

        static void ReceiverProcessing(string transmitterName, Action<string> action)
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream($"{Environment.UserDomainName}-{Environment.UserName}-{transmitterName}");
                    server.WaitForConnection();
                    using var reader = new StreamReader(server);
                    action?.Invoke(reader.ReadToEnd());
                }
                catch { }
            }
        }
        static async Task ReceiverProcessingAsync(string transmitterName, Action<string, CancellationTokenSource> action, CancellationTokenSource cancelTokenSource)
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream($"{Environment.UserDomainName}-{Environment.UserName}-{transmitterName}");
                    await server.WaitForConnectionAsync(cancelTokenSource.Token);
                    using var reader = new StreamReader(server);
                    action?.Invoke(reader.ReadToEnd(), cancelTokenSource);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        public static void SendMessage(string pipeName, string message)
        {
            using var client = new NamedPipeClientStream($"{Environment.UserDomainName}-{Environment.UserName}-{pipeName}");
            client.Connect();
            using var writer = new StreamWriter(client);
            writer.Write(message);
        }
    }
}