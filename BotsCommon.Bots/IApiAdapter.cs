using System;

namespace BotsCommon.Bots
{
    public interface IApiAdapter
    {
        Task AddLike(string postId);
        Task CreateRepost(string postId);
        Task AddReply(string postId, string text);
        Task AddQuotedReply(string postId, string text);
        Task Subscribe(string subscribeId);
    }
}
