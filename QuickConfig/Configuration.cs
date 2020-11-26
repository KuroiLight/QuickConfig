using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuickConfig
{
    public sealed class Configuration
    {
        private readonly Dictionary<string, string> ConfigKeyValues;

        static Configuration()
        {
            Instance = new Configuration();
        }

        public static Configuration Instance { get; }

        private Configuration()
        {
            ConfigKeyValues = new Dictionary<string, string>();
        }

        public bool SetValue<T>(string Key, T Value, Func<T, (bool, T)>? ValidateValue = null)
        {
            (bool valid, T? validatedValue) = (ValidateValue is not null) ? ValidateValue(Value) : (true, Value);
            if (valid && validatedValue is not null) {
                ConfigKeyValues[Key] = validatedValue.ToString();
            }
            return valid;
        }

        public (bool, T?) GetValue<T>(string Key, Func<T, (bool, T)>? ValidateValue = null)
        {
            var value = ConfigKeyValues[Key];
            if (value is null)
                return (false, default(T));

            var targetType = typeof(T);
            if (targetType is null)
                return (false, default(T));

            var parseMethod = targetType.GetMethod("TryParse", new Type[] { typeof(string), typeof(T).MakeByRefType() });
            if (parseMethod is null)
                return (false, default(T));

            var parseParams = new object[2];
            parseParams[0] = value;
            var _invocationResult = parseMethod.Invoke(targetType, parseParams);
            if (_invocationResult is bool parsed && parsed && parseParams[1] is not null) {
                if (ValidateValue is not null)
                    return ValidateValue((T)parseParams[1]);
                else
                    return (true, (T)parseParams[1]);
            }

            return (false, default(T));
        }

        private static bool ValidFilename(string filename)
        {
            return filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
        }

        public bool SaveToFile(string filename)
        {
            if (!ValidFilename(filename))
                return false;

            if (ConfigKeyValues.Count == 0)
                return false;

            try {
                using (StreamWriter sw = new(filename, false)) {
                    StringBuilder sb = new();
                    foreach (var entry in ConfigKeyValues) {
                        sb.Append(entry.Key);
                        sb.Append('=');
                        sb.Append(entry.Value);

                        sw.WriteLine(sb.ToString());
                        sb.Clear();
                    }
                }
                return true;
            } catch {
                return false;
            }
        }

        public bool LoadFromFile(string filename)
        {
            if (!ValidFilename(filename))
                return false;

            ConfigKeyValues.Clear();

            try {
                using (StreamReader fs = new(filename)) {
                    while (!fs.EndOfStream) {
                        var line = fs.ReadLine();
                        if (line is null)
                            continue;
                        var split = line.Split("=", StringSplitOptions.TrimEntries);
                        (string k, string v) = (split[0], split[1]);
                        if (k is not null && v is not null)
                            ConfigKeyValues.Add(k, v);
                    }
                }
                return true;
            } catch {
                return false;
            }
        }
    }
}
