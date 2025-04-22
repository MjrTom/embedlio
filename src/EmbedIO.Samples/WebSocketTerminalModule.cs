using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using EmbedIO.WebSockets;

namespace EmbedIO.Samples
{
    /// <summary>
    /// Define a command-line interface terminal.
    /// </summary>
    public class WebSocketTerminalModule(string urlPath) : WebSocketModule(urlPath, true)
    {
        private readonly ConcurrentDictionary<IWebSocketContext, Process> _processes = new();

        /// <inheritdoc />
        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] rxBuffer, IWebSocketReceiveResult rxResult)
            => _processes.TryGetValue(context, out Process? process)
                ? process.StandardInput.WriteLineAsync(Encoding.GetString(rxBuffer))
                : Task.CompletedTask;

        /// <inheritdoc />
        protected override Task OnClientConnectedAsync(IWebSocketContext context)
        {
#pragma warning disable CA2000 // Call Dispose on object - will do in OnClientDisconnectedAsync.
            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = "cmd.exe",
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = Environment.CurrentDirectory
                },
            };
#pragma warning restore CA2000

            process.OutputDataReceived += async (s, e) => await SendBufferAsync(s! as Process, e!.Data).ConfigureAwait(false);

            process.ErrorDataReceived += async (s, e) => await SendBufferAsync(s! as Process, e!.Data).ConfigureAwait(false);

            process.Exited += async (s, e) =>
            {
                IWebSocketContext ctx = FindContext(s! as Process);
                if (ctx?.WebSocket?.State == WebSocketState.Open)
                    await CloseAsync(ctx).ConfigureAwait(false);
            };

            _processes.TryAdd(context, process);

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
        {
            if (_processes.TryRemove(context, out Process? process))
            {
                if (!process.HasExited)
                    process.Kill();

                process.Dispose();
            }

            return Task.CompletedTask;
        }

        private IWebSocketContext FindContext(Process p)
            => _processes.FirstOrDefault(kvp => kvp.Value == p).Key;

        private async Task SendBufferAsync(Process process, string buffer)
        {
            try
            {
                if (process.HasExited)
                    await Task.CompletedTask;
                IWebSocketContext context = FindContext(process);
                await (context?.WebSocket?.State == WebSocketState.Open
                    ? SendAsync(context, buffer)
                    : Task.CompletedTask);
                return;
            }
            catch
            {
                // ignore process teermination
                await Task.CompletedTask;
                return;
            }
        }
    }
}