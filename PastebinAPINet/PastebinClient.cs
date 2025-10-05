using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PastebinAPINet
{
    /// <summary>
    /// Client for interacting with Pastebin API.
    /// </summary>
    public class PastebinClient : IDisposable
    {
        /// <summary>
        /// Creates a new Pastebin client with login credentials.
        /// </summary>
        /// <param name="pastebinDeveloperKey">Pastebin developer key.</param>
        /// <param name="username">Pastebin account username.</param>
        /// <param name="password">Pastebin account password.</param>
        public PastebinClient(string pastebinDeveloperKey, string username, string password)
        {
            PastebinDeveloperKey = pastebinDeveloperKey;
            Username = username;
            Password = password;
            logger = new Logger();
            httpClient = new HttpClient();
            httpClient = new HttpClient 
            { 
                Timeout = TimeSpan.FromSeconds(15) 
            };
        }

        /// <summary>
        /// Creates a new Pastebin client without login. 
        /// </summary>
        /// <param name="pastebinDeveloperKey">Pastebin developer key.</param>
        public PastebinClient(string pastebinDeveloperKey)
        {
            PastebinDeveloperKey = pastebinDeveloperKey;
            logger = new Logger();
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        private HttpClient httpClient;

        /// <summary>
        /// Logger for messages, warnings, and errors.
        /// </summary>
        public Logger logger { get; set; }

        private string PastebinDeveloperKey { get; set; } = string.Empty;

        private string PastebinAPIUserKey { get; set; } = string.Empty;

        private string Username { get; set; } = string.Empty;

        private string Password { get; set; } = string.Empty;

        /// <summary>
        /// Bool to see if the client is logged in.
        /// </summary>
        public bool LoggedIn { get; private set; }

        private bool IsDisposed { get; set; }

        private HashSet<string> KnownPastes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Delegate for handling new pastes.
        /// </summary>
        /// <param name="pastebinURL">URL of the paste.</param>
        /// <param name="pastebinKey">Key of the paste.</param>
        public delegate void OnNewPasteHandler(string pastebinURL, string pastebinKey);

        /// <summary>
        /// Fired when a new paste is found. Only works if <see cref="StartOnPasteWatcher"/> is ran.
        /// </summary>
        public event OnNewPasteHandler? OnNewPaste;

        /// <summary>
        /// Logs in to Pastebin using the login credentials provided.
        /// </summary>
        public async Task Login()
        {
            if (LoggedIn)
            {
                logger.Warning("Already logged in");
                return;
            }
            try
            {
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("https://pastebin.com/api/api_login.php", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["api_dev_key"] = PastebinDeveloperKey,
                    ["api_user_name"] = Username,
                    ["api_user_password"] = Password
                }));
                string response = await httpResponseMessage.Content.ReadAsStringAsync();
                logger.Log($"Login response {response}");
                if (httpResponseMessage.IsSuccessStatusCode && !response.StartsWith("Bad API request"))
                {
                    PastebinAPIUserKey = response.Trim();
                    LoggedIn = true;
                    logger.Log("Logged in");
                }
                else logger.Log("Failed to log in");
            }
            catch (Exception ex)
            {
                logger.Error($"Login failed {ex.Message}");
            }
        }

        /// <summary>
        /// Starts watching Pastebin for new public pastes at the given interval.
        /// Fires <see cref="OnNewPaste"/> for each new paste it finds.
        /// </summary>
        /// <param name="interval">Time in seconds between checks.</param>
        public async Task StartOnPasteWatcher(int interval)
        {
            while (true)
            {
                try
                {
                    IEnumerable<string> RecentPastes = await GetRecentPastes() ?? Enumerable.Empty<string>();
                    RecentPastes.Where(p => KnownPastes.Add(p.Replace("https://pastebin.com/", ""))).ToList().ForEach(p => OnNewPaste?.Invoke(p, p.Replace("https://pastebin.com/", "")));
                    KnownPastes.RemoveWhere(k => !RecentPastes.Select(p => p.Replace("https://pastebin.com/", "")).ToHashSet().Contains(k));
                }
                catch (Exception ex)
                {
                    logger.Error($"On paste watcher failed {ex.Message}");
                }
                await Task.Delay(interval * 1000);
            }
        }

        /// <summary>
        /// Creates a new paste for the logged in user.
        /// </summary>
        /// <param name="content">Content of the paste.</param>
        /// <param name="name">Optional name of the paste.</param>
        /// <param name="pasteExposure">Optional exposure of the paste.</param>
        /// <param name="pasteExpireDate">Optional date for paste to expire.</param>
        /// <param name="pasteFormat">Optional paste format for syntax highlighting.</param>
        /// <param name="folderKey">Optional folder key to put the paste in a folder.</param>
        /// <returns>Paste URL</returns>
        public async Task<string?> CreatePaste(string content, string? name = null, PasteExposure pasteExposure = default, PasteExpireDate pasteExpireDate = default, PasteFormat pasteFormat = default, string? folderKey = null)
        {
            if (!LoggedIn)
            {
                logger.Warning("Not logged in call Login()");
                return null;
            }
            return await Post(PasteOption.Paste, content, name, PastebinAPIUserKey, pasteExposure, pasteExpireDate, pasteFormat, folderKey);
        }

        /// <summary>
        /// Creates a guest paste without logging in.
        /// </summary>
        /// <param name="content">Content of the paste.</param>
        /// <param name="name">Optional name of the paste.</param>
        /// <param name="pasteExposure">Optional exposure of the paste.</param>
        /// <param name="pasteExpireDate">Optional date for paste to expire.</param>
        /// <param name="pasteFormat">Optional paste format for syntax highlighting.</param>
        /// <returns>Paste URL</returns>
        public async Task<string?> CreateGuestPaste(string content, string? name = null, PasteExposure pasteExposure = default, PasteExpireDate pasteExpireDate = default, PasteFormat pasteFormat = default)
        {
            return await Post(PasteOption.Paste, content, name, null, pasteExposure, pasteExpireDate, pasteFormat);
        }

        /// <summary>
        /// Deletes a paste by key.
        /// </summary>
        /// <param name="pasteKey">The key of the paste to delete.</param>
        /// <returns>API response</returns>
        public async Task<string?> DeletePaste(string pasteKey)
        {
            if (!LoggedIn)
            {
                logger.Warning("Not logged in call Login()");
                return null;
            }
            if (string.IsNullOrEmpty(pasteKey))
            {
                logger.Error("Paste key can't be null");
                return null;
            }
            return await Post(PasteOption.Delete, null, null, PastebinAPIUserKey, default, default, default, null, pasteKey);
        }

        /// <summary>
        /// Gets raw xml pastes for the logged in user.
        /// </summary>
        /// <param name="limit">The maximum number of raw XML pastes to retrieve (1–1000).</param>
        /// <returns>Raw xml pastes</returns>
        public async Task<string?> GetUsersRawPastes(int limit = 50)
        {
            if (!LoggedIn)
            {
                logger.Warning("Not logged in call Login()");
                return null;
            }
            if (limit < 1 || limit > 1000)
            {
                logger.Error("Limit must be between 1 and 1000");
                return null;
            }
            return await Post(PasteOption.List, null, null, PastebinAPIUserKey, default, default, default, null, null, limit.ToString());
        }

        /// <summary>
        /// Gets pastes as a class from XML for the logged in user.
        /// </summary>
        /// <param name="limit">The maximum number of pastes to retrieve (1–1000).</param>
        /// <returns><see cref="PastebinPaste"/></returns>
        public async Task<List<PastebinPaste>?> GetUsersPastes(int limit = 50)
        {
            string? rawXml = await GetUsersRawPastes(limit);
            if (string.IsNullOrEmpty(rawXml)) return null;
            try
            {
                return (new XmlSerializer(typeof(PasteList)).Deserialize(new StringReader($"<pastes>{rawXml}</pastes>")) as PasteList)?.Pastes;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to parse user pastes {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Gets the raw XML of user details for the logged in user.
        /// </summary>
        public async Task<string?> GetRawUserDetails()
        {
            if (!LoggedIn)
            {
                logger.Warning("Not logged in call Login()");
                return null;
            }
            return await Post(PasteOption.UserDetails, null, null, PastebinAPIUserKey);
        }

        /// <summary>
        /// Gets user details as a class from XML for the logged in user.
        /// </summary>
        /// <returns><see cref="PastebinUserDetails"/></returns>
        public async Task<PastebinUserDetails?> GetUserDetails()
        {
            string? rawXml = await GetRawUserDetails();
            if (string.IsNullOrEmpty(rawXml)) return null;
            try
            {
                return new XmlSerializer(typeof(PastebinUserDetails)).Deserialize(new StringReader(rawXml)) as PastebinUserDetails ?? null;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to parse user details {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Gets the raw content of a paste by its key.
        /// </summary>
        /// <param name="pasteKey">The paste key.</param>
        /// <returns>Raw text</returns>
        public async Task<string?> GetRawPaste(string pasteKey)
        {
            if (string.IsNullOrEmpty(pasteKey))
            {
                logger.Error("Paste key can't be null");
                return null;
            }
            try
            {
                string? apiResult = await Post(PasteOption.ShowPaste, null, null, PastebinAPIUserKey, default, default, default, default, pasteKey);
                if (!string.IsNullOrEmpty(apiResult)) return apiResult;
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"https://pastebin.com/raw/{pasteKey}");
                string response = await httpResponseMessage.Content.ReadAsStringAsync();
                logger.Log($"Response for raw paste {response}");
                if (httpResponseMessage.IsSuccessStatusCode || !response.StartsWith("Bad API request")) return response;
                logger.Error($"Failed to get raw paste");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get raw paste {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Gets the most recent public pastes.
        /// </summary>
        /// <param name="limit">The maximum number of pastes to retrieve (1–8).</param>
        /// <returns>Pastebin URLs</returns>
        public async Task<List<string>?> GetRecentPastes(int limit = 8)
        {
            if (limit < 1 || limit > 8)
            {
                logger.Error("Limit must be between 1 and 8");
                return null;
            }
            try
            {
                Match sidebarMatch = Regex.Match(await httpClient.GetStringAsync("https://pastebin.com/"), @"<ul class=""sidebar__menu"">(.*?)</ul>", RegexOptions.Singleline);
                if (sidebarMatch.Success)
                {
                    List<string> pastes = Regex.Matches(sidebarMatch.Groups[1].Value, @"<a href=""(\/[A-Za-z0-9]+)"">").Cast<Match>().Select(m => "https://pastebin.com" + (m.Groups[1].Value ?? "")).ToList<string>();
                    logger.Log($"Successfully got {pastes.Count} pastes");
                    return pastes;
                }
                logger.Error("Failed to get recent pastes");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get recent pastes {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Gets the most recent public pastes keys.
        /// </summary>
        /// <param name="limit">The maximum number of pastes to retrieve (1–8).</param>
        /// <returns>Pastebin keys</returns>
        public async Task<List<string>?> GetRecentPastesKeys(int limit = 8)
        {
            return (await GetRecentPastes(limit))?.Select(p => p.Replace("https://pastebin.com/", "")).Where(p => !string.IsNullOrEmpty(p)).ToList();
        }

        /// <summary>
        /// Sends a request to Pastebin API for various tasks.
        /// </summary>
        /// <returns>API response</returns>
        public async Task<string?> Post(PasteOption pasteOption, string? content = null, string? name = null, string? userAPIKey = null, PasteExposure pasteExposure = default, PasteExpireDate pasteExpireDate = default, PasteFormat pasteFormat = default, string? folderKey = null, string? pasteKey = null, string? limit = null)
        {
            if (!LoggedIn && pasteExposure == PasteExposure.Private)
            {
                logger.Warning("Private can only be used when logged in");
                return null;
            }
            if (string.IsNullOrEmpty(content) && pasteOption == PasteOption.Paste)
            {
                logger.Error("Content can't be null when paste option is paste");
                return null;
            }
            try
            {
                Dictionary<string, string> payload = new Dictionary<string, string>
                {
                    ["api_dev_key"] = PastebinDeveloperKey,
                    ["api_option"] = pasteOption switch
                    {
                        PasteOption.ShowPaste => "show_paste",
                        _ => pasteOption.ToString().ToLower(),
                    }
                };
                if (!string.IsNullOrEmpty(content)) payload["api_paste_code"] = content;
                if (!string.IsNullOrEmpty(name)) payload["api_paste_name"] = name;
                if (!string.IsNullOrEmpty(userAPIKey)) payload["api_user_key"] = userAPIKey;
                if (pasteExposure != default) payload["api_paste_private"] = ((int)pasteExposure).ToString();
                if (pasteExpireDate != default)
                {
                    payload["api_paste_expire_date"] = pasteExpireDate switch
                    {
                        PasteExpireDate.Never => "N",
                        PasteExpireDate.TenMinutes => "10M",
                        PasteExpireDate.OneHour => "1H",
                        PasteExpireDate.OneDay => "1D",
                        PasteExpireDate.OneWeek => "1W",
                        PasteExpireDate.TwoWeeks => "2W",
                        PasteExpireDate.OneMonth => "1M",
                        PasteExpireDate.SixMonths => "6M",
                        PasteExpireDate.OneYear => "1Y",
                        _ => "N"
                    };
                }
                if (!string.IsNullOrEmpty(folderKey)) payload["api_folder_key"] = folderKey;
                if (!string.IsNullOrEmpty(pasteKey)) payload["api_paste_key"] = pasteKey;
                if (!string.IsNullOrEmpty(limit)) payload["api_results_limit"] = limit;
                if (pasteFormat != default)
                {
                    payload["api_paste_format"] = pasteFormat switch
                    {
                        PasteFormat.FourCS => "4cs",
                        PasteFormat.ACME6502 => "6502acme",
                        PasteFormat.KickAss6502 => "6502kickass",
                        PasteFormat.TASM6502 => "6502tasm",
                        PasteFormat.APTSources => "apt_sources",
                        PasteFormat.Batch => "dos",
                        PasteFormat.Blitz3D => "b3d",
                        PasteFormat.BrainFuck => "bf",
                        PasteFormat.CWinAPI => "c_winapi",
                        PasteFormat.CPPWinAPI => "cpp-winapi",
                        PasteFormat.CPPQt => "cpp-qt",
                        PasteFormat.CLoadRunner => "c_loadrunner",
                        PasteFormat.CMac => "c_mac",
                        PasteFormat.CloneC => "klonec",
                        PasteFormat.CloneCPP => "klonecpp",
                        PasteFormat.ColdFusion => "cfm",
                        PasteFormat.Easytrieve => "ezt",
                        PasteFormat.FOLanguage => "fo",
                        PasteFormat.FormulaOne => "f1",
                        PasteFormat.GameMaker => "gml",
                        PasteFormat.GodotGLSL => "godot-glsl",
                        PasteFormat.HTML4 => "html4strict",
                        PasteFormat.InnoScript => "inno",
                        PasteFormat.JCL => "jd",
                        PasteFormat.LibertyBASIC => "lb",
                        PasteFormat.M68000 => "m68k",
                        PasteFormat.MIXAssembler => "mmix",
                        PasteFormat.MK61 => "mk-61",
                        PasteFormat.DevPAC68000 => "68000devpac",
                        PasteFormat.ObjectiveC => "objc",
                        PasteFormat.OCamlBrief => "ocaml-brief",
                        PasteFormat.OpenBSDPacketFilter => "pf",
                        PasteFormat.OpenGLShading => "glsl",
                        PasteFormat.OpenObjectRexx => "oorexx",
                        PasteFormat.OpenOfficeBASIC => " oobas",
                        PasteFormat.PHPBrief => "php-brief",
                        PasteFormat.PythonS60 => "pys60",
                        PasteFormat.RBScript => "rbs",
                        PasteFormat.RubyGnuplot => "gnuplot",
                        PasteFormat.SuperCollider => "sclang",
                        PasteFormat.UnrealScript => "uscript",
                        PasteFormat.XOrgConfig => "xorg_conf",
                        PasteFormat.Z80Assembler => "z80",
                        _ => pasteFormat.ToString().ToLower() ?? ""
                    };
                }
                logger.Log($"Sending payload {string.Join(", ", payload.Select(p => $"{p.Key} = {Uri.EscapeDataString(p.Value)}"))}");
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("https://pastebin.com/api/api_post.php", new FormUrlEncodedContent(payload));
                string response = await httpResponseMessage.Content.ReadAsStringAsync();
                logger.Log($"Post response {response}");
                if (httpResponseMessage.IsSuccessStatusCode && !response.StartsWith("Bad API request"))
                {
                    logger.Log("Post successful");
                    return response;
                }
                logger.Error("Post failed");
            }
            catch (Exception ex)
            {
                logger.Error($"Post failed {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Changes the HttpClient to use the a proxy and tests the connection.
        /// </summary>
        /// <param name="httpClientHandler">Proxy settings.</param>
        public async Task ChangeProxy(HttpClientHandler httpClientHandler)
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                logger.Log("Disposing of old http client");
            }
            httpClient = new HttpClient(httpClientHandler);
            logger.Log("Updated http client to include proxy");
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("https://api.ipify.org");
                logger.Log($"Proxy test response {await response.Content.ReadAsStringAsync()}");
                if (response.IsSuccessStatusCode) logger.Log($"Proxy connection successful");
                else logger.Warning($"Proxy connection failed");
            }
            catch (Exception ex)
            {
                logger.Error($"Proxy connection failed {ex.Message}");
            }
        }

        /// <summary>
        /// Changes the HttpClient to use the a proxy and tests the connection.
        /// </summary>
        /// <param name="ip">Proxy IP address.</param>
        /// <param name="port">Proxy port.</param>
        /// <param name="username">Optional username for proxy.</param>
        /// <param name="password">Optional password for proxy.</param>
        public async Task ChangeProxy(string ip, string port, string? username = null, string? password = null)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                Proxy = new WebProxy($"http://{ip}:{port}"),
                UseProxy = true
            };
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) httpClientHandler.Proxy.Credentials = new NetworkCredential(username, password);
            await ChangeProxy(httpClientHandler);
        }

        /// <summary>
        /// Disposes the HttpClient and marks this client as disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing) httpClient?.Dispose();
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Disposes the client and suppresses finalization.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}