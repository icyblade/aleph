using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBCViewer
{
    class DB3Reader : IWowClientDBReader
    {
        private const int HeaderSize = 48;
        private const uint DB3FmtSig = 0x33424457;          // WDB3
     //   private const uint ADB4FmtSig = 0x34484357;         //WCH4
     //   private const uint ADB5FmtSig = 0x35484357;         //WCH5
        public int RecordsCount { get; private set; }
        public int FieldsCount { get; private set; }
        public int RecordSize { get; private set; }
        public int StringTableSize { get; private set; }

        public uint Build { get; private set; }
        public Dictionary<int, string> StringTable { get; private set; }

        public List<int> index { get; private set; }

        private byte[][] m_rows;

        public byte[] GetRowAsByteArray(int row)
        {
            return m_rows[row];
        }

        public BinaryReader this[int row]
        {
            get { return new BinaryReader(new MemoryStream(m_rows[row]), Encoding.UTF8); }
        }

        public DB3Reader(string fileName)
        {
            using (var reader = BinaryReaderExtensions.FromFile(fileName))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    throw new InvalidDataException(String.Format("File {0} is corrupted!", fileName));
                }

                uint signature = reader.ReadUInt32();

                if ( signature!= DB3FmtSig)
                {

                    throw new InvalidDataException(String.Format("File {0} isn't valid DBC file!", fileName));
                }

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();

                // WDB2 specific fields
                uint tableHash = reader.ReadUInt32();   // new field in WDB2
                Build = reader.ReadUInt32();       // new field in WDB2
                uint unk1 = reader.ReadUInt32();        // new field in WDB2

                int MinId = reader.ReadInt32();     // new field in WDB2
                int MaxId = reader.ReadInt32();     // new field in WDB2
                int locale = reader.ReadInt32();    // new field in WDB2
                int CopyTableSize = reader.ReadInt32();

                index = new List<int>();
                StringTable = new Dictionary<int, string>();

                List<byte[]> rows = new List<byte[]>();

                //  m_rows = new byte[RecordsCount + CopyTableSize/8][];

                List<int[]> pairlist = new List<int[]>();

                List<string> strlist = new List<string>();
                List<string> strl = new List<string>();

                Console.WriteLine(reader.BaseStream.Length);



                if (RecordsCount<=0)
                {
                    return;
                }

                Console.WriteLine("rsize:"+RecordSize);
                Console.WriteLine("rcount:" + RecordsCount);
                Console.WriteLine("csize:" + (6 * (MaxId-MinId+1) + HeaderSize));
                Console.WriteLine("minID:" + MinId);
                Console.WriteLine("maxID:" + MaxId);
                Console.WriteLine("CPSize:" + CopyTableSize);
                
                
                int foffset1 = reader.ReadInt32();

                Console.WriteLine("foffset1:" + foffset1);
                //   reader.BaseStream.Position = reader.BaseStream.Position - 4;

                int rlength1 = reader.ReadUInt16();

                Console.WriteLine("rlength1:" + rlength1);

                reader.BaseStream.Position = reader.BaseStream.Position - 6;

                
                bool isRawData = (foffset1 == (6 * (MaxId - MinId + 1) + HeaderSize));

                int stringTableStart = HeaderSize + RecordsCount * RecordSize;
                int stringTableEnd = stringTableStart + StringTableSize;


                Console.WriteLine("stStart:" + stringTableStart);
                Console.WriteLine("stEnd:" + stringTableEnd);

                bool hasIndex = stringTableEnd + CopyTableSize < reader.BaseStream.Length;

                Console.WriteLine(hasIndex);




                if (isRawData)
                {
                    while (reader.BaseStream.Position!=foffset1)
                    {
                        int[] pairs = new int[2];

                        pairs[0] = reader.ReadInt32();
                        pairs[1] = reader.ReadInt16();

                      //  Console.WriteLine(pairs[0]);
                      //  Console.WriteLine(pairs[1]);
                        pairlist.Add(pairs);
                      //  strl.Add(pairs[0].ToString()+","+pairs[1].ToString());
                    }

                  //  File.WriteAllLines("offsettable.txt",strl);


                    foreach (var p in pairlist)
                    {
                        if (p[0]>0)
                        {
                            reader.BaseStream.Position = p[0];

                            rows.Add(reader.ReadBytes(p[1]));
                        }



                    }

                    

                    m_rows = rows.ToArray();


                    int countc = 0;



                    // reader.BaseStream.Position = stringTableEnd;

                    reader.ReadUInt16();

                    while (reader.BaseStream.Position != reader.BaseStream.Length && hasIndex)
                    {

                        int id = reader.ReadInt32();

                        index.Add(id);

                        // lookup.Add(id,count);

                        countc++;
                        if (countc >= RecordsCount)
                        {
                            break;
                        }

                    }
                    

                    return;
                }

                for (int i = 0; i < RecordsCount; i++)
                {
                    rows.Add(reader.ReadBytes(RecordSize));
                   // m_rows[i] = reader.ReadBytes(RecordSize);
                }

                m_rows = rows.ToArray();

               // int stringTableStart = (int)reader.BaseStream.Position;

               // int stringTableEnd = (stringTableStart + StringTableSize);

               // Console.WriteLine("stableStart:"+stringTableStart);

              //  Console.WriteLine("stableEnd:" + (stringTableStart+StringTableSize).ToString());

              //  StringTable = new Dictionary<int, string>();

                while (reader.BaseStream.Position != stringTableEnd)
                {
                    int index = (int)reader.BaseStream.Position - stringTableStart;
                    StringTable[index] = reader.ReadStringNull();
                }

             //   index = new List<int>();

                Console.WriteLine(stringTableEnd);

                int count = 0;
                

                while (reader.BaseStream.Position != reader.BaseStream.Length && hasIndex) {

                    int id = reader.ReadInt32();

                    index.Add(id);

                   // lookup.Add(id,count);

                    count++;
                    if (count>=RecordsCount)
                    {
                        break;
                    }

                }

                long copyTablePos = stringTableEnd + (hasIndex ? 4 * RecordsCount : 0);

                Console.WriteLine("cpPos:"+copyTablePos);



                if (copyTablePos != reader.BaseStream.Length && CopyTableSize != 0)
                {
                    reader.BaseStream.Position = copyTablePos;

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        int id = reader.ReadInt32();
                        int idcopy = reader.ReadInt32();

                        strlist.Add(id.ToString()+","+idcopy.ToString());

                       //  Console.WriteLine("ID:"+id);
                       //  Console.WriteLine("IDcopy:"+idcopy);

                      //  Console.WriteLine(index.IndexOf(idcopy));

                        if (index.IndexOf(idcopy)>0)
                        {
                            // m_rows[count] = m_rows[index.IndexOf(idcopy)];

                            rows.Add(rows[index.IndexOf(idcopy)]);

                            index.Add(id);

                            count++;

                            RecordsCount++;
                        }
                        /*else
                        {
                            m_rows[count] = new byte[RecordSize];

                            Array.Copy(BitConverter.GetBytes(id), m_rows[count], 4);

                            index.Add(id);

                            count++;

                            RecordsCount++;
                        }
                        */



                      /*  byte[] copyRow = Lookup[idcopy];
                        byte[] newRow = new byte[copyRow.Length];
                        Array.Copy(copyRow, newRow, newRow.Length);
                        Array.Copy(BitConverter.GetBytes(id), newRow, 4);

                        Lookup.Add(id, newRow);*/
                    }

                    m_rows = rows.ToArray();

                }

            }
        }
    }
}
