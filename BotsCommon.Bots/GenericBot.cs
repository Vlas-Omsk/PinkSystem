using System;
using BotsCommon.IO;
using PinkLogging;

namespace BotsCommon.Bots
{
    public sealed class GenericBot : IBot
    {
        private readonly Task[] _tasks;
        private readonly IDataReader<Task<IApiClient>> _dataReader;
        private readonly string _postId;
        private readonly bool _like;
        private readonly bool _repost;
        private readonly bool _reply;
        private readonly IDataReader<string> _repliesReader;
        private readonly bool _quoteReply;
        private readonly IDataReader<string> _userTagsReader;
        private readonly int _quoteReplyUserTagsCount;
        private readonly ILogger _logger;
        private readonly string _subscribeId;
        private readonly int _numberOfRepliesPerCookie;
        private readonly int _numberOfQuoteRepliesPerCookie;

        public GenericBot(
            int threadsCount,
            IDataReader<Task<IApiClient>> dataReader,
            string postId,
            bool like,
            bool repost,
            bool reply,
            IDataReader<string> repliesReader,
            bool quoteReply,
            IDataReader<string> userTagsReader,
            int quoteReplyUserTagsCount,
            string subscribeId,
            int numberOfRepliesPerCookie,
            int numberOfQuoteRepliesPerCookie,
            ILogger logger
        )
        {
            _tasks = new Task[threadsCount];
            _dataReader = dataReader;
            _postId = postId;
            _like = like;
            _repost = repost;
            _reply = reply;
            _repliesReader = repliesReader;
            _quoteReply = quoteReply;
            _userTagsReader = userTagsReader;
            _quoteReplyUserTagsCount = quoteReplyUserTagsCount;
            _subscribeId = subscribeId;
            _numberOfRepliesPerCookie = numberOfRepliesPerCookie;
            _numberOfQuoteRepliesPerCookie = numberOfQuoteRepliesPerCookie;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            Task<IApiClient> apiClientTask;

            while ((apiClientTask = _dataReader.Read()) != null)
            {
                var freeTaskIndex = Array.FindIndex(_tasks, x => x?.IsCompleted ?? true);

                _tasks[freeTaskIndex] = HandleAsync(apiClientTask);

                if (_tasks.All(x => x != null))
                {
                    var task = await Task.WhenAny(_tasks);
                    if (task.Exception != null)
                        break;
                }
            }

            await Task.WhenAll(_tasks);
        }

        private async Task HandleAsync(Task<IApiClient> apiClientTask)
        {
            var tasks = Enumerable.Repeat(Task.CompletedTask, 5).ToArray();

            var apiClient = await apiClientTask;

            if (_like)
                tasks[0] = apiClient.AddLike(_postId);
            if (_repost)
                tasks[1] = apiClient.CreateRepost(_postId);
            if (_reply)
            {
                tasks[2] = Task.Run(async () =>
                {
                    for (var i = 0; i < _numberOfRepliesPerCookie; i++)
                    {
                        var text = _repliesReader.Read();

                        await apiClient.AddReply(_postId, text);
                    }
                });
            }
            if (_quoteReply)
            {
                tasks[3] = Task.Run(async () =>
                {
                    for (var i = 0; i < _numberOfQuoteRepliesPerCookie; i++)
                    {
                        var text = _repliesReader.Read();

                        for (var j = 0; j < _quoteReplyUserTagsCount; j++)
                            text += ' ' + _userTagsReader.Read();

                        await apiClient.AddQuotedReply(_postId, text);
                    }
                });
            }
            if (_subscribeId != null)
                tasks[4] = apiClient.Subscribe(_subscribeId);

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex.Message);
            }
        }
    }
}
