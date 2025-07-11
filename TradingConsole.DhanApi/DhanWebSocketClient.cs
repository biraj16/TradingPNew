using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TradingConsole.DhanApi.Models;
using TradingConsole.DhanApi.Models.WebSocket;

namespace TradingConsole.DhanApi
{
    public class DhanWebSocketClient : IDisposable
    {
        private ClientWebSocket? _webSocket;
        private readonly string _clientId;
        private readonly string _accessToken;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly JsonSerializerOptions _jsonOptions;

        private readonly SemaphoreSlim _subscriptionSemaphore = new SemaphoreSlim(1, 1);
        private const int MAX_INSTRUMENTS_PER_REQUEST = 99;

        #region Public Events
        public event Action<TickerPacket>? OnLtpUpdate;
        public event Action<PreviousClosePacket>? OnPreviousCloseUpdate;
        public event Action<QuotePacket>? OnQuoteUpdate;
        public event Action<OiPacket>? OnOiUpdate;
        public event Action<OrderBookEntry>? OnOrderUpdate;
        public event Action? OnConnected;
        #endregion

        #region WebSocket Message Models
        private class WebSocketSubscriptionInstrument
        {
            [JsonPropertyName("exchangeSegment")]
            public string ExchangeSegment { get; set; } = string.Empty;

            [JsonPropertyName("securityId")]
            public string SecurityId { get; set; } = string.Empty;
        }

        private class WebSocketSubscriptionRequest
        {
            [JsonPropertyName("requestCode")]
            public int RequestCode { get; set; }

            [JsonPropertyName("instrumentCount")]
            public int InstrumentCount { get; set; }

            [JsonPropertyName("instrumentList")]
            public List<WebSocketSubscriptionInstrument> InstrumentList { get; set; } = new List<WebSocketSubscriptionInstrument>();
        }
        #endregion

        public DhanWebSocketClient(string clientId, string accessToken)
        {
            _clientId = clientId;
            _accessToken = accessToken;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task ConnectAsync()
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open) return;
            _webSocket?.Dispose();

            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var uri = new Uri($"wss://api-feed.dhan.co?version=2&token={_accessToken}&clientId={_clientId}&authType=2");
                Debug.WriteLine($"[WebSocket] Connecting to: {uri}");
                await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token);
                Debug.WriteLine("[WebSocket] Connected Successfully.");
                OnConnected?.Invoke();
                _ = Task.Run(StartReceivingAsync, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebSocket] Connection error: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                _cancellationTokenSource?.Cancel();
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client initiated disconnect.", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WebSocket] Error closing websocket: {ex.Message}");
                }
            }
            _webSocket?.Dispose();
            _webSocket = null;
        }

        public async Task SubscribeToOrderFeedAsync()
        {
            if (_webSocket?.State != WebSocketState.Open || _cancellationTokenSource == null) return;

            var orderSubscriptionRequest = new { customers = new[] { _clientId } };
            string jsonRequest = JsonSerializer.Serialize(orderSubscriptionRequest, _jsonOptions);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonRequest));

            try
            {
                await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                Debug.WriteLine("[WebSocket] Successfully subscribed to order feed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebSocket] Error subscribing to order feed: {ex.Message}");
            }
        }

        public async Task SubscribeToInstrumentsAsync(Dictionary<string, int> instruments, int feedType = 15)
        {
            if (_webSocket?.State != WebSocketState.Open || !instruments.Any() || _cancellationTokenSource == null)
            {
                Debug.WriteLine("[WebSocket] Cannot subscribe - invalid state or no instruments");
                return;
            }

            await _subscriptionSemaphore.WaitAsync(_cancellationTokenSource.Token);
            try
            {
                if (instruments.Count > MAX_INSTRUMENTS_PER_REQUEST)
                {
                    Debug.WriteLine($"[WebSocket] INFO: Subscribing to {instruments.Count} instruments, which exceeds the limit of {MAX_INSTRUMENTS_PER_REQUEST}. Auto-chunking...");
                    var chunks = ChunkInstruments(instruments, MAX_INSTRUMENTS_PER_REQUEST);
                    foreach (var chunk in chunks)
                    {
                        await SubscribeToSingleChunkAsync(chunk, feedType);
                        await Task.Delay(300, _cancellationTokenSource.Token);
                    }
                    return;
                }

                await SubscribeToSingleChunkAsync(instruments, feedType);
            }
            finally
            {
                _subscriptionSemaphore.Release();
            }
        }

        private async Task SubscribeToSingleChunkAsync(Dictionary<string, int> instruments, int feedType)
        {
            if (_webSocket?.State != WebSocketState.Open || _cancellationTokenSource == null) return;

            var instrumentList = instruments.Select(kvp => new WebSocketSubscriptionInstrument
            {
                SecurityId = kvp.Key,
                ExchangeSegment = GetExchangeSegmentName(kvp.Value)
            }).ToList();

            var subscriptionRequest = new WebSocketSubscriptionRequest
            {
                RequestCode = feedType,
                InstrumentCount = instrumentList.Count,
                InstrumentList = instrumentList
            };

            string jsonRequest = JsonSerializer.Serialize(subscriptionRequest, _jsonOptions);
            Debug.WriteLine($"[WebSocket] Sending Subscription - FeedType: {feedType}, Count: {subscriptionRequest.InstrumentCount}");

            try
            {
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonRequest));
                await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                Debug.WriteLine($"[WebSocket] Successfully sent subscription for {subscriptionRequest.InstrumentCount} instruments");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebSocket] Error sending subscription: {ex.Message}");
                throw;
            }
        }

        private static IEnumerable<Dictionary<string, int>> ChunkInstruments(Dictionary<string, int> instruments, int chunkSize)
        {
            var instrumentList = instruments.ToList();
            for (int i = 0; i < instrumentList.Count; i += chunkSize)
            {
                yield return instrumentList
                    .Skip(i)
                    .Take(chunkSize)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        private string GetExchangeSegmentName(int segmentId)
        {
            return segmentId switch
            {
                0 => "IDX_I",
                1 => "NSE_EQ",
                2 => "NSE_FNO",
                8 => "BSE_FNO",
                _ => "UNKNOWN"
            };
        }

        private async Task StartReceivingAsync()
        {
            if (_webSocket == null || _cancellationTokenSource == null) return;

            var buffer = new ArraySegment<byte>(new byte[1024 * 8]);
            try
            {
                while (_webSocket.State == WebSocketState.Open && !_cancellationTokenSource.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        ParseBinaryMessage(new ArraySegment<byte>(buffer.Array, 0, result.Count));
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        ParseTextMessage(jsonString);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[WebSocket] Receiving task was cancelled gracefully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebSocket] Error receiving data: {ex.Message}");
            }
        }

        private void ParseTextMessage(string json)
        {
            try
            {
                if (json.Contains("Connected")) return;

                var orderUpdate = JsonSerializer.Deserialize<OrderBookEntry>(json, _jsonOptions);
                if (orderUpdate != null && !string.IsNullOrEmpty(orderUpdate.OrderId))
                {
                    Debug.WriteLine($"[PARSER] >>> SUCCESS: Parsed Order Update for OrderId {orderUpdate.OrderId}. Status: {orderUpdate.OrderStatus}");
                    OnOrderUpdate?.Invoke(orderUpdate);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PARSER] CRITICAL ERROR parsing text message: {ex.Message}. JSON: {json}");
            }
        }

        private void ParseBinaryMessage(ArraySegment<byte> data)
        {
            if (data.Array == null) return;

            try
            {
                using var stream = new MemoryStream(data.Array, data.Offset, data.Count);
                using var reader = new BinaryReader(stream);

                while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
                {
                    byte feedCode = reader.ReadByte();
                    ushort messageLength = reader.ReadUInt16();
                    byte exchangeSegment = reader.ReadByte();
                    int securityId = reader.ReadInt32();

                    long messageEndPosition = reader.BaseStream.Position - 8 + messageLength;
                    if (messageEndPosition > reader.BaseStream.Length)
                    {
                        Debug.WriteLine($"[PARSER] Incomplete packet received. Stated length {messageLength} exceeds buffer size.");
                        break;
                    }

                    switch (feedCode)
                    {
                        case 2: // Ticker Packet (LTP only)
                            if (reader.BaseStream.Position + 8 <= messageEndPosition)
                            {
                                float ltp = reader.ReadSingle();
                                int lastTradeTime = reader.ReadInt32();
                                OnLtpUpdate?.Invoke(new TickerPacket { SecurityId = securityId.ToString(), LastPrice = (decimal)ltp, LastTradeTime = lastTradeTime });
                            }
                            break;

                        case 4: // Quote Packet (Full market data)
                            if (reader.BaseStream.Position + 42 <= messageEndPosition)
                            {
                                var quote = new QuotePacket { SecurityId = securityId.ToString(), LastPrice = (decimal)reader.ReadSingle(), LastTradeQuantity = reader.ReadInt16(), LastTradeTime = reader.ReadInt32(), AvgTradePrice = (decimal)reader.ReadSingle(), Volume = reader.ReadInt32(), TotalSellQuantity = reader.ReadInt32(), TotalBuyQuantity = reader.ReadInt32(), Open = (decimal)reader.ReadSingle(), Close = (decimal)reader.ReadSingle(), High = (decimal)reader.ReadSingle(), Low = (decimal)reader.ReadSingle() };
                                OnQuoteUpdate?.Invoke(quote);
                            }
                            break;

                        case 5: // OI Packet (Open Interest)
                            if (reader.BaseStream.Position + 4 <= messageEndPosition)
                            {
                                int openInterest = reader.ReadInt32();
                                OnOiUpdate?.Invoke(new OiPacket { SecurityId = securityId.ToString(), OpenInterest = openInterest });
                            }
                            break;

                        case 6: // Previous Close Packet
                            if (reader.BaseStream.Position + 4 <= messageEndPosition)
                            {
                                float prevClose = reader.ReadSingle();
                                OnPreviousCloseUpdate?.Invoke(new PreviousClosePacket { SecurityId = securityId.ToString(), PreviousClose = (decimal)prevClose });
                            }
                            break;

                        default:
                            Debug.WriteLine($"[PARSER] Unknown Feed Code: {feedCode}, SecId: {securityId}. Skipping.");
                            break;
                    }

                    reader.BaseStream.Position = messageEndPosition;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PARSER] CRITICAL ERROR parsing binary message: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null) Debug.WriteLine($"[PARSER] Inner Exception: {ex.InnerException.Message}");
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _webSocket?.Dispose();
            _subscriptionSemaphore?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
