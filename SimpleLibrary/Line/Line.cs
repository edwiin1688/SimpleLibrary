using Autofac;
using Newtonsoft.Json;
using SimpleLibrary.Logger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleLibrary.Line
{
    public class Line : PrintLogger
    {
        private string _Url = "";
        private string _UserId = "";
        private string _ApiKey = "";
        private readonly HttpClient _httpClient;

        public Line(string url, string userId, string apiKey, ContainerBuilder builder = null)
        {
            _Url = url;
            _UserId = userId;
            _ApiKey = apiKey;
            _httpClient = new HttpClient();
            InitLogger(builder);
        }

        public void Notify(string message)
        {
            var msg = new Dictionary<string, object>
            {
                { "to", _UserId },
                { "messages", new object[] {
                    new Dictionary<string, string> {
                        { "type", "text" },
                        { "text", message }
                    }
                }}
            };
            SendRequest(msg);
        }

        public void NotifyImage(string message, string imageUrl)
        {
            var msg = new Dictionary<string, object>
            {
                { "to", _UserId },
                { "messages", new object[] {
                    new Dictionary<string, string> {
                        { "type", "text" },
                        { "text", message }
                    },
                    new Dictionary<string, string> {
                        { "type", "image" },
                        { "originalContentUrl", imageUrl },
                        { "previewImageUrl", imageUrl }
                    }
                }}
            };
            SendRequest(msg);
        }

        public void NotifySticker(string message, string packageId, string stickerId)
        {
            var msg = new Dictionary<string, object>
            {
                { "to", _UserId },
                { "messages", new object[] {
                    new Dictionary<string, string> {
                        { "type", "text" },
                        { "text", message }
                    },
                    new Dictionary<string, string> {
                        { "type", "sticker" },
                        { "packageId", packageId },
                        { "stickerId", stickerId }
                    }
                }}
            };
            SendRequest(msg);
        }

        public void NotifyWithActions(string message, List<LineAction> actions)
        {
            var quickReplyItems = new List<object>();
            foreach (var a in actions)
            {
                var actionObj = new Dictionary<string, string>
                {
                    { "type", a.Type },
                    { "label", a.Label }
                };
                if (!string.IsNullOrEmpty(a.Text))
                    actionObj["text"] = a.Text;
                if (!string.IsNullOrEmpty(a.Uri))
                    actionObj["uri"] = a.Uri;

                quickReplyItems.Add(new Dictionary<string, object>
                {
                    { "type", "action" },
                    { "action", actionObj }
                });
            }

            var msg = new Dictionary<string, object>
            {
                { "to", _UserId },
                { "messages", new object[] {
                    new Dictionary<string, object> {
                        { "type", "text" },
                        { "text", message },
                        { "quickReply", new Dictionary<string, object> {
                            { "items", quickReplyItems.ToArray() }
                        }}
                    }
                }}
            };
            SendRequest(msg);
        }

        private void SendRequest(object msg)
        {
            try
            {
                string msgStr = JsonConvert.SerializeObject(msg);
                var content = new StringContent(msgStr, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, _Url)
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {_ApiKey}");

                var response = _httpClient.Send(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Print($"Error sending LINE message: {e.Message}", Color.Red);
            }
        }

        public async Task NotifyAsync(string message, CancellationToken cancellationToken = default)
        {
            var msg = new Dictionary<string, object>
            {
                { "to", _UserId },
                { "messages", new object[] {
                    new Dictionary<string, string> {
                        { "type", "text" },
                        { "text", message }
                    }
                }}
            };
            await SendRequestAsync(msg, cancellationToken);
        }

        public async Task NotifyImageAsync(string message, string imageUrl, CancellationToken cancellationToken = default)
        {
            var msg = new Dictionary<string, object>
            {
                { "to", _UserId },
                { "messages", new object[] {
                    new Dictionary<string, string> {
                        { "type", "text" },
                        { "text", message }
                    },
                    new Dictionary<string, string> {
                        { "type", "image" },
                        { "originalContentUrl", imageUrl },
                        { "previewImageUrl", imageUrl }
                    }
                }}
            };
            await SendRequestAsync(msg, cancellationToken);
        }

        public async Task NotifyStickerAsync(string message, string packageId, string stickerId, CancellationToken cancellationToken = default)
        {
            var msg = new Dictionary<string, object>
            {
                { "to", _UserId },
                { "messages", new object[] {
                    new Dictionary<string, string> {
                        { "type", "text" },
                        { "text", message }
                    },
                    new Dictionary<string, string> {
                        { "type", "sticker" },
                        { "packageId", packageId },
                        { "stickerId", stickerId }
                    }
                }}
            };
            await SendRequestAsync(msg, cancellationToken);
        }

        private async Task SendRequestAsync(object msg, CancellationToken cancellationToken)
        {
            try
            {
                string msgStr = JsonConvert.SerializeObject(msg);
                var content = new StringContent(msgStr, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, _Url)
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {_ApiKey}");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Print($"Error sending LINE message: {e.Message}", Color.Red);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class LineAction
    {
        public string Type { get; set; } = "message";
        public string Label { get; set; }
        public string Text { get; set; }
        public string Uri { get; set; }
    }
}
