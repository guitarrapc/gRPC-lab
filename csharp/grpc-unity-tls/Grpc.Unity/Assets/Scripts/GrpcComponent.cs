using Grpc.Core;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GrpcUnitySample
{
    public class GrpcComponent : MonoBehaviour
    {
        [Header("Endpoint")]
        [SerializeField]
        private Text _endpointLabelText = null;
        [SerializeField]
        private InputField _endpointInputField = null;

        [Header("Output")]
        [SerializeField]
        private Text _outputText = null;

        [Header("Execute")]
        [SerializeField]
        private Button _duplexRun = null;
        [SerializeField]
        private Button _duplexCancel = null;

        private CancellationTokenSource _cts = null;
        private Channel _channel = null;
        private List<AsyncDuplexStreamingCall<HelloRequest, HelloReply>> _streamingClients = new List<AsyncDuplexStreamingCall<HelloRequest, HelloReply>>();
        private bool _isDuplexRunning = false;
        private object _lock = new object();

        void Start()
        {
            if (_endpointLabelText == null)
                throw new ArgumentException($"{nameof(_endpointLabelText)} component is not attached to {nameof(GrpcComponent)}");
            if (_endpointInputField == null)
                throw new ArgumentException($"{nameof(_endpointInputField)} component is not attached to {nameof(GrpcComponent)}");
            if (_outputText == null)
                throw new ArgumentException($"{nameof(_outputText)} component is not attached to {nameof(GrpcComponent)}");
            if (_duplexCancel == null)
                throw new ArgumentException($"{nameof(_duplexCancel)} component is not attached to {nameof(GrpcComponent)}");
            if (_duplexRun == null)
                throw new ArgumentException($"{nameof(_duplexRun)} component is not attached to {nameof(GrpcComponent)}");

            _duplexRun.interactable = true;
            _duplexCancel.interactable = false;

            _endpointInputField.onEndEdit.AddListener(text => _endpointLabelText.text = "Endpoint: " + _endpointInputField.text);
            _outputText.text = "";
        }

        async void OnDestroy()
        {
            _cts?.Dispose();
            if (_streamingClients != null)
            {
                foreach (var streamingClient in _streamingClients)
                    streamingClient.Dispose();
            }
            await _channel?.ShutdownAsync();
        }

        // Unary
        public async void UnaryRun()
        {
            var channel = await CreateChannelAsync();
            Debug.Log($"Begin unary {channel.ResolvedTarget}");
            await Task.WhenAll(UnaryRunAsync("r1", channel), UnaryRunAsync("r2", channel));
        }
        private async Task UnaryRunAsync(string prefix, Channel channel)
        {
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = $"{prefix} GreeterClient" });
            WriteLine("Unary Greeting: " + reply.Message);
        }

        // Duplex
        public void DuplexCancel()
        {
            if (_cts == null)
            {
                Debug.Log("Duplex not started");
                return;
            }

            Debug.Log("Duplex cancel");
            _cts?.Cancel();
            _cts?.Dispose();
            if (!_duplexRun.IsDestroyed())
                _duplexRun.interactable = true;
            if (!_duplexCancel.IsDestroyed())
                _duplexCancel.interactable = false;
        }

        public async void DuplexRun()
        {
            _endpointLabelText.text = "Endpoint: " + _endpointInputField.text;
            if (_isDuplexRunning)
            {
                Debug.Log($"Duplex is already running.");
                return;
            }
            _isDuplexRunning = true;
            _duplexRun.interactable = false;
            _duplexCancel.interactable = true;
            _cts = new CancellationTokenSource();

            try
            {
                var channel = await CreateChannelAsync();
                Debug.Log($"Begin duplex {channel.ResolvedTarget}");
                await Task.WhenAll(DuplexRunAsync("r1", channel, _cts.Token), DuplexRunAsync("r2", channel, _cts.Token));
            }
            catch (OperationCanceledException _)
            {
                Debug.Log("Duplex is canceled.");
            }
            finally
            {
                if (!_duplexRun.IsDestroyed())
                    _duplexRun.interactable = true;
                if (!_duplexCancel.IsDestroyed())
                    _duplexCancel.interactable = false;
                _isDuplexRunning = false;
                _cts?.Dispose();
            }
        }
        private async Task DuplexRunAsync(string prefix, Channel channel, CancellationToken ct = default)
        {
            var context = SynchronizationContext.Current;

            var requestHeaders = new Metadata
            {
                { "x-host-port", "10-0-0-10" },
            };
            var client = new Greeter.GreeterClient(channel);
            var streamingClient = client.StreamingBothWays(requestHeaders);
            _streamingClients.Add(streamingClient);

            var readTask = Task.Run(async () =>
            {
                while (await streamingClient.ResponseStream.MoveNext())
                {
                    if (ct.IsCancellationRequested)
                        return;

                    var current = streamingClient.ResponseStream.Current;
                    context.Post((state) =>
                    {
                        if (!ct.IsCancellationRequested)
                        {
                            WriteLine(current.Message);
                        }
                    }, null);
                }
            }, ct);

            var i = 0;
            try
            {
                while (i++ < 100)
                {
                    Debug.Log($"Streaming Request {i}");
                    if (ct.IsCancellationRequested)
                        return;

                    await streamingClient.RequestStream.WriteAsync(new HelloRequest
                    {
                        Name = $"streaming: {prefix} {i}",
                    });
                    await Task.Delay(TimeSpan.FromMilliseconds(10), ct);
                }
            }
            finally
            {
                //await _streamingClient.RequestStream.CompleteAsync();
                await readTask;
            }
        }

        private void WriteLine(string message)
        {
            lock (_lock)
            {
                _outputText.text += $"\n{message}";
            }
        }

        private async Task<Channel> CreateChannelAsync()
        {
            if (_channel == null)
            {
                _channel = CreateChannelCore();
                return _channel;
            }
            
            if (_channel.ResolvedTarget == _endpointInputField.text)
            {
                return _channel;
            }
            else
            {
                await _channel.ShutdownAsync();
                _channel = CreateChannelCore();
                return _channel;
            }
        }

        private Channel CreateChannelCore()
        {
            var hostPort = _endpointInputField.text.Split(':');
            var host = hostPort[0];
            var port = 80;
            if (hostPort.Length == 2)
            {
                port = int.Parse(hostPort[1]);
            }
            var channelCredential = port == 443
                ? new SslCredentials()
                : ChannelCredentials.Insecure;
            var channel = new Channel(host, port, channelCredential);
            return channel;
        }
    }
}