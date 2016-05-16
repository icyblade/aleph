using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBCViewer
{
    class WDBReader : IWowClientDBReader
    {
        private const int HeaderSize = 24;
        private uint[] WDBSigs = new uint[]
        {
            0x574D4F42, // creaturecache.wdb
            0x57474F42, // gameobjectcache.wdb
            0x57494442, // itemcache.wdb
            0x574E4442, // itemnamecache.wdb
            0x57495458, // itemtextcache.wdb
            0x574E5043, // npccache.wdb
            0x57505458, // pagetextcache.wdb
            0x57515354, // questcache.wdb
            0x5752444E  // wowcache.wdb
        };

        public int RecordsCount { get; private set; }
        public int FieldsCount { get; private set; }
        public int RecordSize { get; private set; }
        public int StringTableSize { get; private set; }

        public List<int> index { get; private set; }

        public uint Build { get; private set; }

        public Dictionary<int, string> StringTable { get; private set; }

        // private Dictionary<int, byte[]> m_rows;

        private List<byte[]> m_rows;
        

        public byte[] GetRowAsByteArray(int row)
        {
            return m_rows[row];
        }

        public BinaryReader this[int row]
        {
            get { return new BinaryReader(new MemoryStream(m_rows[row]), Encoding.UTF8); }
        }

        public WDBReader(string fileName)
        {
            using (var reader = BinaryReaderExtensions.FromFile(fileName))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    throw new InvalidDataException(String.Format("File {0} is corrupted!", fileName));
                }

                var signature = reader.ReadUInt32();

                if (!WDBSigs.Contains(signature))
                {
                    throw new InvalidDataException(String.Format("File {0} isn't valid WDB file!", fileName));
                }

                Build= reader.ReadUInt32();
                uint locale = reader.ReadUInt32();
                var unk1 = reader.ReadInt32();

             //   Console.WriteLine(unk1);

                var unk2 = reader.ReadInt32();

            //    Console.WriteLine(unk2);

               // Console.WriteLine("-----------------");

                var version = reader.ReadInt32();
                index = new List<int>();
                //m_rows = new Dictionary<int, byte[]>();
                m_rows = new List<byte[]>();

                StringTable = new Dictionary<int, string>();
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var entry = reader.ReadInt32();
                    var size = reader.ReadInt32();
                    if (entry == 0 && size == 0 && reader.BaseStream.Position == reader.BaseStream.Length)
                        break;
                    var row = new byte[0]
                        .Concat(BitConverter.GetBytes(entry))
                        .Concat(reader.ReadBytes(size))
                        .ToArray();
                    //  m_rows.Add(entry, row);
                    m_rows.Add(row);
                }

                RecordsCount = m_rows.Count;
                

               /* foreach (var r in m_rows)
                {

                    Console.WriteLine(r.Value.Length);
                }

            */
               /* RecordSize = 0;
                FieldsCount = 0;
                StringTableSize = 0;*/

            }
        }
    }
}
