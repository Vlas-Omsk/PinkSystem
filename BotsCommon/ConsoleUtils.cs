using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#nullable enable

namespace BotsCommon
{
    internal delegate bool TryParseDelegate<T>(string str, [NotNullWhen(true)] out T? value);

    public static class ConsoleUtils
    {
        public static void WriteColored(string str, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
        {
            var savedForegroundColor = Console.ForegroundColor;
            var savedBackgroundColor = Console.BackgroundColor;

            if (foregroundColor.HasValue)
                Console.ForegroundColor = foregroundColor.Value;

            if (backgroundColor.HasValue)
                Console.BackgroundColor = backgroundColor.Value;

            Console.Write(str);

            Console.ForegroundColor = savedForegroundColor;
            Console.BackgroundColor = savedBackgroundColor;
        }

        private static T? RequestValue<T>(
            string name,
            string question,
            string? defaultValue,
            string? availableValues,
            bool supportNullValue,
            TryParseDelegate<T> tryParse
        )
        {
            Console.WriteLine();
            Console.WriteLine(question);

            if (availableValues != null)
                Console.WriteLine($"Available values: {availableValues}");

            if (supportNullValue)
                Console.WriteLine("Leave field blank if you do not want to use this.");

            Console.WriteLine("Press Esc to clear your input.");
            Console.WriteLine();

            var defaultValueString = defaultValue?.ToString();

            while (true)
            {
                Console.Write($"{name}{(supportNullValue ? " (Optional)" : string.Empty)}: ");

                var str = ReadInput(defaultValueString);

                defaultValueString = null;

                if (string.IsNullOrEmpty(str))
                {
                    if (supportNullValue == true)
                        return default;

                    continue;
                }

                if (!tryParse(str, out var enumValue))
                    continue;

                return enumValue;
            }
        }

        public static T? RequestEnumValue<T>(string name, string question, T? defaultValue, bool supportNullValue) where T : struct, Enum
        {
            return RequestValue(
                name,
                question,
                defaultValue?.ToString(),
                "\r\n    " + string.Join("\r\n    ", Enum.GetValues<T>().Select(x =>
                {
                    var name = x.ToString();
                    var result = name;

                    var descriptionAttribute = typeof(T)
                        .GetRuntimeFields()
                        .First(x => x.Name == name)
                        .GetCustomAttribute<DescriptionAttribute>();

                    if (descriptionAttribute != null)
                        result += " - " + descriptionAttribute.Description;

                    return $"{result} ({x.GetHashCode()})";
                })),
                supportNullValue,
                (string str, [NotNullWhen(true)] out T? value) =>
                {
                    if (Enum.TryParse<T>(str, true, out var nonNullValue))
                    {
                        value = nonNullValue;
                        return true;
                    }

                    value = null;
                    return false;
                }
            );
        }

        public static string? RequestStringValue(string name, string question, string? defaultValue, bool supportNullValue)
        {
            return RequestValue(
                name,
                question,
                defaultValue,
                null,
                supportNullValue,
                (string str, [NotNullWhen(true)] out string? value) =>
                {
                    value = str;
                    return true;
                }
            );
        }

        public static bool? RequestBoolValue(string name, string question, bool? defaultValue, bool supportNullValue)
        {
            return RequestValue(
                name,
                question,
                defaultValue.HasValue ? (defaultValue.Value ? "yes" : "no") : null,
                "yes, no",
                supportNullValue,
                (string str, [NotNullWhen(true)] out bool? value) =>
                {
                    str = str.ToLower();

                    if (bool.TryParse(str, out var nonNullValue))
                    {
                        value = nonNullValue;
                        return true;
                    }

                    if (str is "y" or "yes")
                    {
                        value = true;
                        return true;
                    }

                    if (str is "n" or "no")
                    {
                        value = false;
                        return true;
                    }

                    value = null;
                    return false;
                }
            );
        }

        public static int? RequestIntValue(string name, string question, int? defaultValue, bool supportNullValue)
        {
            return RequestValue<int?>(
                name,
                question,
                defaultValue.HasValue ? defaultValue.Value.ToString() : null,
                null,
                supportNullValue,
                (string str, [NotNullWhen(true)] out int? value) =>
                {
                    if (int.TryParse(str, out var nonNullValue))
                    {
                        value = nonNullValue;
                        return true;
                    }

                    value = null;
                    return false;
                }
            );
        }

        private sealed class ReadInputHandler
        {
            private string _value;
            private int _cursorStartPos;
            private int _position;
            private int _offset;

            public ReadInputHandler(string? value)
            {
                _cursorStartPos = Console.CursorLeft;
                _value = value ?? string.Empty;
                _position = _value.Length;

                UpdateValue();
            }

            private void UpdateValue()
            {
                _offset = _position - (Console.WindowWidth - 1 - _cursorStartPos);

                if (_offset < 0)
                    _offset = 0;

                var left = _value[_offset..];

                if (left.Length < Console.WindowWidth - 1 - _cursorStartPos)
                    left += new string(' ', (Console.WindowWidth - 1 - _cursorStartPos) - left.Length);

                if (left.Length >= Console.WindowWidth - 1 - _cursorStartPos)
                    left = left[..(Console.WindowWidth - 1 - _cursorStartPos)];

                Console.CursorLeft = _cursorStartPos;
                Console.Write(left.Replace('\t', ' '));
                Console.CursorLeft = _cursorStartPos + (_position - _offset);
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

                if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    ch = char.ToUpper(ch);

                _position++;
                _value = _value[..(_position - 1)] + ch + _value[(_position - 1)..];

                UpdateValue();
            }

            public string Read()
            {
                while (true)
                {
                    var key = Console.ReadKey(intercept: true);

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
                            Console.WriteLine();
                            return _value;
                        default:
                            Add(key);
                            break;
                    }
                }
            }
        }

        public static string ReadInput(string? value)
        {
            return new ReadInputHandler(value).Read();
        }
    }
}