using Microsoft.AspNetCore.SignalR.Client;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using TatehamaTTC_v1bata.Model;

namespace TatehamaTTC_v1bata.Network
{

    public class Network : IAsyncDisposable
    {
        private readonly TimeSpan _renewMargin = TimeSpan.FromMinutes(1);
        private readonly OpenIddictClientService _service;
        private HubConnection? _connection;

        private static string _token = "";
        private string _refreshToken = "";
        private DateTimeOffset _tokenExpiration = DateTimeOffset.MinValue;
        private bool _eventHandlersSet = false;

        public static bool connected { get; set; } = false;

        // 再接続間隔（ミリ秒）
        private const int ReconnectIntervalMs = 1000;

        /// <summary>
        /// サーバーから来たデータ
        /// </summary>
        private DataFromServer DataFromServer;

        private bool previousStatus { get; set; }
        private bool connectErrorDialog { get; set; } = false;
        private bool previousDriveStatus { get; set; }

        public Network(OpenIddictClientService service)
        {
            _service = service;
            StartUpdateLoop();
            previousStatus = false;
            connectErrorDialog = false;
        }

        public void StartUpdateLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await UpdateLoop();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
        }

        /// <summary>
        /// 定常ループ
        /// </summary>
        /// <returns></returns>
        private async Task UpdateLoop()
        {
            while (true)
            {
                var timer = Task.Delay(100);
                await timer;
                try
                {
                    if (!connected)
                    {
                        continue;
                    }

                    await SendData_to_Server();
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// インタラクティブ認証を行い、SignalR接続を試みる
        /// </summary>
        /// <returns>ユーザーのアクションが必要かどうか</returns>
        public async Task<bool> Authorize()
        {
            // 認証を行う
            var isAuthenticated = await InteractiveAuthenticateAsync();
            if (!isAuthenticated)
            {
                return false;
            }

            await DisposeAndStopConnectionAsync(CancellationToken.None); // 古いクライアントを破棄
            InitializeConnection(); // 新しいクライアントを初期化
            // 接続を試みる
            var isActionNeeded = await Connect();
            if (isActionNeeded)
            {
                return true;
            }

            SetEventHandlers(); // イベントハンドラを設定
            return false;
        }

        /// <summary>
        /// interactive認証とエラーハンドリング
        /// </summary>
        /// <returns>認証に成功したかどうか</returns>
        private async Task<bool> InteractiveAuthenticateAsync()
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(90)).Token;
            return await InteractiveAuthenticateAsync(cancellationToken);
        }

        /// <summary>
        /// interactive認証とエラーハンドリング
        /// </summary>
        /// <returns>認証に成功したかどうか</returns>
        private async Task<bool> InteractiveAuthenticateAsync(CancellationToken cancellationToken)
        {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));
            try
            {
                // Ask OpenIddict to initiate the authentication flow (typically, by starting the system browser).
                var result = await _service.ChallengeInteractivelyAsync(new()
                {
                    CancellationToken = source.Token,
                    Scopes = [OpenIddictConstants.Scopes.OfflineAccess]
                });

                // Wait for the user to complete the authorization process.             
                var resultAuth = await _service.AuthenticateInteractivelyAsync(new()
                {
                    CancellationToken = cancellationToken,
                    Nonce = result.Nonce
                });
                _token = resultAuth.BackchannelAccessToken;
                _tokenExpiration = resultAuth.BackchannelAccessTokenExpirationDate ?? DateTimeOffset.MinValue;
                _refreshToken = resultAuth.RefreshToken;
                // 認証完了！
                return true;
            }
            catch (OperationCanceledException)
            {
                // その他別な理由で認証失敗      
                if (connectErrorDialog) return false;
                connectErrorDialog = true;
                DialogResult result = MessageBox.Show($"認証でタイムアウトしました。\n再認証してください。\n※いいえを選択した場合、再認証にはTTC再起動が必要です。",
                    "認証失敗 | 仮TTC - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await InteractiveAuthenticateAsync();
                    connectErrorDialog = false;
                    return r;
                }

                return false;
            }
            catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error ==
                                                                           OpenIddictConstants.Errors
                                                                               .UnauthorizedClient)
            {
                // ログインしたユーザーがサーバーにいないか、入鋏ロールがついてない
                MessageBox.Show($"認証が拒否されました。\n運転会サーバーに参加し入鋏を受けてください。", "認証拒否 | 仮TTC - ダイヤ運転会");
                return false;
            }
            catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error ==
                                                                           OpenIddictConstants.Errors.ServerError)
            {
                // サーバーでトラブル発生                       
                if (connectErrorDialog) return false;
                connectErrorDialog = true;
                DialogResult result =
                    MessageBox.Show(
                        $"認証に失敗しました。\n再認証しますか？\n※いいえを選択した場合、再認証にはTTC再起動が必要です。\n\n{exception.Message}\n{exception.StackTrace}",
                        "認証失敗 | 仮TTC - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await InteractiveAuthenticateAsync();
                    connectErrorDialog = false;
                    return r;
                }

                return false;
            }
            catch (Exception exception)
            {
                // その他別な理由で認証失敗      
                if (connectErrorDialog) return false;
                connectErrorDialog = true;
                DialogResult result =
                    MessageBox.Show(
                        $"認証に失敗しました。\n再認証しますか？\n※いいえを選択した場合、再認証にはTTC再起動が必要です。\n\n{exception.Message}\n{exception.StackTrace}",
                        "認証失敗 | 仮TTC - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await InteractiveAuthenticateAsync();
                    connectErrorDialog = false;
                    return r;
                }

                return false;
            }
        }


        private async Task TryReconnectAsync()
        {
            while (true)
            {
                try
                {
                    var isActionNeeded = await TryReconnectOnceAsync();
                    if (isActionNeeded)
                    {
                        Debug.WriteLine("Action needed after reconnection.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Reconnect failed: {ex.Message}");
                }

                if (_connection != null && _connection.State == HubConnectionState.Connected)
                {
                    Debug.WriteLine("Reconnected successfully.");
                    break;
                }

                await Task.Delay(ReconnectIntervalMs);
            }
        }

        /// <summary>
        /// 再接続を試みます。
        /// </summary>
        /// <returns>ユーザーによるアクションが必要かどうか(=すなわち、再接続ループを打ち切るべきかどうか)</returns>
        private async Task<bool> TryReconnectOnceAsync()
        {
            bool isActionNeeded;
            // トークンが切れていない場合 かつ 切れるまで余裕がある場合はそのまま再接続
            if (_tokenExpiration > DateTimeOffset.UtcNow + _renewMargin)
            {
                Debug.WriteLine("Try reconnect with current token...");
                isActionNeeded = await Connect();
                Debug.WriteLine("Reconnected with current token.");
                if (isActionNeeded)
                {
                    return true; // アクションが必要な場合はtrueを返す
                }
                SetEventHandlers(); // イベントハンドラを設定
                return isActionNeeded;
            }

            // トークンが切れていてリフレッシュトークンが有効な場合はリフレッシュ
            try
            {
                Debug.WriteLine("Try refresh token...");
                await RefreshTokenWithHandlingAsync(CancellationToken.None);
                await DisposeAndStopConnectionAsync(CancellationToken.None); // 古いクライアントを破棄
                InitializeConnection(); // 新しいクライアントを初期化
                isActionNeeded = await Connect(); // 新しいクライアントを開始
                if (isActionNeeded)
                {
                    return true; // アクションが必要な場合はtrueを返す
                }

                SetEventHandlers(); // イベントハンドラを設定
                Debug.WriteLine("Reconnected with refreshed token.");
                return false; // アクションが必要ない場合はfalseを返す    
            }
            catch (OpenIddictExceptions.ProtocolException ex)
                when (ex.Error is
                          OpenIddictConstants.Errors.InvalidToken
                          or OpenIddictConstants.Errors.InvalidGrant
                          or OpenIddictConstants.Errors.ExpiredToken)
            {
                // ignore: リフレッシュトークンが無効な場合
            }
            catch (InvalidOperationException)
            {
                // ignore: リフレッシュトークンが設定されていない場合
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during token refresh: {ex.Message}");
                throw;
            }

            // リフレッシュトークンが無効な場合
            Debug.WriteLine("Refresh token is invalid or expired.");
            if (connectErrorDialog) return false;
            connectErrorDialog = true;
            DialogResult dialogResult = MessageBox.Show(
                "トークンが切れました。\n再認証してください。\n※いいえを選択した場合、再認証にはTTC再起動が必要です。",
                "認証失敗 | 仮TTC - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (dialogResult == DialogResult.Yes)
            {
                var r = await Authorize();
                connectErrorDialog = false;
                return r;
            }
            Debug.WriteLine("Reconnected after re-authentication.");
            return true;
        }


        /// <summary>
        /// リフレッシュトークンを使用してトークンを更新します。
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task RefreshTokenWithHandlingAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                throw new InvalidOperationException("Refresh token is not set.");
            }

            var result = await _service.AuthenticateWithRefreshTokenAsync(new()
            {
                CancellationToken = cancellationToken,
                RefreshToken = _refreshToken
            });

            _token = result.AccessToken;
            _tokenExpiration = result.AccessTokenExpirationDate ?? DateTimeOffset.MinValue;
            _refreshToken = result.RefreshToken;
            Debug.WriteLine($"Token refreshed successfully");
        }


        // _connectionの破棄と停止
        private async Task DisposeAndStopConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection == null)
            {
                return;
            }

            await _connection.StopAsync(cancellationToken);
            await _connection.DisposeAsync();
            _connection = null;
        }


        // _connectionの初期化
        private void InitializeConnection()
        {
            if (_connection != null)
            {
                throw new InvalidOperationException("_connection is already initialized.");
            }

            _connection = new HubConnectionBuilder()
                .WithUrl($"{ServerAddress.SignalAddress}/hub/train?access_token={_token}")
                .Build();
            _eventHandlersSet = false;
        }

        // SignalR接続のイベントハンドラ設定
        private void SetEventHandlers()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("_connection is not initialized.");
            }
            if (_eventHandlersSet)
            {
                return; // イベントハンドラは一度だけ設定する
            }

            _connection.On<DataFromServer>("ReceiveData", OnReceiveDataFromServer);

            _connection.Closed += async (error) =>
            {
                Debug.WriteLine($"SignalR disconnected");
                connected = false;
                if (error == null)
                {
                    return;
                }

                Debug.WriteLine($"Error: {error.Message}");
                // 接続が切れた場合、再接続を試みる
                await TryReconnectAsync();
            };
            _eventHandlersSet = true;
        }


        /// <summary>
        /// 接続処理
        /// </summary>
        /// <returns>ユーザーのアクションが必要かどうか</returns>
        private async Task<bool> Connect()
        {
            var result = false;
            while (!connected)
            {
                try
                {
                    await _connection.StartAsync();
                    Debug.WriteLine("Connected");
                    connected = true;
                }
                // 該当Hubにアクセスするためのロールが無いときのエラー 
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    Debug.WriteLine("Forbidden");
                    connected = false;
                    if (connectErrorDialog)
                    {
                        return true;
                    }

                    connectErrorDialog = true;
                    DialogResult dialogResult =
                        MessageBox.Show($"ロール不足です。\nアカウントを確認して再認証してください。\n再認証しますか？\n※いいえを選択した場合、再認証にはTTC再起動が必要です。",
                            "認証失敗 | 館浜TTC - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (dialogResult == DialogResult.Yes)
                    {
                        result = await Authorize();
                    }

                    connectErrorDialog = false;
                }
                // Disposeされた接続を使用しようとした場合のエラー
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine("Maybe using disposed connection");
                    connected = false;
                    // 一旦接続を破棄して再初期化
                    await DisposeAndStopConnectionAsync(CancellationToken.None);
                    InitializeConnection();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_connection Error!!");
                    connected = false;
                }
            }

            return result;
        }

        private void OnReceiveDataFromServer(DataFromServer data)
        {
            if (data == null)
            {
                Debug.WriteLine("Failed to receive Data.");
                return;
            }

            Debug.WriteLine(data);
        }


        public async Task SetCtcRelay(string TcName, RaiseDrop raiseDrop)
        {
            RouteData newRouteData = await _connection?.InvokeAsync<RouteData>("SetCtcRelay", TcName, raiseDrop);
        }

        //await _connection.InvokeAsync<DataFromServer>("DriverGetsOff", OverrideDiaName);
        //previousDriveStatus = false;

        public async ValueTask DisposeAsync()
        {
            await DisposeAndStopConnectionAsync(CancellationToken.None);
        }
    }
}