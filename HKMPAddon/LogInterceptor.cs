using Hkmp.Logging;
using System;

namespace HKMPDiscordBot
{
    public class ChatEventArgs : EventArgs
    {
        public ushort PlayerId;
        public string Message;
    }
    internal class LogInterceptor : ILogger
    {
        public event EventHandler<ChatEventArgs> OnChatMessage;
        public void Debug(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Fine(string message)
        {
        }

        public void Info(string message)
        {
            if (message.StartsWith("Received chat message from"))
            {
                var messageArr = message.Split(new char[] { ' ' });
                var index = 0;
                while (messageArr[index] != "from") { index++; }
                var pid = messageArr[index + 1].Substring(0, messageArr[index + 1].Length - 1);
                while (messageArr[index] != "message:") { index++; }
                index++;
                var m = "";
                while (index < messageArr.Length)
                {
                    m += $" {messageArr[index]}";
                    index++;
                }
                m = m.Substring(2, m.Length - 3);
                ushort p = ushort.Parse(pid);
                OnChatMessage?.Invoke(this, new ChatEventArgs { PlayerId = p, Message = m });
            }

        }

        public void Warn(string message)
        {
        }
    }
}
