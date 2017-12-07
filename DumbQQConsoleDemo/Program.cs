using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DumbQQ.Client;
using DumbQQ.Models;
using System.Drawing;

namespace DumbQQConsoleDemo
{
    public static class Program
    {
        private const string CookiePath = "dump.json";
        private static readonly DumbQQClient Client = new DumbQQClient() { CacheTimeout = TimeSpan.FromDays(1) };

        public static void Main(string[] args)
        {
            // 好友消息回调
            Client.FriendMessageReceived +=
                (sender, e) =>
                {
                    FriendMessage message = e;
                    Console.WriteLine($"{message.Sender.Alias ?? message.Sender.Nickname}:{message.Content}");
                };
            // 群消息回调
            Client.GroupMessageReceived += (sender, e) =>
            {
                GroupMessage message = e;
                Console.WriteLine(
                    $"[{message.Group.Name}]{message.Sender.Alias ?? message.Sender.Nickname}:{message.Content}");
                if (Regex.IsMatch(@"^\s*Knock knock\s*$", message.Content))
                    message.Reply("Who's there?");
                else if (message.StrictlyMentionedMe)
                    message.Reply("什么事？");
            };
            // 讨论组消息回调
            Client.DiscussionMessageReceived +=
                (sender, e) =>
                {
                    DiscussionMessage message = e;
                    Console.WriteLine($"[{message.Discussion.Name}]{message.Sender.Nickname}:{message.Content}");
                };
            // 消息回显
            Client.MessageEcho += (sender, e) => { Console.WriteLine($"{e.Target.Name}>{e.Content}"); };
            if (File.Exists(CookiePath))
            {
                // 尝试使用cookie登录
                if (Client.Start(File.ReadAllText(CookiePath)) != DumbQQClient.LoginResult.Succeeded)
                    QrLogin();
            }
            else
            {
                QrLogin();
            }
            Console.WriteLine($"欢迎，{Client.Nickname}!");
            // 导出cookie
            try
            {
                File.WriteAllText(CookiePath, Client.DumpCookies());
            }
            catch
            {
                // Ignored
            }
            // 防止程序终止
            while (Client.Status == DumbQQClient.ClientStatus.Active)
            {
            }
        }

        private static void QrLogin()
        {
            while (true)
            {
                switch (Client.Start(Program.QrCodeDownloadedCallback))
                {
                    case DumbQQClient.LoginResult.Succeeded:
                        return;
                    case DumbQQClient.LoginResult.QrCodeExpired:
                        continue;
                    default:
                        Console.WriteLine("登录失败，需要重试吗？(y/n)");
                        var response = Console.ReadLine();
                        if (Regex.IsMatch(@"^\s*y(es)?\s*$", response, RegexOptions.IgnoreCase))
                            continue;
                        Environment.Exit(1);
                        return;
                }
            }
        }

        private static void QrCodeDownloadedCallback(byte[] qrCodeData)
        {
            Image qrCodeImage;
            using (MemoryStream ms = new MemoryStream(qrCodeData))
                qrCodeImage = Image.FromStream(ms);

            string fileName = $"__qrct{DateTime.Now.ToString("yyyyMMHHmmss")}.png";
            qrCodeImage.Save(fileName);
            File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.ReadOnly | FileAttributes.Hidden);

            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo(fileName)
            };
            process.Start();
            process.WaitForExit(15000);
            process.Close();

            File.SetAttributes(fileName, File.GetAttributes(fileName) & ~(FileAttributes.ReadOnly | FileAttributes.Hidden));
            File.Delete(fileName);
        }
    }
}