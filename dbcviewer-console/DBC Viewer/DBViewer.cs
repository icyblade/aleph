using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.IO;
using System.Xml;

namespace DBCViewer
{
    class DBViewer
    {

        private PluginInterface.DataTable m_dataTable;
        private IWowClientDBReader m_dbreader;
        private XmlDocument m_definitions;
        private XmlNodeList m_fields;
        private XmlElement m_definition;        // definition for current file
        private string m_dbcName;               // file name without extension
        private string m_dbcFile;               // path to current file
        private DateTime m_startTime;



        private XmlElement Definition { get { return m_definition; } }
        private string DBCName { get { return m_dbcName; } }
        //public int DefinitionIndex { get { return m_selector != null ? m_selector.DefinitionIndex : 0; } }
        private string DBCFile { get { return m_dbcFile; } }


        public void Export(string fileName)
        {



                StringBuilder sb = new StringBuilder();


                try
                {

                        AutoLoadFile(fileName);


                }
                catch (Exception ex)
                {

                    sb.AppendLine(fileName);
                    sb.AppendLine(ex.ToString());
                }

                using (FileStream fs =new FileStream("debug.log",FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(sb.ToString());
                    }
                }
            
        }

        private void LoadDefinitions()
        {
            m_definitions = new XmlDocument();
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            m_definitions.Load("dbclayout.xml");
            
        }

        private XmlElement AutoGetDefinition()
        {
            XmlNodeList definitions = m_definitions["DBFilesClient"].GetElementsByTagName(m_dbcName);

            Console.WriteLine(definitions);

            if (definitions.Count == 0)
            {
                definitions = m_definitions["DBFilesClient"].GetElementsByTagName(Path.GetFileName(m_dbcFile));
            }

            if (definitions.Count == 0)
            {
                var msg = String.Format(CultureInfo.InvariantCulture, "{0} missing definition!", m_dbcName);
                return null;
            }
            else if (definitions.Count == 1)
            {
                return ((XmlElement)definitions[0]);
            }
            else
            {
                int MaxIndex = definitions.Count - 1;

                return ((XmlElement)definitions[MaxIndex]);
            }
        }


        private void AutoLoadFile(string file)
        {
            m_dbcFile = file;
            

            m_dbcName = Path.GetFileNameWithoutExtension(file);

            LoadDefinitions(); // reload in case of modification

            m_definition = AutoGetDefinition();

            if (m_definition == null)
            {
                InitDefinitions();
                LoadDefinitions();
                m_definition = AutoGetDefinition();
            }
            

            m_startTime = DateTime.Now;
            //backgroundWorker1.RunWorkerAsync(file);
            ReadDB(file);
        }

        IWowClientDBReader reader;

        private void InitDefinitions()
        {
            var m_name = DBCName;

            var file = DBCFile;

            var ext = Path.GetExtension(file).ToUpperInvariant();


            if (ext == ".DBC")
                reader = new DBCReader(file);
            else if (ext == ".DB2")
            {
                try
                {
                    reader = new DB2Reader(file);
                }
                catch
                {
                    try
                    {

                        reader = new DB3Reader(file);
                    }
                    catch
                    {
                        try
                        {

                            reader = new DB4Reader(file);
                        }
                        catch
                        {
                            reader = new DB5Reader(file);
                        }
                    }
                }
            }
            else if (ext == ".ADB")
            {
                try
                {
                    reader = new ADBReader(file);
                }
                catch
                {
                    try
                    {

                        reader = new DB3Reader(file);
                    }
                    catch
                    {
                        reader = new ADB5Reader(file);
                    }
                }
            }
            else if (ext == ".WDB")
            {
                reader = new WDBReader(file);
            }



            XmlElement def = Definition;

            if (def == null)
            {

                def = CreateDefaultDefinition();
                if (def == null)
                {
                    throw new Exception((String.Format("Can't create default definitions for {0}", m_name)));
                }
            }

            this.m_definition = def;
           
            XmlDocument doc = new XmlDocument();

            string docPath = "dbclayout.xml";
            doc.Load(docPath);
            
            var newNode = doc.ImportNode(def, true);
            doc["DBFilesClient"].AppendChild(newNode);
            doc.Save(docPath);
        }

        private struct fType
        {

            public string typeName;
            public int isArray;

        }

        IWowClientDBReader dbreader;

        private XmlElement CreateDefaultDefinition()
        {


            if (reader is DB3Reader)
            {


                if (reader.RecordSize / reader.FieldsCount == 4)
                {
                    var doc = new XmlDocument();

                    XmlElement newnode = doc.CreateElement(DBCName);
                    newnode.SetAttributeNode("build", "").Value = reader.Build.ToString();

                    for (int i = 0; i < reader.FieldsCount; ++i)
                    {


                        XmlElement ele = doc.CreateElement("field");
                        ele.SetAttributeNode("type", "").Value = "int";
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}", i);
                        newnode.AppendChild(ele);
                    }
                    
                    return newnode;

                }
                else
                {
                    var doc = new XmlDocument();

                    XmlElement newnode = doc.CreateElement(DBCName);
                    newnode.SetAttributeNode("build", "").Value = reader.Build.ToString();

                    for (int i = 0; i < reader.FieldsCount; ++i)
                    {
                        /*if (i == 0)
                        {
                            XmlElement index = doc.CreateElement("index");
                            XmlNode primary = index.AppendChild(doc.CreateElement("primary"));
                            primary.InnerText = "field0";
                            newnode.AppendChild(index);
                        }*/

                        XmlElement ele = doc.CreateElement("field");
                        ele.SetAttributeNode("type", "").Value = "ushort";
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}", i);
                        newnode.AppendChild(ele);
                    }
                    
                    return newnode;
                }

            }
            else if (reader is DB5Reader)
            {
                DB5Reader db5reader = (DB5Reader)reader;

                var doc = new XmlDocument();

                XmlElement newnode = doc.CreateElement(DBCName);
                newnode.SetAttributeNode("build", "").Value = reader.Build.ToString();

                XmlElement tmpnode = doc.CreateElement(DBCName);

                List<fType> types = new List<fType>();


                for (int i = 0; i < db5reader.typeList.Count; i++)
                {

                    if (i < db5reader.typeList.Count - 1 && ((32 - db5reader.typeList[i][0]) / 8) != db5reader.typeList[i + 1][1] - db5reader.typeList[i][1])
                    {
                        int count = (db5reader.typeList[i + 1][1] - db5reader.typeList[i][1]) / ((32 - db5reader.typeList[i][0]) / 8);


                        fType ftype1;
                        ftype1.typeName = typeCov(((32 - db5reader.typeList[i][0]) / 8));
                        ftype1.isArray = 1;

                        types.Add(ftype1);

                        for (int j = 0; j < count - 1; j++)
                        {
                            fType ftype;
                            ftype.typeName = typeCov(((32 - db5reader.typeList[i][0]) / 8));
                            ftype.isArray = 2;

                            types.Add(ftype);
                        }

                    }
                    else
                    {
                        fType ftype;
                        ftype.typeName = typeCov(((32 - db5reader.typeList[i][0]) / 8));
                        ftype.isArray = 0;
                        types.Add(ftype);
                    }


                }

                int ar = 0;

                int temp = 0;


                for (int i = 0; i < types.Count; i++)
                {
                    XmlElement ele = doc.CreateElement("field");



                    ele.SetAttributeNode("type", "").Value = types[i].typeName;

                    if (types[i].isArray == 1)
                    {

                        ar = 1;
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}_{1}", i, ar);
                        temp = i;
                    }
                    else if (types[i].isArray == 2)
                    {
                        ar++;
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}_{1}", temp, ar);

                    }
                    else {

                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}", i);
                        ar = 0;
                        temp = i;
                    }

                    tmpnode.AppendChild(ele);
                    // i++;
                }

                try
                {
                    dbreader = DBReaderFactory.GetReader(DBCFile);

                }
                catch (Exception ex)
                {

                }

                DataTable dt = new DataTable();

                dt = CreateColumns(dt, tmpnode.ChildNodes);

                for (int i = 0; i < reader.RecordsCount; i++)
                {
                    DataRow dataRow = dt.NewRow();

                    using (BinaryReader br = dbreader[i])
                    {
                        for (int j = 0; j < types.Count; j++)
                        {


                            switch (types[j].typeName)
                            {
                                case "long":
                                    dataRow[j] = br.ReadInt64();
                                    break;
                                case "ulong":
                                    dataRow[j] = br.ReadUInt64();
                                    break;
                                case "int":
                                    dataRow[j] = br.ReadInt32();
                                    break;
                                case "uint":
                                    dataRow[j] = br.ReadUInt32();
                                    break;
                                case "short":
                                    dataRow[j] = br.ReadInt16();
                                    break;
                                case "ushort":
                                    dataRow[j] = br.ReadUInt16();
                                    break;
                                case "sbyte":
                                    dataRow[j] = br.ReadSByte();
                                    break;
                                case "byte":
                                    dataRow[j] = br.ReadByte();
                                    break;
                                case "float":
                                    dataRow[j] = br.ReadSingle();
                                    break;
                                case "double":
                                    dataRow[j] = br.ReadDouble();
                                    break;
                                case "index":
                                    dataRow[j] = m_dbreader.index[i];
                                    break;
                                case "int3":
                                    byte[] bytes = br.ReadBytes(3);

                                    byte[] buffer = new byte[4];

                                    Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);

                                    dataRow[j] = BitConverter.ToInt32(buffer, 0);

                                    break;

                                case "string":
                                    if (m_dbreader is WDBReader)
                                        dataRow[j] = br.ReadStringNull();
                                    else if (m_dbreader is STLReader)
                                    {
                                        int offset = br.ReadInt32();
                                        dataRow[j] = (m_dbreader as STLReader).ReadString(offset);
                                    }
                                    else
                                    {

                                        try
                                        {
                                            dataRow[j] = m_dbreader.StringTable[br.ReadInt32()];
                                        }
                                        catch
                                        {
                                            dataRow[j] = "Invalid string index!";

                                        }
                                    }
                                    break;
                                case "nstring":
                                    try
                                    {

                                        dataRow[j] = br.ReadStringNull();
                                    }
                                    catch
                                    {

                                        dataRow[j] = "Invalid string index!";
                                    }
                                    break;
                                case "cstring":
                                    dataRow[j] = br.ReadStringNumber();
                                    break;
                                default:
                                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Unknown field type {0}!", types[j]));
                            }
                        }
                    }
                    dt.Rows.Add(dataRow);
                }

                

                for (int i = 0; i < types.Count; i++)
                {
                    bool isString = true;

                    if (types[i].typeName.Equals("int"))
                    {
                        foreach(DataRow dr in dt.Rows)
                        {
                            if(!reader.StringTable.ContainsKey( (int) dr[i])){
                                isString = false;
                                break;
                            }
                        }
                        if (isString)
                        {
                            fType ft;
                            ft.typeName = "string";
                            ft.isArray = types[i].isArray;

                            types[i] = ft;
                        }
                    }
                }

                if (db5reader.index.Count > 0)
                {
                    XmlElement elei = doc.CreateElement("field");
                    elei.SetAttributeNode("type", "").Value = "index";
                    elei.SetAttributeNode("name", "").Value = String.Format("m_ID");
                    newnode.AppendChild(elei);
                }


                ar = 0;

                temp = 0;


                for (int i = 0; i < types.Count; i++)
                {
                    XmlElement ele = doc.CreateElement("field");



                    ele.SetAttributeNode("type", "").Value = types[i].typeName;

                    if (types[i].isArray == 1)
                    {

                        ar = 1;
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}_{1}", i, ar);
                        temp = i;
                    }
                    else if (types[i].isArray == 2)
                    {
                        ar++;
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}_{1}", temp, ar);

                    }
                    else {

                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}", i);
                        ar = 0;
                        temp = i;
                    }

                    newnode.AppendChild(ele);
                    // i++;
                }


                return newnode;
            }
            else
            {


                if (reader.RecordSize % reader.FieldsCount == 0) // only for files with 4 byte fields
                {
                    var doc = new XmlDocument();

                    XmlElement newnode = doc.CreateElement(DBCName);
                    newnode.SetAttributeNode("build", "").Value = reader.Build.ToString();

                    for (int i = 0; i < reader.FieldsCount; ++i)
                    {

                        XmlElement ele = doc.CreateElement("field");
                        ele.SetAttributeNode("type", "").Value = "int";
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}", i);
                        newnode.AppendChild(ele);
                    }
                    
                    return newnode;
                }
                else
                {
                    var doc = new XmlDocument();

                    XmlElement newnode = doc.CreateElement(DBCName);
                    newnode.SetAttributeNode("build", "").Value = reader.Build.ToString();

                    for (int i = 0; i < reader.FieldsCount; ++i)
                    {

                        XmlElement ele = doc.CreateElement("field");
                        ele.SetAttributeNode("type", "").Value = "ushort";
                        ele.SetAttributeNode("name", "").Value = String.Format("field{0}", i);
                        newnode.AppendChild(ele);
                    }
                    
                    return newnode;
                }

            }

            //return null;
        }

        private string typeCov(int type)
        {

            switch (type)
            {
                case 1:
                    return "byte";
                case 2:
                    return "ushort";
                case 3:
                    return "int3";
                case 4:
                    return "int";
                default:

                    return "int";

            }

        }

        private static int GetFieldsCount(XmlNodeList fields)
        {
            int count = 0;
            foreach (XmlElement field in fields)
            {
                switch (field.Attributes["type"].Value)
                {
                    case "long":
                    case "ulong":
                    case "double":
                        count += 2;
                        break;
                    default:
                        count++;
                        break;
                }
            }
            return count;
        }

        private void CreateIndexes()
        {
            XmlNodeList indexes = m_definition.GetElementsByTagName("index");
            var columns = new DataColumn[indexes.Count];
            var idx = 0;
            var columns_2 = new DataColumn[indexes.Count];
            var idx_2 = 0;
            foreach (XmlElement index in indexes)
            {

                if (index["primary"] != null)
                {
                    columns[idx++] = m_dataTable.Columns[index["primary"].InnerText];
                }
            }
            m_dataTable.PrimaryKey = columns;

            // Console.WriteLine(columns.Length);

            //Console.WriteLine("PKEYlen:"+m_dataTable.PrimaryKey.Length);

            foreach (XmlElement index in indexes)
            {

                if (index["NonUniqueIndex"] != null)
                {
                    columns_2[idx_2++] = m_dataTable.Columns[index["NonUniqueIndex"].InnerText];
                }
            }

            m_dataTable.NonUniqueIndexs = columns_2;

            //Console.WriteLine("nuiLen:"+m_dataTable.NonUniqueIndexs.Length);

        }

        private void CreateColumns()
        {
            foreach (XmlElement field in m_fields)
            {
                var colName = field.Attributes["name"].Value;

                switch (field.Attributes["type"].Value)
                {
                    case "long":
                        m_dataTable.Columns.Add(colName, typeof(long));
                        break;
                    case "ulong":
                        m_dataTable.Columns.Add(colName, typeof(ulong));
                        break;
                    case "int":
                    case "int3":
                        m_dataTable.Columns.Add(colName, typeof(int));
                        break;
                    case "uint":
                        m_dataTable.Columns.Add(colName, typeof(uint));
                        break;
                    case "short":
                        m_dataTable.Columns.Add(colName, typeof(short));
                        break;
                    case "ushort":
                        m_dataTable.Columns.Add(colName, typeof(ushort));
                        break;
                    case "sbyte":
                        m_dataTable.Columns.Add(colName, typeof(sbyte));
                        break;
                    case "byte":
                        m_dataTable.Columns.Add(colName, typeof(byte));
                        break;
                    case "float":
                        m_dataTable.Columns.Add(colName, typeof(float));
                        break;
                    case "double":
                        m_dataTable.Columns.Add(colName, typeof(double));
                        break;
                    case "index":
                        m_dataTable.Columns.Add(colName, typeof(int));
                        break;
                    case "string":
                    case "cstring":
                    case "nstring":
                        m_dataTable.Columns.Add(colName, typeof(string));
                        break;
                    default:
                        throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Unknown field type {0}!", field.Attributes["type"].Value));
                }
            }
        }

        private DataTable CreateColumns(DataTable m_dataTable, XmlNodeList m_fields)
        {
            foreach (XmlElement field in m_fields)
            {
                var colName = field.Attributes["name"].Value;

                switch (field.Attributes["type"].Value)
                {
                    case "long":
                        m_dataTable.Columns.Add(colName, typeof(long));
                        break;
                    case "ulong":
                        m_dataTable.Columns.Add(colName, typeof(ulong));
                        break;
                    case "int":
                    case "int3":
                        m_dataTable.Columns.Add(colName, typeof(int));
                        break;
                    case "uint":
                        m_dataTable.Columns.Add(colName, typeof(uint));
                        break;
                    case "short":
                        m_dataTable.Columns.Add(colName, typeof(short));
                        break;
                    case "ushort":
                        m_dataTable.Columns.Add(colName, typeof(ushort));
                        break;
                    case "sbyte":
                        m_dataTable.Columns.Add(colName, typeof(sbyte));
                        break;
                    case "byte":
                        m_dataTable.Columns.Add(colName, typeof(byte));
                        break;
                    case "float":
                        m_dataTable.Columns.Add(colName, typeof(float));
                        break;
                    case "double":
                        m_dataTable.Columns.Add(colName, typeof(double));
                        break;
                    case "index":
                        m_dataTable.Columns.Add(colName, typeof(int));
                        break;
                    case "string":
                    case "cstring":
                    case "nstring":
                        m_dataTable.Columns.Add(colName, typeof(string));
                        break;
                    default:
                        throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Unknown field type {0}!", field.Attributes["type"].Value));
                }
            }

            return m_dataTable;
        }


        private void CloseFile()
        {


            m_definition = null;
            m_dataTable = null;
        }

        private void ReadDB(string file)
        {
            try
            {
                m_dbreader = DBReaderFactory.GetReader(file);

            }
            catch (Exception ex)
            {
                // e.Cancel = true;
                return;
            }

            m_fields = m_definition.GetElementsByTagName("field");

            string[] types = new string[m_fields.Count];

            for (int j = 0; j < m_fields.Count; ++j)
                types[j] = m_fields[j].Attributes["type"].Value;

            // hack for *.adb files (because they don't have FieldsCount)
            bool notADB = !(m_dbreader is ADBReader);
            // hack for *.wdb files (because they don't have FieldsCount)
            bool notWDB = !(m_dbreader is WDBReader);
            // hack for *.wdb files (because they don't have FieldsCount)
            bool notSTL = !(m_dbreader is STLReader);

            bool notDB5 = !(m_dbreader is DB5Reader);

            int fcount = GetFieldsCount(m_fields);
            if (fcount != m_dbreader.FieldsCount && notADB && notWDB && notSTL && notDB5)
            {
                if (!types[0].Equals("index"))
                {

                    string msg = String.Format(CultureInfo.InvariantCulture, "{0} has invalid definition!\nFields count mismatch: got {1}, expected {2}", Path.GetFileName(file), fcount, m_dbreader.FieldsCount);

                    return;

                }
            }

            m_dataTable = new PluginInterface.DataTable(Path.GetFileName(file));
            m_dataTable.Locale = CultureInfo.InvariantCulture;

            CreateColumns();                                // Add columns

            CreateIndexes();                                // Add indexes

            //bool extraData = false;

            //   Console.WriteLine(m_dbreader.RecordsCount);

            for (int i = 0; i < m_dbreader.RecordsCount; ++i) // Add rows
            {
                DataRow dataRow = m_dataTable.NewRow();

                //    Console.WriteLine(m_dbreader.RecordsCount);

                using (BinaryReader br = m_dbreader[i])
                {
                    //  Console.WriteLine(m_dbreader[i].BaseStream.Length);

                    for (int j = 0; j < m_fields.Count; ++j)    // Add cells
                    {
                        //  Console.WriteLine(m_fields.Count);

                        switch (types[j])
                        {
                            case "long":
                                dataRow[j] = br.ReadInt64();
                                break;
                            case "ulong":
                                dataRow[j] = br.ReadUInt64();
                                break;
                            case "int":
                                dataRow[j] = br.ReadInt32();
                                break;
                            case "uint":
                                dataRow[j] = br.ReadUInt32();
                                break;
                            case "short":
                                dataRow[j] = br.ReadInt16();
                                break;
                            case "ushort":
                                dataRow[j] = br.ReadUInt16();
                                break;
                            case "sbyte":
                                dataRow[j] = br.ReadSByte();
                                break;
                            case "byte":
                                dataRow[j] = br.ReadByte();
                                break;
                            case "float":
                                dataRow[j] = br.ReadSingle();
                                break;
                            case "double":
                                dataRow[j] = br.ReadDouble();
                                break;
                            case "index":
                                dataRow[j] = m_dbreader.index[i];
                                break;
                            case "int3":
                                byte[] bytes = br.ReadBytes(3);

                                byte[] buffer = new byte[4];

                                Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);

                                dataRow[j] = BitConverter.ToInt32(buffer, 0);

                                break;

                            case "string":
                                if (m_dbreader is WDBReader)
                                    dataRow[j] = br.ReadStringNull();
                                else if (m_dbreader is STLReader)
                                {
                                    int offset = br.ReadInt32();
                                    dataRow[j] = (m_dbreader as STLReader).ReadString(offset);
                                }
                                else
                                {

                                    try
                                    {
                                        dataRow[j] = m_dbreader.StringTable[br.ReadInt32()];
                                    }
                                    catch
                                    {
                                        dataRow[j] = "Invalid string index!";

                                    }
                                }
                                break;
                            case "nstring":
                                try
                                {

                                    dataRow[j] = br.ReadStringNull();
                                }
                                catch
                                {

                                    dataRow[j] = "Invalid string index!";
                                }
                                break;
                            case "cstring":
                                dataRow[j] = br.ReadStringNumber();
                                break;
                            default:
                                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Unknown field type {0}!", types[j]));
                        }
                    }
                }

                m_dataTable.Rows.Add(dataRow);

                //int percent = (int)((float)m_dataTable.Rows.Count / (float)m_dbreader.RecordsCount * 100.0f);
                //(sender as BackgroundWorker).ReportProgress(percent);
            }




            Export2SQL.Export2SQL tosql = new Export2SQL.Export2SQL();
            tosql.Run(m_dataTable);

            CloseFile();
        }

    }
}
