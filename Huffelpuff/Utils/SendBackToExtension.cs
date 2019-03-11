using SharpIrc;

namespace Huffelpuff.Utils
{
    public static class SendBackToExtension
    {
        public static string SendBackTo(this IrcEventArgs e)
        {
            return string.IsNullOrEmpty(e.Data.Channel)
                ? e.Data.Nick
                : e.Data.Channel;
        }
    }
}
