using System;
using BotsCommon.IO;
using PinkLogging;
using PinkNet;

namespace BotsCommon.Bots
{
    public sealed class GenericBot : IBot
    {
        private readonly Task[] _tasks;
        private readonly IDataReader<Proxy> _proxiesReader;
        private readonly IDataReader<string> _userAgentsReader;
        private readonly IDataReader<string> _cookiesPathsReader;
        private readonly string _postId;
        private readonly bool _like;
        private readonly bool _repost;
        private readonly bool _reply;
        private readonly IDataReader<string> _repliesReader;
        private readonly bool _quoteReply;
        private readonly IDataReader<string> _userTagsReader;
        private readonly int _quoteReplyUserTagsCount;
        private readonly IProgress<string> _progress;
        private readonly ILogger _logger;
        private readonly string _subscribeId;
        private readonly int _numberOfRepliesPerCookie;
        private readonly int _numberOfQuoteRepliesPerCookie;
        private readonly IApiAdapterFactory _apiAdapterFactory;

        public GenericBot(
            int threadsCount,
            IDataReader<Proxy> proxiesReader,
            IDataReader<string> userAgentsReader,
            IDataReader<string> cookiesPathsReader,
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
            IProgress<string> progress,
            ILogger logger,
            IApiAdapterFactory apiAdapterFactory
        )
        {
            _tasks = new Task[threadsCount];
            _proxiesReader = proxiesReader;
            _userAgentsReader = userAgentsReader;
            _cookiesPathsReader = cookiesPathsReader;
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
            _progress = progress;
            _logger = logger;
            _apiAdapterFactory = apiAdapterFactory;
        }

        public Task StartAsync()
        {
            return Task.Run(() =>
            {
                string cookiePath;

                while ((cookiePath = _cookiesPathsReader.Read()) != null)
                {
                    var freeTaskIndex = Array.FindIndex(_tasks, x => x?.IsCompleted ?? true);

                    _tasks[freeTaskIndex] = HandleAsync(cookiePath);

                    UpdateProgress();

                    if (_tasks.All(x => x != null))
                    {
                        var task = _tasks[Task.WaitAny(_tasks)];
                        if (task.Exception != null)
                            break;
                    }
                }

                Task.WaitAll(_tasks);
            });
        }

        private Task HandleAsync(string cookiePath)
        {
            return Task.Run(() =>
            {
                using var reader = new StreamLinesDataReader(new StreamReader(cookiePath));
                var cookies = new NetscapeCookieReader(reader);

                var apiAdapter = _apiAdapterFactory.Create(cookies, _proxiesReader?.Read(), _userAgentsReader.Read());
                var tasks = Enumerable.Repeat(Task.CompletedTask, 5).ToArray();

                if (_like)
                    tasks[0] = apiAdapter.AddLike(_postId);
                if (_repost)
                    tasks[1] = apiAdapter.CreateRepost(_postId);
                if (_reply)
                {
                    tasks[2] = Task.Run(async () =>
                    {
                        for (var i = 0; i < _numberOfRepliesPerCookie; i++)
                        {
                            var text = _repliesReader.Read();

                            await apiAdapter.AddReply(_postId, text);
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

                            await apiAdapter.AddQuotedReply(_postId, text);
                        }
                    });
                }
                if (_subscribeId != null)
                    tasks[4] = apiAdapter.Subscribe(_subscribeId);

                try
                {
                    Task.WaitAll(tasks);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex.Message);
                }
            });
        }

        private void UpdateProgress()
        {
            var message =
                $"{_cookiesPathsReader.GetProgress():0.00}% ({_cookiesPathsReader.Index} / {_cookiesPathsReader.Length}) " +
                $"UserAgent: {_userAgentsReader.Index} / {_userAgentsReader.Length}";

            if (_proxiesReader != null)
                message += $" Proxy: {_proxiesReader.Index} / {_proxiesReader.Length}";

            if (_repliesReader != null)
                message += $" Comment: {_repliesReader.Index} / {_repliesReader.Length}";

            _logger.Info("Progress: " + message);
            _progress.Report(message);
        }
    }
}
