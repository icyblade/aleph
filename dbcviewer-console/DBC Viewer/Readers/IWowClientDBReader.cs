﻿using System.Collections.Generic;
using System.IO;

namespace DBCViewer
{
    interface IWowClientDBReader
    {
        int RecordsCount { get; }
        int FieldsCount { get; }
        int RecordSize { get; }
        int StringTableSize { get; }

        List<int> index { get;}

        uint Build { get; }

        Dictionary<int, string> StringTable { get; }
        byte[] GetRowAsByteArray(int row);
        BinaryReader this[int row] { get; }
        

    }
}
