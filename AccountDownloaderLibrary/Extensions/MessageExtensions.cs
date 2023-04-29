using CloudX.Shared;

namespace AccountDownloaderLibrary.Extensions
{
    public static class MessageExtensions
    {
        public static string GetOtherUserId(this Message message)
        {
            return message.SenderId == message.OwnerId ? message.RecipientId : message.SenderId;
        }

        public static void SetOtherUserId(this Message message, string other)
        {
            if (message.SenderId == message.OwnerId)
                message.RecipientId = other;
            else
                message.SenderId = other;
        }
    }
}
