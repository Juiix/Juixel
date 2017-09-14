using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Crypto
{
    public class RC4Key
    {
        public byte[] Bytes;

        public RC4Key(byte[] Bytes)
        {
            this.Bytes = Bytes;
        }

        public RC4Key(string Key)
        {
            Bytes = Encoding.UTF8.GetBytes(Key);
        }

        public static implicit operator RC4Key(string Key) => new RC4Key(Key);
        public static implicit operator RC4Key(byte[] Key) => new RC4Key(Key);
    }

    public class RC4
    {
        private readonly byte[] Key;

        public RC4(RC4Key Key)
        {
            this.Key = Key.Bytes;
        }

        public void Process(byte[] Buffer, int Offset, int Length)
        {
            if (Key == null)
                return;
            if (Offset > Buffer.Length)
                return;
            if (Buffer.Length - Offset < Length)
                Length = Buffer.Length - Offset;
            if (Length == 0)
                return;

            int a, i, j, k, tmp;
            int[] key, box;

            key = new int[256];
            box = new int[256];

            for (i = 0; i < 256; i++)
            {
                key[i] = Key[i % Key.Length];
                box[i] = i;
            }
            for (j = i = 0; i < 256; i++)
            {
                j = (j + box[i] + key[i]) % 256;
                tmp = box[i];
                box[i] = box[j];
                box[j] = tmp;
            }
            for (a = j = i = 0; i < Buffer.Length; i++)
            {
                a++;
                a %= 256;
                j += box[a];
                j %= 256;
                tmp = box[a];
                box[a] = box[j];
                box[j] = tmp;
                k = box[((box[a] + box[j]) % 256)];
                if (i > Offset)
                    Buffer[i] = (byte)(Buffer[i] ^ k);
            }
        }
    }
}
