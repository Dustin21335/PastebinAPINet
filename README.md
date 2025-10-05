# Pastebin API library by [Dustin](https://github.com/Dustin21335)

[![](https://img.shields.io/nuget/v/PastebinAPINet.svg?style=for-the-badge)](https://www.nuget.org/packages/PastebinAPINet/)
[![](https://img.shields.io/nuget/dt/PastebinAPINet.svg?style=for-the-badge)](https://www.nuget.org/packages/PastebinAPINet/)

# Pastebin API library by [Dustin](https://github.com/Dustin21335)

[![](https://img.shields.io/nuget/v/PastebinAPINet.svg?style=for-the-badge)](https://www.nuget.org/packages/PastebinAPINet/)
[![](https://img.shields.io/nuget/dt/PastebinAPINet.svg?style=for-the-badge)](https://www.nuget.org/packages/PastebinAPINet/)

### Examples
#### Creating a guest post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.Login();
string? pastebinURL = await pastebinClient.CreatePaste("Content", "Title", PasteExposure.Unlisted, PasteExpireDate.TenMinutes, PasteFormat.Text);
Console.WriteLine(pastebinURL);
```

#### Creating a user post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.Login();
await pastebinClient.CreatePaste("Content", "Title", PasteExposure.Unlisted, PasteExpireDate.TenMinutes, PasteFormat.Text);
```

#### Handling logging
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.logger.OnLog += (logType, message) =>
{
   Console.WriteLine($"[{logType}] {message}");
};
```

```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.logger.OnLog += (logType, message) =>
{
   if (logType != Logger.LogType.Log) Console.WriteLine($"[{logType}] {message}");
};
```

#### Handling on new post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.OnNewPaste += (pastebinURL, pastebinKey) =>
{
   Console.WriteLine($"{pastebinURL} - {pastebinKey}");
};        
_ = Task.Run(async () => await pastebinClient.StartOnPasteWatcher(10));
```

#### Deleting a user post
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.DeletePaste("PasteKey");
```

#### Getting user posts
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.Login();
string? xmlPastes = await pastebinClient.GetUsersRawPastes(50);
Console.WriteLine(xmlPastes);
```

```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.Login();
(await pastebinClient.GetUsersPastes(50))?.ForEach(p =>
{
      Console.WriteLine($"{p.Name} - {p.Exposure.ToString()} - {p.Views}");
});
```

#### Getting user details
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.Login();
string? xmlUserDetails = await pastebinClient.GetRawUserDetails();   
Console.WriteLine(xmlUserDetails);
```

```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.Login();
PastebinUserDetails? pastebinUserDetails = await pastebinClient.GetUserDetails();
if (pastebinUserDetails != null) Console.WriteLine($"{pastebinUserDetails.Name} - {pastebinUserDetails.Email} - {pastebinUserDetails.AccountType}");
```


#### Getting raw pastes
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
string? rawPaste = await pastebinClient.GetRawPaste("PasteKey");
Console.WriteLine(rawPaste);
```

```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey", "Username", "Password");
await pastebinClient.Login();
string? rawPaste = await pastebinClient.GetRawPaste("PasteKey");
Console.WriteLine(rawPaste);
```

#### Getting recent pastes
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
List<string>? recentPastes = await pastebinClient.GetRecentPastes(8);
Console.WriteLine(string.Join(", ", recentPastes ?? new List<string>()));
```

```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
List<string>? recentPastesKeys = await pastebinClient.GetRecentPastesKeys(8);
Console.WriteLine(string.Join(", ", recentPastesKeys ?? new List<string>()));
```

#### Disposing
```csharp
PastebinClient pastebinClient = new PastebinClient("DeveloperAPIKey");
pastebinClient.Dispose();
```
