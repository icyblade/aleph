using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PluginInterface;

namespace wowSpellArgsReplace
{
    public class Export2SQL
    {
        [Import("PluginFinished")]
        public Action<int> Finished { get; set; }

        public void Run(PluginInterface.DataTable data)
        {
            StreamWriter sqlWriter = new StreamWriter(Path.GetFileNameWithoutExtension(data.TableName) + ".sql");

            WriteSqlStructure(sqlWriter, data);

            foreach (DataRow row in data.Rows)
            {
                StringBuilder result = new StringBuilder();
                result.AppendFormat("INSERT INTO `{0}` VALUES (", Path.GetFileNameWithoutExtension(data.TableName));

                int flds = 0;

                for (var i = 0; i < data.Columns.Count; ++i)
                {
                    switch (data.Columns[i].DataType.Name)
                    {
                        case "Int64":
                            result.Append(row[i]);
                            break;
                        case "UInt64":
                            result.Append(row[i]);
                            break;
                        case "Int32":
                            result.Append(row[i]);
                            break;
                        case "UInt32":
                            result.Append(row[i]);
                            break;
                        case "Int16":
                            result.Append(row[i]);
                            break;
                        case "UInt16":
                            result.Append(row[i]);
                            break;
                        case "SByte":
                            result.Append(row[i]);
                            break;
                        case "Byte":
                            result.Append(row[i]);
                            break;
                        case "Single":
                            result.Append(((float)row[i]).ToString(CultureInfo.InvariantCulture));
                            break;
                        case "Double":
                            result.Append(((double)row[i]).ToString(CultureInfo.InvariantCulture));
                            break;
                        case "String":
                            result.Append("\"" + StripBadCharacters((string)row[i]) + "\"");
                            break;
                        default:
                            throw new Exception(String.Format("Unknown field type {0}!", data.Columns[i].DataType.Name));
                    }

                    if (flds != data.Columns.Count - 1)
                        result.Append(", ");

                    flds++;
                }

                result.Append(");");
                sqlWriter.WriteLine(result);
            }

            sqlWriter.Flush();
            sqlWriter.Close();



            Console.WriteLine("Finished:" + data.Rows.Count);
            // Finished(data.Rows.Count);
        }

//        private void WriteSqlStructure(StreamWriter sqlWriter, PluginInterface.DataTable data)
//        {
//            sqlWriter.WriteLine(@"DROP TABLE IF EXISTS `{0}`;
//CREATE TABLE `{0}` (
//	`m_ID` INT UNSIGNED NOT NULL DEFAULT '0',
//	`m_name_lang` TEXT NOT NULL,
//	`m_nameSubtext_lang` TEXT NOT NULL,
//	`m_description_lang` TEXT NOT NULL,
//	`m_auraDescription_lang` TEXT NOT NULL,
//	`m_spellMisc` INT UNSIGNED NOT NULL DEFAULT '0',
//	`m_spellDescriptionVariableID` INT UNSIGNED NOT NULL DEFAULT '0',
//    PRIMARY KEY(`m_ID`),
//    KEY `idx_dbc_Spell_m_spellMisc` (`m_spellMisc`)
//) ENGINE = MyISAM DEFAULT CHARSET = utf8 COMMENT = 'Export of Spell.db2'; ",data.TableName);
//            sqlWriter.WriteLine();
//        }

        private void WriteSqlStructure(StreamWriter sqlWriter, PluginInterface.DataTable data)
        {
            sqlWriter.WriteLine("DROP TABLE IF EXISTS `{0}`;", data.TableName);
            sqlWriter.WriteLine("CREATE TABLE `{0}` (", data.TableName);

            Console.WriteLine("columnsCount:" + data.Columns.Count);

            for (var i = 0; i < data.Columns.Count; ++i)
            {
                sqlWriter.Write("\t" + String.Format("`{0}`", data.Columns[i].ColumnName));

                switch (data.Columns[i].DataType.Name)
                {
                    case "Int64":
                        sqlWriter.Write(" BIGINT NOT NULL DEFAULT '0'");
                        break;
                    case "UInt64":
                        sqlWriter.Write(" BIGINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "Int32":
                        sqlWriter.Write(" INT NOT NULL DEFAULT '0'");
                        break;
                    case "UInt32":
                        sqlWriter.Write(" INT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "Int16":
                        sqlWriter.Write(" SMALLINT NOT NULL DEFAULT '0'");
                        break;
                    case "UInt16":
                        sqlWriter.Write(" SMALLINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "SByte":
                        sqlWriter.Write(" TINYINT NOT NULL DEFAULT '0'");
                        break;
                    case "Byte":
                        sqlWriter.Write(" TINYINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "Single":
                        sqlWriter.Write(" FLOAT NOT NULL DEFAULT '0'");
                        break;
                    case "Double":
                        sqlWriter.Write(" DOUBLE NOT NULL DEFAULT '0'");
                        break;
                    case "String":
                        sqlWriter.Write(" TEXT NOT NULL");
                        break;
                    default:
                        throw new Exception(String.Format("Unknown field type {0}!", data.Columns[i].DataType.Name));
                }
                if (i == data.Columns.Count - 1)
                {

                }
                else
                {
                    sqlWriter.WriteLine(",");
                }

            }

            bool pkey = false;

            bool nonuniquekey = false;

            foreach (DataColumn index in data.PrimaryKey)
            {
                // sqlWriter.WriteLine("\tPRIMARY KEY (`{0}`)", index.ColumnName);
                pkey = true;
            }


            foreach (DataColumn index in data.NonUniqueIndexs)
            {
                // sqlWriter.WriteLine("\tPRIMARY KEY (`{0}`)", index.ColumnName);

                //  Console.WriteLine("NKEY");
                nonuniquekey = true;
            }

            if (pkey)
            {
                //Console.WriteLine("Here");
                sqlWriter.WriteLine(",");
            }
            else if (!pkey && nonuniquekey)
            {

            }
            else
            {
                sqlWriter.WriteLine("");
            }

            int count = 0;

            foreach (DataColumn index in data.PrimaryKey)
            {

                if (count == data.PrimaryKey.Length - 1 && !nonuniquekey)
                {
                    sqlWriter.WriteLine("\tPRIMARY KEY (`{0}`)", index.ColumnName);
                }
                else
                {

                    sqlWriter.WriteLine("\tPRIMARY KEY (`{0}`),", index.ColumnName);
                }
                count++;
            }





            if (pkey && nonuniquekey)
            {
                //sqlWriter.WriteLine("");
                //sqlWriter.Write(",");
            }
            else if (!pkey && nonuniquekey)
            {

                sqlWriter.WriteLine(",");
            }
            else
            {
                // sqlWriter.WriteLine("");
            }

            int count_2 = 0;

            foreach (DataColumn index in data.NonUniqueIndexs)
            {

                if (count_2 == data.NonUniqueIndexs.Length - 1)
                {
                    sqlWriter.WriteLine("\tKEY `idx_{0}_{1}` (`{2}`)", Path.GetFileNameWithoutExtension(data.TableName), index.ColumnName, index.ColumnName);
                }
                else
                {

                    sqlWriter.WriteLine("\tKEY `idx_{0}_{1}` (`{2}`),", Path.GetFileNameWithoutExtension(data.TableName), index.ColumnName, index.ColumnName);
                }
                count_2++;
            }


            sqlWriter.WriteLine(") ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Export of {0}';", data.TableName);
            sqlWriter.WriteLine();
        }


        static string StripBadCharacters(string input)
        {

            input = input.Replace(@"\", @"\\");
            // input = Regex.Replace(input, @"\""", @"\\\\\\\\""");
            input = input.Replace(@"'", @"\'");
            //input = Regex.Replace(input, @"'", @"\'");
            input = input.Replace("\"", "\\\"");
            return input;
        }
    }
}
