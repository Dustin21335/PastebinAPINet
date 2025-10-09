# Pastebin API library by [Dustin](https://github.com/Dustin21335)

[![](https://img.shields.io/nuget/v/PastebinAPINet.svg?style=for-the-badge)](https://www.nuget.org/packages/PastebinAPINet/)
[![](https://img.shields.io/nuget/dt/PastebinAPINet.svg?style=for-the-badge)](https://www.nuget.org/packages/PastebinAPINet/)

### If you'd like to support me please star this github.

### Examples
#### Creating pastebin client
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
```

#### Creating a guest post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
string? pastebinURL = await pastebinClient.CreateGuestPaste("Content", "Title", PasteExposure.Unlisted, PasteExpireDate.TenMinutes, PasteFormat.Text);
Console.WriteLine(pastebinURL);
```

#### Handling logging
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.logger.OnLog += (logType, message) =>
{
   Console.WriteLine($"[{logType}] {message}");
};
```

- Only logging warnings, and errors.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.logger.OnLog += (logType, message) =>
{
   if (logType != Logger.LogType.Log) Console.WriteLine($"[{logType}] {message}");
};
```

#### Using on new post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.OnNewPaste += (pastebinURL, pastebinKey) =>
{
   Console.WriteLine($"{pastebinURL} - {pastebinKey}");
};        
_ = Task.Run(async () => await pastebinClient.StartOnPasteWatcher(10));
```

#### Getting raw pastes
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
string? rawPaste = await pastebinClient.GetRawPaste("PasteKey");
Console.WriteLine(rawPaste);
```


#### Getting recent pastes
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
List<string>? recentPastes = await pastebinClient.GetRecentPastes(8);
Console.WriteLine(string.Join(", ", recentPastes ?? new List<string>()));
```
- Returns keys instead of paste URLs.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
List<string>? recentPastesKeys = await pastebinClient.GetRecentPastesKeys(8);
Console.WriteLine(string.Join(", ", recentPastesKeys ?? new List<string>()));
```

#### Changing proxies
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
await pastebinClient.ChangeProxy("Ip", "Port");
```
- Changing proxy with username and password.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
await pastebinClient.ChangeProxy("Ip", "Port", "Username", "Password");
```

- Changing proxy with HttpClientHandler.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
await pastebinClient.ChangeProxy(new HttpClientHandler()
{
      Proxy = new WebProxy("http://Ip:Port"),
      UseProxy = true
});
```

```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
await pastebinClient.ChangeProxy(new HttpClientHandler()
{
      Proxy = new WebProxy("http://Ip:Port")
      {
         Credentials = new NetworkCredential("Username", "Password")
      },
      UseProxy = true
});
```

#### Disposing
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.Dispose();
```

### User Examples
#### Creating pastebin user
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
```
- Another way to create pastebin user.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = new PastebinClient.PastebinUser(pastebinClient, "Username", "Password");
await pastebinUser.Login();
```

#### Handling user logging
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
pastebinUser.logger.OnLog += (logType, message) =>
{
    Console.WriteLine($"[{logType}] {message}");
};
```

- Only logging warnings, and errors.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
pastebinUser.logger.OnLog += (logType, message) =>
{
    if (logType != Logger.LogType.Log) Console.WriteLine($"[{logType}] {message}");
};
```

#### Creating a user post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
await pastebinUser.CreatePaste("Content", "Title", PasteExposure.Unlisted, PasteExpireDate.TenMinutes, PasteFormat.Text);
```

#### Deleting a user post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
await pastebinUser.DeletePaste("PasteKey");
```

#### Getting user posts
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
string? xmlPastes = await pastebinUser.GetUsersRawPastes(50);
Console.WriteLine(xmlPastes);
```
- Uses the pastebin paste class instead.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
(await pastebinUser.GetUsersPastes(50))?.ForEach(p =>
{
      Console.WriteLine($"{p.Name} - {p.Exposure.ToString()} - {p.Views}");
});
```

#### Getting user details
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
string? xmlUserDetails = await pastebinUser.GetRawUserDetails();
Console.WriteLine(xmlUserDetails);
```
- Uses the pastebin user details class instead.
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
PastebinUserDetails? pastebinUserDetails = await pastebinUser.GetUserDetails();
if (pastebinUserDetails != null) Console.WriteLine($"{pastebinUserDetails.Name} - {pastebinUserDetails.Email} - {pastebinUserDetails.AccountType}");
```

#### Getting raw user pastes
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
PastebinClient.PastebinUser pastebinUser = pastebinClient.CreatePastebinUser("Username", "Password");
await pastebinUser.Login();
string? rawPaste = await pastebinUser.GetRawUserPaste("PasteKey");
Console.WriteLine(rawPaste);
```