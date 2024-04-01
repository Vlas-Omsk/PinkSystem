using System;

namespace BotsCommon.Console
{
    public sealed record ReadInputValue(
        string? Value,
        bool Cancelled
    );

    public static class ConsoleUtils
    {
        public static void WriteColored(
            string str,
            ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null
        )
        {
            var savedForegroundColor = System.Console.ForegroundColor;
            var savedBackgroundColor = System.Console.BackgroundColor;

            if (foregroundColor.HasValue)
                System.Console.ForegroundColor = foregroundColor.Value;

            if (backgroundColor.HasValue)
                System.Console.BackgroundColor = backgroundColor.Value;

            System.Console.Write(str);

            System.Console.ForegroundColor = savedForegroundColor;
            System.Console.BackgroundColor = savedBackgroundColor;
        }

        private sealed class ReadInputHandler
        {
            private readonly Func<ConsoleKeyInfo, ReadInputValue?> _inputHandler;
            private string _value;
            private int _cursorStartPos;
            private int _position;
            private int _offset;

            public ReadInputHandler(string? value, Func<ConsoleKeyInfo, ReadInputValue?> inputHandler)
            {
                _inputHandler = inputHandler;
                _cursorStartPos = System.Console.CursorLeft;
                _value = value ?? string.Empty;
                _position = _value.Length;

                UpdateValue();
            }

            private void UpdateValue()
            {
                _offset = _position - (System.Console.WindowWidth - 1 - _cursorStartPos);

                if (_offset < 0)
                    _offset = 0;

                var left = _value[_offset..];

                if (left.Length < System.Console.WindowWidth - 1 - _cursorStartPos)
                    left += new string(' ', System.Console.WindowWidth - 1 - _cursorStartPos - left.Length);

                if (left.Length >= System.Console.WindowWidth - 1 - _cursorStartPos)
                    left = left[..(System.Console.WindowWidth - 1 - _cursorStartPos)];

                System.Console.CursorLeft = _cursorStartPos;
                System.Console.Write(left.Replace('\t', ' '));
                System.Console.CursorLeft = _cursorStartPos + (_position - _offset);
            }

            public void RemovePrev()
            {
                if (_position == 0)
                    return;

                _position--;
                _value = _value[.._position] + _value[(_position + 1)..];

                UpdateValue();
            }

            public void RemoveNext()
            {
                if (_position == _value.Length)
                    return;

                _value = _value[.._position] + _value[(_position + 1)..];

                UpdateValue();
            }

            public void MoveCursorRight()
            {
                if (_position == _value.Length)
                    return;

                _position++;

                UpdateValue();
            }

            public void MoveCursorLeft()
            {
                if (_position == 0)
                    return;

                _position--;

                UpdateValue();
            }

            public void MoveCursorToHome()
            {
                _position = 0;

                UpdateValue();
            }

            public void MoveCursorToEnd()
            {
                _position = _value.Length;

                UpdateValue();
            }

            public void Clear()
            {
                _position = 0;
                _value = string.Empty;

                UpdateValue();
            }

            public void Add(ConsoleKeyInfo key)
            {
                var ch = key.KeyChar;

                if (!char.IsLetterOrDigit(ch))
                    return;

                if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    ch = char.ToUpper(ch);

                _position++;
                _value = _value[..(_position - 1)] + ch + _value[(_position - 1)..];

                UpdateValue();
            }

            public ReadInputValue Read()
            {
                while (true)
                {
                    var key = System.Console.ReadKey(intercept: true);

                    switch (key.Key)
                    {
                        case ConsoleKey.Backspace:
                            RemovePrev();
                            break;
                        case ConsoleKey.Delete:
                            RemoveNext();
                            break;
                        case ConsoleKey.RightArrow:
                            MoveCursorRight();
                            break;
                        case ConsoleKey.LeftArrow:
                            MoveCursorLeft();
                            break;
                        case ConsoleKey.Home:
                        case ConsoleKey.UpArrow:
                            MoveCursorToHome();
                            break;
                        case ConsoleKey.End:
                        case ConsoleKey.DownArrow:
                            MoveCursorToEnd();
                            break;
                        case ConsoleKey.Escape:
                            Clear();
                            break;
                        case ConsoleKey.Enter:
                            System.Console.WriteLine();
                            return new ReadInputValue(_value, false);
                        default:
                            var result = _inputHandler(key);

                            if (result != null)
                                return result;

                            if (key.Key == ConsoleKey.Z &&
                                key.Modifiers.HasFlag(ConsoleModifiers.Control))
                                return new ReadInputValue(null, true);

                            Add(key);
                            break;
                    }
                }
            }
        }

        public static ReadInputValue ReadInput(string? value, Func<ConsoleKeyInfo, ReadInputValue?> inputHandler)
        {
            return new ReadInputHandler(value, inputHandler).Read();
        }
    }
}