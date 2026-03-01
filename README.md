# 📚 SimpleLibrary

SimpleLibrary 是一個基於 `.NET 10` 的實用工具函式庫，專門提供幾種日常開發常見的整合服務，包含檔案雲端上傳、電子郵件發送、通訊軟體通知以及壓縮工具。

---

## ✨ 核心功能 (Features)

這個函式庫封裝了以下實用功能，讓你可以更快地將其引入並用於自己的專案中：

*   **☁️ AWS S3 檔案上傳**
    提供簡易的 S3 操作介面，讓你迅速上傳單一檔案或是 Zip 壓縮檔至 AWS 指定的儲存桶 (Bucket) 中，並回傳限時的預先授權下載網址 (Pre-signed URL)。
    - 支援自訂 AWS Region
    - 支援上傳進度追蹤 (Progress)
    - 支援 Async/await 異步操作
    - 支援刪除檔案功能
    - 支援檢查檔案是否存在
    - 支援自訂預先授權網址有效期

*   **📧 GMail 寄送通知**
    基於 Google SMTP 功能封裝，只要提供 Gmail 帳號與密碼 (應用程式碼)，即可群發寄送支援 HTML 格式的電子郵件通知。
    - 支援自訂 SMTP 主機與連接埠
    - 支援附件檔案
    - 支援內嵌圖片 (Inline Images)
    - 支援 Async/await 異步發送
    - 支援 CancellationToken 取消操作

*   **💬 Line Notify 通知**
    只要填寫權杖 (Token)、URL 等基本資訊，就能輕鬆透過 POST 呼叫傳出機器人推播訊息，將警報或資訊送到 Line 聊天室。
    - 支援 HttpClient (非同步)
    - 支援圖片訊息
    - 支援貼圖訊息
    - 支援快速回覆按鈕 (Quick Reply)
    - 完善的錯誤處理機制

*   **🗜️ Zip 檔案壓縮**
    利用 SharpZipLib 打造的便捷方法，能夠一次自動巡覽並將指定目錄與子目錄下的所有檔案壓縮起來，產生 `.zip` 壓縮檔。
    - 支援壓縮進度追蹤
    - 支援 Async/await 異步操作
    - **支援解壓縮功能 (Unzip)**
    - 支援解壓縮進度追蹤

*   **📝 彈性的 Logger 介面**
    提供支援依賴注入 (DI / Autofac) 的 Console 日誌工具。你能夠自訂要使用預設 `ConsoleLogger` 或是第三方的 `ColorfulLogger` 將彩色訊息記錄傳到最底層使用。
    - 支援 **檔案日誌** (FileLogger)
    - 支援 **日誌級別** (Debug, Info, Warning, Error)
    - 支援 **時間戳記** (Timestamp)
    - 可設定最低日誌級別過濾

*   **🛠️ 其他輔助套件**
    內含部分常用的擴充方法 (Extension Methods，如 `IEnumerable` 的 `ForEach`) 及設計模式基底 (例如供繼承使用的快速 `Singleton` 類別)。

---

## 📖 使用範例 (Usage Examples)

### AWS S3

```csharp
// 基本用法
var s3 = new S3("bucket-name", "access-key", "secret-key");
string url = s3.UploadFile("path/to/file.zip");

// 自訂 Region
var s3 = new S3("bucket-name", "access-key", "secret-key", Amazon.RegionEndpoint.USEast1);

// 支援進度追蹤
var progress = new Progress<int>(percent => Console.WriteLine($"進度: {percent}%"));
s3.UploadFile("path/to/file.zip", progress);

// 異步操作
string url = await s3.UploadFileAsync("path/to/file.zip");

// 刪除檔案
bool success = await s3.DeleteFileAsync("file-name.txt");
```

### GMail

```csharp
// 基本用法
var gmail = new GMail("your-email@gmail.com", "app-password");
gmail.SendMessage("顯示名稱", "主旨", "HTML內容", new List<string> { "recipient@example.com" });

// 夾帶附件
var attachments = new List<string> { "file1.pdf", "image.png" };
gmail.SendMessage("顯示名稱", "主旨", "HTML內容", recipients, attachments, null);

// 內嵌圖片
var images = new Dictionary<string, string> { { "path/to/image.png", "image-cid" } };
gmail.SendMessage("顯示名稱", "主旨", "<img src='cid:image-cid'>", recipients, null, images);

// 異步發送
await gmail.SendMessageAsync("顯示名稱", "主旨", "內容", recipients);
```

### Line

```csharp
var line = new Line("https://api.line.me/v2/bot/message/push", "user-id", "access-token");

// 文字訊息
line.Notify("Hello World!");

// 圖片訊息
line.NotifyImage("查看這張圖片", "https://example.com/image.jpg");

// 貼圖訊息
line.NotifySticker("提示訊息", "11537", "52002734");

// 快速回覆按鈕
var actions = new List<LineAction>
{
    new LineAction { Type = "message", Label = "選項A", Text = "A" },
    new LineAction { Type = "message", Label = "選項B", Text = "B" }
};
line.NotifyWithActions("請選擇", actions);

// 異步訊息
await line.NotifyAsync("Async message");
```

### Zip

```csharp
var zip = new Zip();

// 壓縮目錄
zip.ZipTo("output.zip", "input-folder");

// 支援進度追蹤
var progress = new Progress<int>(p => Console.WriteLine($"壓縮進度: {p}%"));
zip.ZipTo("output.zip", "input-folder", progress);

// 異步壓縮
await zip.ZipToAsync("output.zip", "input-folder");

// 解壓縮
zip.UnzipTo("input.zip", "output-folder");

// 異步解壓縮
await zip.UnzipToAsync("input.zip", "output-folder", progress);
```

### Logger

```csharp
// 使用 ConsoleLogger (預設)
var logger = new ConsoleLogger();

// 使用 ColorfulLogger
var colorful = new ColorfulLogger();

// 使用 FileLogger
var file = new FileLogger("logs/app.log");

// 設定顯示時間戳記
file.ShowTimestamp = true;

// 設定日誌級別
logger.MinLevel = LogLevel.Warning; // 只顯示 Warning 和 Error

// 列印訊息
logger.Print("Info 訊息", Color.White, LogLevel.Info);
logger.Debug("Debug 訊息");
logger.Info("Info 訊息");
logger.Warning("Warning 訊息");
logger.Error("Error 訊息");
```

---

## 🔧 技術與套件相依 (Tech Stack & Dependencies)

*   **Target Framework**: `.NET 10.0`
*   **Autofac**: 用於 Logger 注入。
*   **AWSSDK.S3 / Core**: AWS S3 整合。
*   **Newtonsoft.Json**: 用於解析 Line 等等 REST API 的 JSON 傳遞資料。
*   **SharpZipLib**: 提供底層的高效檔案壓縮機制。

---

## 📖 官方文件與範例

如果您需要了解如何將上述各個組件實際加入程式碼內使用：

**[Wiki 中有更進一步的程式碼使用範例與細節](https://github.com/chiisen/SimpleLibrary/wiki)**

---
