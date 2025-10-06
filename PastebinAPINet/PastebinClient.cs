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
        /// Class for interacting with user only actions.
        /// </summary>
        public class PastebinUser
        {
            /// <summary>
            /// Creates a new Pastebin User.
            /// </summary>
            /// <param name="pastebinClient">Pastebin Client instance.</param>
            /// <param name="username">Pastebin account username.</param>
            /// <param name="password">Pastebin account password.</param>
            public PastebinUser(PastebinClient pastebinClient, string username, string password)
            {
                this.pastebinClient = pastebinClient;
                Username = username;
                Password = password;
                logger = new Logger();
            }

            private PastebinClient pastebinClient { get; set; }

            private string PastebinAPIUserKey { get; set; } = string.Empty;

            private string Username { get; set; } = string.Empty;

            private string Password { get; set; } = string.Empty;

            /// <summary>
            /// Logger for messages, warnings, and errors on Pastebin User.
            /// </summary>
            public Logger logger { get; set; }

            /// <summary>
            /// Bool to see if the user is logged in.
            /// </summary>
            public bool LoggedIn { get; private set; }

            /// <summary>
            /// Logs into pastebin to get a user key with the login credentials.
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
                    HttpResponseMessage httpResponseMessage = await pastebinClient.httpClient.PostAsync("https://pastebin.com/api/api_login.php", new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["api_dev_key"] = pastebinClient.DeveloperKey,
                        ["api_user_name"] = Username,
                        ["api_user_password"] = Password
                    }));
                    string response = await httpResponseMessage.Content.ReadAsStringAsync();
                    logger.Log($"Login response {response}");
                    if (httpResponseMessage.IsSuccessStatusCode && !response.StartsWith("Bad API request"))
                    {
                        PastebinAPIUserKey = response.Trim();
                        LoggedIn = true;
                        Username = string.Empty;
                        Password = string.Empty;
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
            /// Creates a new paste for the user.
            /// </summary>
            /// <param name="content">Content of the paste.</param>
            /// <param name="name">Optional name of the paste.</param>
            /// <param name="pasteExposure">Optional exposure of the paste.</param>
            /// <param name="pasteExpireDate">Optional date for paste to expire.</param>
            /// <param name="pasteFormat">Optional paste format for syntax highlighting.</param>
            /// <param name="folderKey">Optional folder key to put the paste in a folder.</param>
            /// <returns>Paste URL</returns>
            /// <remarks>
            /// If the user is not logged in, call <see cref="Login"/> first.
            /// </remarks>
            public async Task<string?> CreatePaste(string content, string? name = null, PasteExposure pasteExposure = default, PasteExpireDate pasteExpireDate = default, PasteFormat pasteFormat = default, string? folderKey = null)
            {
                if (!LoggedIn)
                {
                    logger.Warning("Not logged in call Login()"); 
                    return null;
                }
                return await pastebinClient.CreatePastebinRequest().WithOption(PasteOption.Paste).WithContent(content).WithName(name).WithExposure(pasteExposure).WithExpireDate(pasteExpireDate).WithFormat(pasteFormat).WithFolderKey(folderKey).Send();
            }

            /// <summary>
            /// Deletes a paste by key.
            /// </summary>
            /// <param name="pasteKey">The key of the paste to delete.</param>
            /// <returns>API response</returns>
            /// <remarks>
            /// If the user is not logged in, call <see cref="Login"/> first.
            /// </remarks>
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
                return await pastebinClient.CreatePastebinRequest().WithOption(PasteOption.Delete).WithUserAPIKey(PastebinAPIUserKey).WithPasteKey(pasteKey).Send();
            }

            /// <summary>
            /// Gets raw user XML pastes for the user.
            /// </summary>
            /// <param name="limit">The maximum number of raw XML pastes to retrieve (1–1000).</param>
            /// <returns>Raw user XML pastes</returns>
            /// <remarks>
            /// If the user is not logged in, call <see cref="Login"/> first.
            /// </remarks>
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
                return await pastebinClient.CreatePastebinRequest().WithOption(PasteOption.List).WithUserAPIKey(PastebinAPIUserKey).WithLimit(limit).Send();
            }

            /// <summary>
            /// Gets pastes as a class from XML for the user.
            /// </summary>
            /// <param name="limit">The maximum number of pastes to get (1–1000).</param>
            /// <returns><see cref="PastebinPaste"/></returns>
            /// <remarks>
            /// If the user is not logged in, call <see cref="Login"/> first.
            /// </remarks>
            public async Task<List<PastebinPaste>?> GetUsersPastes(int limit = 50)
            {
                string? rawXml = await GetUsersRawPastes(limit);
                if (string.IsNullOrEmpty(rawXml))
                {
                    logger.Warning("User paste XML is empty");
                    return null;
                }
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
            /// Gets raw user XML details for the user.
            /// </summary>
            /// <returns>Raw user XML details</returns>
            /// <remarks>
            /// If the user is not logged in, call <see cref="Login"/> first.
            /// </remarks>
            public async Task<string?> GetRawUserDetails()
            {
                if (!LoggedIn)
                {
                    logger.Warning("Not logged in call Login()");
                    return null;
                }
                return await pastebinClient.CreatePastebinRequest().WithOption(PasteOption.UserDetails).WithUserAPIKey(PastebinAPIUserKey).Send();
            }

            /// <summary>
            /// Gets user details as a class from XML for the logged in user.
            /// </summary>
            /// <returns><see cref="PastebinUserDetails"/></returns>
            /// <remarks>
            /// If the user is not logged in, call <see cref="Login"/> first.
            /// </remarks>
            public async Task<PastebinUserDetails?> GetUserDetails()
            {
                string? rawXml = await GetRawUserDetails();
                if (string.IsNullOrEmpty(rawXml))
                {
                    logger.Warning("User details XML is empty");
                    return null;
                }
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
            /// <returns>Raw content</returns>
            /// <remarks>
            /// If the user is not logged in, call <see cref="Login"/> first.
            /// Must be owned by the user.
            /// </remarks>
            public async Task<string?> GetRawUserPaste(string pasteKey)
            {
                if (string.IsNullOrEmpty(pasteKey))
                {
                    logger.Error("Paste key can't be null");
                    return null;
                }
                try
                {
                    return await pastebinClient.CreatePastebinRequest().WithOption(PasteOption.ShowPaste).WithPasteKey(pasteKey).WithUserAPIKey(PastebinAPIUserKey).Send();
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to get raw paste {ex.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Class for interacting with the central Pastebin API.
        /// </summary>
        public class PastebinRequest
        {
            /// <summary>
            /// Creates a new Pastebin Request.
            /// </summary>
            /// <param name="pastebinClient">Pastebin Client instance.</param>
            public PastebinRequest(PastebinClient pastebinClient)
            {
                this.pastebinClient = pastebinClient;
                parameters["api_dev_key"] = pastebinClient.DeveloperKey;
            }

            private PastebinClient pastebinClient { get; set; }

            private Dictionary<string, string> parameters { get; set; } = new Dictionary<string, string>();

            /// <summary>
            /// Adds a option.
            /// </summary>
            /// <param name="pasteOption">Paste options to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithOption(PasteOption pasteOption)
            {
                parameters["api_option"] = pasteOption switch
                {
                    PasteOption.ShowPaste => "show_paste",
                    _ => pasteOption.ToString().ToLower(),
                };
                return this;
            }

            /// <summary>
            /// Adds content.
            /// </summary>
            /// <param name="content">Paste content to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithContent(string? content)
            {
                if (!string.IsNullOrEmpty(content)) parameters["api_paste_code"] = content;
                return this;
            }

            /// <summary>
            /// Adds name.
            /// </summary>
            /// <param name="name">Paste name to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithName(string? name)
            {
                if (!string.IsNullOrEmpty(name)) parameters["api_paste_name"] = name;
                return this;
            }

            /// <summary>
            /// Adds user API key.
            /// </summary>
            /// <param name="userAPIKey">Paste user API key to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithUserAPIKey(string? userAPIKey)
            {
                if (!string.IsNullOrEmpty(userAPIKey)) parameters["api_user_key"] = userAPIKey;
                return this;
            }

            /// <summary>
            /// Adds exposure.
            /// </summary>
            /// <param name="pasteExposure">Paste exposure to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithExposure(PasteExposure pasteExposure)
            {
                parameters["api_paste_private"] = ((int)pasteExposure).ToString();
                return this;
            }

            /// <summary>
            /// Adds expire date.
            /// </summary>
            /// <param name="pasteExpireDate">Paste expire date to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithExpireDate(PasteExpireDate pasteExpireDate)
            {
                parameters["api_paste_expire_date"] = pasteExpireDate switch
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
                return this;
            }

            /// <summary>
            /// Adds folder key.
            /// </summary>
            /// <param name="folderKey">Paste folder key to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithFolderKey(string? folderKey)
            {
                if (!string.IsNullOrEmpty(folderKey)) parameters["api_folder_key"] = folderKey;
                return this;
            }

            /// <summary>
            /// Adds paste key.
            /// </summary>
            /// <param name="pasteKey">Paste key to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithPasteKey(string? pasteKey)
            {
                if (!string.IsNullOrEmpty(pasteKey)) parameters["api_paste_key"] = pasteKey;
                return this;
            }

            /// <summary>
            /// Adds limit.
            /// </summary>
            /// <param name="limit">Paste limit to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithLimit(int limit)
            {
                parameters["api_results_limit"] = limit.ToString();
                return this;
            }

            /// <summary>
            /// Adds format.
            /// </summary>
            /// <param name="pasteFormat">Paste format to send.</param>
            /// <returns><see cref="PastebinRequest"/></returns>
            public PastebinRequest WithFormat(PasteFormat pasteFormat)
            {
                parameters["api_paste_format"] = pasteFormat switch
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
                return this;
            }

            /// <summary>
            /// Sends parameters selected to Pastebin API.
            /// </summary>
            /// <returns>API response</returns>
            public async Task<string?> Send()
            {
                try
                {
                    HttpResponseMessage httpResponseMessage = await pastebinClient.httpClient.PostAsync("https://pastebin.com/api/api_post.php", new FormUrlEncodedContent(parameters));
                    string response = await httpResponseMessage.Content.ReadAsStringAsync();
                    pastebinClient.logger.Log($"Post response {response}");
                    if (httpResponseMessage.IsSuccessStatusCode && !response.StartsWith("Bad API request"))
                    {
                        pastebinClient.logger.Log("Post successful");
                        return response;
                    }
                    pastebinClient.logger.Error("Post failed");
                }
                catch (Exception ex)
                {
                    pastebinClient.logger.Error($"Send failed {ex.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Creates a new Pastebin client.
        /// </summary>
        /// <param name="developerKey">Pastebin developer key.</param>
        public PastebinClient(string developerKey)
        {
            DeveloperKey = developerKey;
            logger = new Logger();
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        /// <summary>
        /// Logger for messages, warnings, and errors on Pastebin Client.
        /// </summary>
        public Logger logger { get; set; }

        /// <summary>
        /// Delegate for handling new pastes.
        /// </summary>
        /// <param name="pastebinURL">URL of the paste.</param>
        /// <param name="pastebinKey">Key of the paste.</param>
        public delegate void OnNewPasteHandler(string pastebinURL, string pastebinKey);

        /// <summary>
        /// Fired when a new paste is found. Only works if <see cref="StartOnPasteWatcher"/> is started.
        /// </summary>
        public event OnNewPasteHandler? OnNewPaste;

        private HttpClient httpClient { get; set; }

        private bool IsDisposed { get; set; }

        private string DeveloperKey { get; set; } = string.Empty;

        private HashSet<string> KnownPastes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Starts watching Pastebin for new public pastes at the provided interval.
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
        /// Creates a guest paste.
        /// </summary>
        /// <param name="content">Content of the paste.</param>
        /// <param name="name">Optional name of the paste.</param>
        /// <param name="pasteExposure">Optional exposure of the paste.</param>
        /// <param name="pasteExpireDate">Optional date for paste to expire.</param>
        /// <param name="pasteFormat">Optional paste format for syntax highlighting.</param>
        /// <returns>Paste URL</returns>
        public async Task<string?> CreateGuestPaste(string content, string? name = null, PasteExposure pasteExposure = default, PasteExpireDate pasteExpireDate = default, PasteFormat pasteFormat = default)
        {
            return await CreatePastebinRequest().WithOption(PasteOption.Paste).WithContent(content).WithName(name).WithExposure(pasteExposure).WithExpireDate(pasteExpireDate).WithFormat(pasteFormat).Send();
        }

        /// <summary>
        /// Gets the raw content of a paste by its key if its not private.
        /// </summary>
        /// <param name="pasteKey">The paste key.</param>
        /// <returns>Raw content</returns>
        /// <remarks>
        /// This won't work on private pastes.
        /// </remarks>
        public async Task<string?> GetRawPaste(string pasteKey)
        {
            if (string.IsNullOrEmpty(pasteKey))
            {
                logger.Error("Paste key can't be null");
                return null;
            }
            try
            {
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
        /// <param name="limit">The maximum number of pastes to get (1–8).</param>
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
        /// <returns>Paste keys</returns>
        public async Task<List<string>?> GetRecentPastesKeys(int limit = 8)
        {
            return (await GetRecentPastes(limit))?.Select(p => p.Replace("https://pastebin.com/", "")).Where(p => !string.IsNullOrEmpty(p)).ToList();
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
        /// Creases a Pastebin Request.
        /// </summary>
        /// <returns><see cref="PastebinRequest"/></returns>
        public PastebinRequest CreatePastebinRequest()
        {
            return new PastebinRequest(this);
        }

        /// <summary>
        /// Creases a Pastebin User.
        /// </summary>
        /// <param name="username">Pastebin account username.</param>
        /// <param name="password">Pastebin account password.</param>
        /// <returns><see cref="PastebinUser"/></returns>
        public PastebinUser CreatePastebinUser(string username, string password)
        {
            return new PastebinUser(this, username, password);
        }

        /// <summary>
        /// Disposes the HttpClient and marks this client as disposed.
        /// </summary>
        /// <param name="disposing">True if called from <see cref="Dispose()"/>, false if called from finalizer.</param>
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