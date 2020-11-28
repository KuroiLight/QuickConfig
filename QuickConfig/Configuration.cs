using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace QuickConfig
{
    public sealed class Configuration
    {
        private readonly Dictionary<string, object> ConfigKeyValues;

        /// <summary>
        /// validates whether the value is correct or in bounds, may also transform the value if it isnt
        /// </summary>
        /// <typeparam name="T">the type of the value to validate</typeparam>
        /// <param name="value">the value to validate or transform</param>
        /// <returns>returns a (bool for whether the value is valid, true whether a transform was made or not if value is valid in the end;  the final value, should always return the value transformed or not if its valid)</returns>
        public delegate (bool, T?) ValidationMethod<T>(in T value);

        private object? cachedValue;
        private string? cachedKey;

        static Configuration()
        {
            Instance = new Configuration();
        }

        public static Configuration Instance { get; }

        private Configuration()
        {
            ConfigKeyValues = new Dictionary<string, object>();
        }

        public bool SetValue<T>(string key, T value, ValidationMethod<T> vmethod)
        {
            (bool valid, T? transormed) = vmethod(value);
            return valid && SetValue<T>(key, transormed);
        }

        public T? GetValue<T>(string key, ValidationMethod<T> vmethod)
        {
            T? gottenValue = GetValue<T>(key);
            if (gottenValue is not null) {
                (bool valid, T? transormed) = vmethod(gottenValue);
                return transormed;
            }
            return default;
        }

        public bool SetValue<T>(string key, T value)
        {
            if (value is null || key is null)
                return false;

            var keyname = value.GetType().ToString() + ':' + key;

            ConfigKeyValues[keyname] = (object)value;
            return true;
        }

        public T? GetValue<T>(string key)
        {
            if (key is null)
                return default(T);

            if (key == cachedKey && cachedValue is not null)
                return (T)cachedValue;

            var keyname = typeof(T).ToString() + ':' + key;
            if (!ConfigKeyValues.ContainsKey(keyname))
                return default;

            var value = ConfigKeyValues[keyname];

            (cachedKey, cachedValue) = (key, value);

            if (value is not null)
                return (T)value;
            return default(T);
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
            static (string s1, string s2) split(string s, char splitter)
            {
                var splitten = s.Split(splitter, StringSplitOptions.TrimEntries);
                return (splitten[0], splitten[1]);
            }

            static object? StringToType(string s, string type)
            {
                if (s is null || type is null)
                    return null;

                Type T = Type.GetType(type);
                if (T is null)
                    return null;

                TypeConverter tc = TypeDescriptor.GetConverter(T);
                object? converted = tc.ConvertFromString(s);
                if (converted is null)
                    return null;

                return converted;
            }

            if (!ValidFilename(filename))
                return false;

            ConfigKeyValues.Clear();

            try {
                using (StreamReader fs = new(filename)) {
                    while (!fs.EndOfStream) {
                        var line = fs.ReadLine();
                        if (line is null)
                            continue;

                        (string k, string v) = split(line, '=');
                        if (k is not null && v is not null) {
                            (string type, string key) = split(k, ':');
                            object? value = StringToType(v, type);
                            if (value is not null)
                                ConfigKeyValues.Add(k, value);
                        }
                    }
                }
                return true;
            } catch {
                return false;
            }
        }
    }
}
