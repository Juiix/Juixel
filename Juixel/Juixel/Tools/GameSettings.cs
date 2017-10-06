using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Utilities.Net.Tools;

namespace Juixel.Tools
{
    public struct SavableData
    {
        public ValueTypes Type;
        public object Obj;
    }

    public class GameSettings
    {
        private static Dictionary<string, SavableData> _CurrentData = new Dictionary<string, SavableData>();

        public static void Load(string PathEx)
        {
            DirectoryPath = Path.Combine(ApplicationData, PathEx);
            FilePath = Path.Combine(DirectoryPath, File_Name);
            if (File.Exists(FilePath))
            {
                byte[] Data = File.ReadAllBytes(FilePath);
                try
                {
                    unpackData(Data);
                }
                catch
                {
                    Logger.Log("Loading GameSettings failed!", true, true);
                    File.Delete(FilePath);
                    _CurrentData = new Dictionary<string, SavableData>();
                }
            }
        }

        #region GetFunctions

        public static string GetString(string key, string def)
        {
            if (_CurrentData.ContainsKey(key))
                return _CurrentData[key].Obj as string;
            return def;
        }

        public static int GetInt(string key, int def)
        {
            if (_CurrentData.ContainsKey(key))
                return (int)_CurrentData[key].Obj;
            return def;
        }

        public static bool GetBool(string key, bool def)
        {
            if (_CurrentData.ContainsKey(key))
                return (bool)_CurrentData[key].Obj;
            return def;
        }

        #endregion

        #region Set And Save Functions

        public static void SetString(string key, string value)
        {
            _CurrentData[key] = new SavableData
            {
                Type = ValueTypes.String,
                Obj = value
            };
        }

        public static void SetInt(string key, int value)
        {
            _CurrentData[key] = new SavableData
            {
                Type = ValueTypes.Int,
                Obj = value
            };
        }

        public static void SetBool(string key, bool value)
        {
            _CurrentData[key] = new SavableData
            {
                Type = ValueTypes.Bool,
                Obj = value
            };
        }

        public static byte[] SaveString(string value)
        {
            List<byte> data = Encoding.UTF8.GetBytes(value).ToList();
            data.Insert(0, (byte)ValueTypes.String);
            return data.ToArray();
        }

        public static byte[] SaveInt(int value)
        {
            List<byte> data = BitConverter.GetBytes(value).ToList();
            data.Insert(0, (byte)ValueTypes.Int);
            return data.ToArray();
        }

        public static byte[] SaveBool(bool value)
        {
            return new byte[] { (byte)ValueTypes.Bool, (byte)(value ? 1 : 0) };
        }

        #endregion

        #region IOFunctions

        public static SavableData Convert(byte[] Data)
        {
            List<byte> d = Data.ToList();
            ValueTypes t = (ValueTypes)d[0];
            object obj = null;
            d.RemoveAt(0);
            switch (t)
            {
                case ValueTypes.String:
                    obj = Encoding.UTF8.GetString(d.ToArray());
                    break;
                case ValueTypes.Int:
                    obj = BitConverter.ToInt32(d.ToArray(), 0);
                    break;
                case ValueTypes.Bool:
                    obj = d[0] == 1;
                    break;
            }
            return new SavableData
            {
                Type = t,
                Obj = obj
            };
        }

        private static bool flushing = false;
        private static bool flushAgain = false;

        public static void flush()
        {
            if (!flushing)
            {
                flushing = true;
                if (!Directory.Exists(DirectoryPath))
                    Directory.CreateDirectory(DirectoryPath);

                File.WriteAllBytes(FilePath, convertedData());
                flushing = false;
                if (flushAgain)
                {
                    flushAgain = false;
                    flush();
                }
            }
            else
                flushAgain = true;
        }

        private static void unpackData(byte[] data)
        {
            using (var Stream = new MemoryStream(data))
            {
                NetworkReader r = new NetworkReader(Stream);
                int count = r.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    string key = r.ReadUTF32();
                    byte[] value = r.ReadBytes32();
                    _CurrentData[key] = Convert(value);
                }
            }
        }

        private static byte[] convertedData()
        {
            KeyValuePair<string, SavableData>[] datas = _CurrentData.ToArray();
            MemoryStream ms = new MemoryStream();
            NetworkWriter w = new NetworkWriter(ms);
            w.Write(datas.Length);
            foreach (var pair in datas)
            {
                w.WriteUTF32(pair.Key);
                byte[] bytes = null;
                switch (pair.Value.Type)
                {
                    case ValueTypes.String:
                        bytes = SaveString((string)pair.Value.Obj);
                        break;
                    case ValueTypes.Int:
                        bytes = SaveInt((int)pair.Value.Obj);
                        break;
                    case ValueTypes.Bool:
                        bytes = SaveBool((bool)pair.Value.Obj);
                        break;
                }
                w.WriteBytes32(bytes);
            }
            return ms.ToArray();
        }

        private const string File_Name = "GameSettings.bdata";

        private static string ApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static string DirectoryPath;

        private static string FilePath;

        #endregion

    }

    public enum ValueTypes : byte
    {
        String = 0,
        Int = 1,
        Bool = 2
    }
}
