using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace BotsCommon.Console
{
    internal delegate bool TryParseDelegate<T>(
        string str,
        [NotNullWhen(true)] out T? value,
        [NotNullWhen(true)] out string? name
    );

    public sealed record RequestOption<T>(
        string Name,
        string? Description,
        T Value
    );

    public sealed class ConsoleRequester
    {
        private readonly List<string> _errors = new();
        private readonly List<Step> _steps = new();
        private bool _firstRequest = true;

        private sealed class Step
        {
            public bool IsSectionEnter { get; set; }
            public bool IsSectionExit { get; set; }
            public string? Name { get; set; }
            public string? Value { get; set; }
        }

        public void AddError(string message)
        {
            _errors.Add(message);
        }

        public void EnterSection(string name)
        {
            _steps.Add(new Step
            {
                IsSectionEnter = true,
                Name = name
            });
        }

        public void ExitSection()
        {
            _steps.Add(new Step
            {
                IsSectionExit = true
            });
        }

        private T? RequestValue<T>(
            string name,
            string question,
            string? defaultValue,
            string? availableValues,
            bool supportNullValue,
            TryParseDelegate<T> tryParse
        )
        {
            var firstCursorTop = 0;

            if (_firstRequest)
            {
                _firstRequest = false;
            }
            else
            {
                for (var i = 0; i < System.Console.WindowHeight; i++)
                    System.Console.WriteLine();

                var top = Math.Max(0, System.Console.CursorTop - System.Console.WindowHeight);

                System.Console.SetCursorPosition(0, top);
            }

            var spaceCount = 0;

            foreach (var step in _steps)
            {
                if (step.IsSectionEnter)
                {
                    var prefix = spaceCount > 0 ?
                        new string(' ', (spaceCount - 1) * 4) + " +- " :
                        string.Empty;

                    ConsoleUtils.WriteColored(prefix + step.Name, foregroundColor: ConsoleColor.DarkGray);
                    System.Console.WriteLine();

                    spaceCount += 1;
                }
                else if (step.IsSectionExit)
                {
                    spaceCount -= 1;
                }
                else
                {
                    var prefix = spaceCount > 0 ?
                        new string(' ', (spaceCount - 1) * 4) + " +- " :
                        string.Empty;

                    ConsoleUtils.WriteColored(prefix, foregroundColor: ConsoleColor.DarkGray);

                    System.Console.WriteLine(
                        $"{step.Name}: {step.Value}"
                    );
                }
            }

            System.Console.WriteLine();
            System.Console.WriteLine(question);

            if (availableValues != null)
            {
                ConsoleUtils.WriteColored(availableValues, foregroundColor: ConsoleColor.DarkGray);
                System.Console.WriteLine();
            }

            if (supportNullValue)
            {
                ConsoleUtils.WriteColored("Leave field blank if you do not want to use this.", foregroundColor: ConsoleColor.DarkGray);
                System.Console.WriteLine();
            }

            ConsoleUtils.WriteColored("Press Esc to clear your input.", foregroundColor: ConsoleColor.DarkGray);
            System.Console.WriteLine();
            System.Console.WriteLine();

            if (_errors.Count > 0)
            {
                foreach (var error in _errors)
                {
                    ConsoleUtils.WriteColored(error, foregroundColor: ConsoleColor.DarkRed);
                    System.Console.WriteLine();
                }

                _errors.Clear();

                System.Console.WriteLine();
            }

            var defaultValueString = defaultValue?.ToString();

            var textLinesCount = System.Console.CursorTop - firstCursorTop;
            var bottomOffset = System.Console.WindowHeight - textLinesCount;

            for (var i = 0; i < bottomOffset - 1; i++)
                System.Console.WriteLine();

            System.Console.SetCursorPosition(0, textLinesCount);

            while (true)
            {
                System.Console.Write($"{name}{(supportNullValue ? " (Optional)" : string.Empty)}: ");

                var str = ConsoleUtils.ReadInput(defaultValueString).Value;

                defaultValueString = null;

                if (string.IsNullOrEmpty(str))
                {
                    if (supportNullValue == true)
                    {
                        _steps.RemoveAll(x =>
                            x.Name == name &&
                            !x.IsSectionEnter &&
                            !x.IsSectionExit
                        );

                        _steps.Add(new Step
                        {
                            Name = name,
                            Value = "<none>"
                        });

                        return default;
                    }

                    continue;
                }

                if (!tryParse(str, out var value, out var valueName))
                    continue;

                _steps.RemoveAll(x =>
                    x.Name == name &&
                    !x.IsSectionEnter &&
                    !x.IsSectionExit
                );

                _steps.Add(new Step
                {
                    Name = name,
                    Value = valueName
                });

                return value;
            }
        }

        public T? RequestOptionsValue<T>(
            string name,
            string question,
            T? defaultValue,
            bool supportNullValue,
            IEnumerable<RequestOption<T>> options
        )
        {
            return RequestValue(
                name,
                question,
                options.FirstOrDefault(x => x.Value?.Equals(defaultValue) == true)?.Name,
                "Available values:\n    " +
                string.Join(
                    "\n    ",
                    options.Select((x, i) =>
                    {
                        var result = $"{i + 1}: {x.Name}";

                        if (x.Description != null)
                            result += $" - {x.Description}";

                        return result;
                    })
                ),
                supportNullValue,
                (string str, [NotNullWhen(true)] out T? value, [NotNullWhen(true)] out string? name) =>
                {
                    var option = options.FirstOrDefault(
                        x => x.Name.Equals(str, StringComparison.OrdinalIgnoreCase)
                    );

                    if (option == null && int.TryParse(str, out var index))
                        option = options.Skip(index - 1).FirstOrDefault();

                    if (option != null)
                    {
                        value = option.Value!;
                        name = option.Name;
                        return true;
                    }

                    value = default;
                    name = null;
                    return false;
                }
            );
        }

        public string? RequestStringValue(
            string name,
            string question,
            string? defaultValue,
            string? availableValues,
            bool supportNullValue
        )
        {
            return RequestValue(
                name,
                question,
                defaultValue,
                availableValues,
                supportNullValue,
                (string str, [NotNullWhen(true)] out string? value, [NotNullWhen(true)] out string? name) =>
                {
                    value = str;
                    name = str;
                    return true;
                }
            );
        }

        public bool? RequestBoolValue(
            string name,
            string question,
            bool? defaultValue,
            bool supportNullValue
        )
        {
            return RequestValue(
                name,
                question,
                defaultValue.HasValue ? defaultValue.Value ? "yes" : "no" : null,
                "Available values: (y)es, (n)o",
                supportNullValue,
                (string str, [NotNullWhen(true)] out bool? value, [NotNullWhen(true)] out string? name) =>
                {
                    str = str.ToLower();

                    if (bool.TryParse(str, out var nonNullValue))
                    {
                        value = nonNullValue;
                        name = nonNullValue ? "yes" : "no";
                        return true;
                    }

                    if (str is "y" or "yes")
                    {
                        value = true;
                        name = "yes";
                        return true;
                    }

                    if (str is "n" or "no")
                    {
                        value = false;
                        name = "no";
                        return true;
                    }

                    value = null;
                    name = null;
                    return false;
                }
            );
        }

        public int? RequestIntValue(
            string name,
            string question,
            int? defaultValue,
            bool supportNullValue,
            int? min = null,
            int? max = null
        )
        {
            return RequestValue(
                name,
                question,
                defaultValue.HasValue ? defaultValue.Value.ToString() : null,
                min.HasValue || max.HasValue ?
                    (
                        $"{(min.HasValue ? "Minimum: " + min.Value : null)} " +
                        $"{(max.HasValue ? "Maximum: " + max.Value : null)}"
                    ).Trim() :
                    null,
                supportNullValue,
                (string str, [NotNullWhen(true)] out int? value, [NotNullWhen(true)] out string? name) =>
                {
                    if (int.TryParse(str, out var nonNullValue) &&
                        (!min.HasValue || nonNullValue >= min.Value) &&
                        (!max.HasValue || nonNullValue <= max.Value))
                    {
                        value = nonNullValue;
                        name = nonNullValue.ToString();
                        return true;
                    }

                    value = null;
                    name = null;
                    return false;
                }
            );
        }

        public long? RequestLongValue(
            string name,
            string question,
            long? defaultValue,
            bool supportNullValue,
            long? min = null,
            long? max = null
        )
        {
            return RequestValue(
                name,
                question,
                defaultValue.HasValue ? defaultValue.Value.ToString() : null,
                min.HasValue || max.HasValue ?
                    (
                        $"{(min.HasValue ? "Minimum: " + min.Value : null)} " +
                        $"{(max.HasValue ? "Maximum: " + max.Value : null)}"
                    ).Trim() :
                    null,
                supportNullValue,
                (string str, [NotNullWhen(true)] out long? value, [NotNullWhen(true)] out string? name) =>
                {
                    if (long.TryParse(str, out var nonNullValue) &&
                        (!min.HasValue || nonNullValue >= min.Value) &&
                        (!max.HasValue || nonNullValue <= max.Value))
                    {
                        value = nonNullValue;
                        name = nonNullValue.ToString();
                        return true;
                    }

                    value = null;
                    name = null;
                    return false;
                }
            );
        }
    }
}
