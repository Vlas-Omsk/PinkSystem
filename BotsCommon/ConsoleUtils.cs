using System.Diagnostics.CodeAnalysis;

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
                string.Join(", ", Enum.GetValues<T>().Select(x => $"{x} ({x.GetHashCode()})")),
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

        public static string ReadInput(string? value)
        {
            var length = 0;

            if (value != null)
            {
                length = value.Length;

                Console.Write(value);
            }
            else
            {
                value = string.Empty;
            }

            var position = length;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        {
                            if (position == 0)
                                break;

                            position--;
                            length--;

                            var leftStr = value[(position + 1)..];

                            value = value[..position] + leftStr;

                            var left = --Console.CursorLeft;

                            Console.Write(leftStr + " ");
                            Console.CursorLeft = left;
                        }
                        break;
                    case ConsoleKey.Delete:
                        {
                            if (position == length)
                                break;

                            length--;

                            var leftStr = value[(position + 1)..];

                            value = value[..position] + leftStr;

                            var left = Console.CursorLeft;

                            Console.Write(leftStr + " ");
                            Console.CursorLeft = left;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        {
                            if (position == length)
                                break;

                            position++;
                            Console.CursorLeft++;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        {
                            if (position == 0)
                                break;

                            position--;
                            Console.CursorLeft--;
                        }
                        break;
                    case ConsoleKey.Home:
                    case ConsoleKey.UpArrow:
                        {
                            Console.CursorLeft -= position;
                            position = 0;
                        }
                        break;
                    case ConsoleKey.End:
                    case ConsoleKey.DownArrow:
                        {
                            Console.CursorLeft += length - position;
                            position = length;
                        }
                        break;
                    case ConsoleKey.Escape:
                        {
                            Console.CursorLeft -= position;
                            position = 0;
                            length = 0;

                            var left = Console.CursorLeft + 1;

                            Console.Write(new string(' ', value.Length));
                            Console.CursorLeft = left;

                            value = string.Empty;
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return value;
                    default:
                        {
                            var ch = key.KeyChar;

                            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                                ch = char.ToUpper(ch);

                            position++;
                            length++;

                            var leftStr = ch + value[(position - 1)..];

                            value = value[..(position - 1)] + leftStr;

                            var left = Console.CursorLeft + 1;

                            Console.Write(leftStr);
                            Console.CursorLeft = left;
                        }
                        break;
                }
            }
        }
    }
}