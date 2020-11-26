using System;
using System.Data.SqlTypes;
using QuickConfig;

namespace configtest
{
    class Program
    {
        static Configuration C = Configuration.Instance;
        static void Main(string[] args)
        {
            Console.WriteLine(C.SetValue<int>("setting1", 4546432, (v) => {
                if (20000 < v)
                    return (true, 20000);
                else if (1000 > v)
                    return (true, 1000);
                return (true, v);
            }));

            Console.WriteLine(C.SetValue<int>("setting4", 9999, (v) => {
                if (20000 < v)
                    return (true, 20000);
                else if (1000 > v)
                    return (true, 1000);
                return (true, v);
            }));

            Console.WriteLine(C.SetValue<int>("setting1", 555, (v) => {
                if (20000 < v)
                    return (true, 20000);
                else if (1000 > v)
                    return (true, 1000);
                return (true, v);
            }));

            Console.WriteLine(C.SetValue<bool>("settingb", true, (v) => {
                return (true, v);
            }));

            C.SaveToFile("config.ini");
            C.LoadFromFile("config.ini");
            Console.WriteLine("--");

            Console.WriteLine(C.GetValue<int>("setting1", (v) => {
                if (20000 < v)
                    return (true, 20000);
                else if (1000 > v)
                    return (true, 1000);
                return (true, v);
            }));

            Console.WriteLine(C.GetValue<int>("setting4", (v) => {
                if (20000 < v)
                    return (true, 20000);
                else if (1000 > v)
                    return (true, 1000);
                return (true, v);
            }));

            Console.WriteLine(C.GetValue<bool>("settingb", (v) => {
                return (true, v);
            }));


            Console.WriteLine("Hello World!");
        }
    }
}
