using System;

namespace BotsCommon.Bots
{
    public interface IApiClient
    {
        Task AddLike(string postId);
        Task CreateRepost(string postId);
        Task AddReply(string postId, string text);
        Task AddQuotedReply(string postId, string text);
        Task Subscribe(string subscribeId);
    }
}
