using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace wowSpellArgsReplace
{
    class repSpell
    {

        public const string imageURL = "http://static.tuwan.com/templet/wow/hd/tf7/images/";

        readonly static Regex durationRx = new Regex(@"[$]([0-9]*)[dD]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex timePeriodRx = new Regex(@"[$]([0-9]*)[tTp]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex rangeRx = new Regex(@"[$]([0-9]*)[rR]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex radius1Rx = new Regex(@"[$]([0-9]*)[a]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex radius2Rx = new Regex(@"[$]([0-9]*)[A]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex swvalueRx = new Regex(@"[$]([0-9]*)[sS][wW]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex svalueRx = new Regex(@"[$]([0-9]*)[sSwWf]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex ovalueRx = new Regex(@"[$]([0-9]*)[oO]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex bvalueRx = new Regex(@"[$]([0-9]*)[bB]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex minvalueRx = new Regex(@"[$]([0-9]*)[m]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex maxvalueRx = new Regex(@"[$]([0-9]*)[M]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex cvalueRx = new Regex(@"[$]([0-9]*)[cC]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex evalueRx = new Regex(@"[$]([0-9]*)[eE]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex hvalueRx = new Regex(@"[$]([0-9]*)[hH]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex ivalueRx = new Regex(@"[$]([0-9]*)[iI]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex nvalueRx = new Regex(@"[$]([0-9]*)[nN]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex qvalueRx = new Regex(@"[$]([0-9]*)[q]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex uvalueRx = new Regex(@"[$]([0-9]*)[uU]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex vvalueRx = new Regex(@"[$]([0-9]*)[v]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex xvalueRx = new Regex(@"[$]([0-9]*)[xX]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex zvalueRx = new Regex(@"[$]([0-9]*)[z]([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex rppmvalueRx = new Regex(@"[$]([0-9]*)procrppm([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex icdvalueRx = new Regex(@"[$]([0-9]*)proccooldown([0-9]*)(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex absRx = new Regex(@"[$]abs(?![a-zA-Z])", RegexOptions.Compiled);
        readonly static Regex colorRx = new Regex(@"[|][cC]([a-zA-Z0-9]{2})([a-zA-Z0-9]{6})([\s\S]*?)[|][Rr]", RegexOptions.Compiled);
        readonly static Regex ticonRx = new Regex(@"[|][Tt]([\s\S]*?)[:]([\s\S]*?)[|][Tt]", RegexOptions.Compiled);

        readonly static Regex divisionReg = new Regex(@"[$]([0-9]*)[/]([0-9]*)[;]([0-9]*[a-zA-Z]*[0-9]*)", RegexOptions.Compiled);
        readonly static Regex multiplicationReg = new Regex(@"[$]([0-9]*)[*]([0-9]*)[;]([0-9]*[a-zA-Z]*[0-9]*)", RegexOptions.Compiled);


        readonly static Regex specialReg = new Regex(@"[$][^\$\+\-\*\/\{\}\(\)\u4e00-\u9fa5\s\[\]\,\.\，\。\）\（\%\、\—\：\:\;\；\|]+", RegexOptions.Compiled);


        readonly static Regex glyphRegex = new Regex(@"[$]\?[\s\S]*?\]\[[\s\S]*?\]", RegexOptions.Compiled);

        readonly static Regex SGglyphRegex = new Regex(@"[$]\?[^0-9]*?([0-9]*)\[([\s\S]*?)\]", RegexOptions.Compiled);

        readonly static Regex mountReg = new Regex(@"[$]\?[j][\s\S]*?\[([\s\S]*?)\]\[([\s\S]*?)\]", RegexOptions.Compiled);

        readonly static Regex multiGenRegex = new Regex(@"[$][A-Za-z\u4e00-\u9fa5]*?[:][\S\s]*?[;]", RegexOptions.Compiled);

        readonly static Regex recursionRegex = new Regex(@"[$][{]([^\{]*?)[}]", RegexOptions.Compiled);

        readonly static Regex regex1 = new Regex(@"[$][^\u4e00-\u9fa5\s\%\.\,\。\，\{\}\/\*\+\-]+", RegexOptions.Compiled);

        readonly static Regex regex2 = new Regex(@"[$][{][\s\S]*?[}]", RegexOptions.Compiled);

        readonly static Regex regex3 = new Regex(@"[$][{]([^\{]*?)[}]", RegexOptions.Compiled);



        public repSpell(string connString)
        {
            regexList.Add(durationRx);
            regexList.Add(timePeriodRx);
            regexList.Add(rangeRx);
            regexList.Add(radius1Rx);
            regexList.Add(radius2Rx);
            regexList.Add(swvalueRx);
            regexList.Add(svalueRx);
            regexList.Add(ovalueRx);
            regexList.Add(bvalueRx);
            regexList.Add(minvalueRx);
            regexList.Add(maxvalueRx);
            regexList.Add(cvalueRx);
            regexList.Add(evalueRx);
            regexList.Add(hvalueRx);
            regexList.Add(ivalueRx);
            regexList.Add(nvalueRx);
            regexList.Add(qvalueRx);
            regexList.Add(uvalueRx);
            regexList.Add(vvalueRx);
            regexList.Add(xvalueRx);
            regexList.Add(zvalueRx);
            regexList.Add(rppmvalueRx);
            regexList.Add(icdvalueRx);
            regexList.Add(absRx);
            Conn = new MySqlConnection(connString);
        }



        MySqlConnection Conn;

        public DataSet getDS(DataSet ds, string sql)
        {
            //打开连接   
            //    getConnection();
            Conn.Open();
            try
            {
                //查询结果   

                // MySqlDataAdapter t2 = new MySqlDataAdapter("set names utf8", Conn);
                MySqlCommand cmd1 = new MySqlCommand("set names utf8", Conn);
                cmd1.ExecuteNonQuery();

                /*   MySqlCommand cmd = new MySqlCommand("INSERT INTO `dbc_JournalInstance` VALUES (9999, 1358, 0, 1042000, 1042065, 1041974, 1041990, 0, \""+test+"\", \"测试啊\");", Conn);
                   cmd.ExecuteNonQuery(); */
                MySqlDataAdapter mda = new MySqlDataAdapter(sql, Conn);
                //  Console.WriteLine(mda.SelectCommand.CommandText);

                mda.Fill(ds);

            }
            catch (System.Exception e)
            {
                //MessageBox.Show("getDS()异常：" + e.Message);
                Console.WriteLine(e.Message);
            }
            finally
            {
                Conn.Close();
                Conn.Dispose();
            }


            return ds;
        }

        public DataSet getDataSet(string sqlquery)
        {
            DataSet dat = new DataSet();
            getDS(dat, sqlquery);

            return dat;
        }

        public struct Spell
        {
            public int m_ID, m_spellMisc, m_spellDescriptionVariableID;
            public string m_name_lang;
            public string m_nameSubtext_lang;
            public string m_description_lang;
            public string m_auraDescription_lang;


        }

        public struct artifactPower
        {
            public int m_ID, m_powerID, m_artifactID, m_Circle, m_powerType, m_rank, m_maxRank, m_spellID, m_spellMisc, m_spellDescriptionVariableID;
            public string m_value;
            public string m_pos_x;
            public string m_pos_y;
            public string m_name_lang;
            public string m_nameSubtext_lang;
            public string m_description_lang;
            public string m_auraDescription_lang;
            public string m_iconPath;


        }

        public struct powerLoc
        {
            public double X, Y;


        }

        public struct powerLink
        {
            public int m_ID, m_startID, m_endID, m_artifactID;

            public double m_left;
            public double m_top;
            public double m_width;
            public double m_rotate;

            public powerLoc startLoc;
            public powerLoc endLoc;

        }

        public List<Spell> spellList = new List<Spell>();

        public List<artifactPower> apList = new List<artifactPower>();

        public List<powerLink> linkList = new List<powerLink>();



        private void readallspell()
        {
            string sqlquery = "SELECT * FROM dbc_Spell;";

            spellList.Clear();
            // 
            DataSet da = getDataSet(sqlquery);

            for (int i = 0; i < da.Tables[0].Rows.Count; i++)
            {
                Spell spell;

                spell.m_ID = int.Parse(da.Tables[0].Rows[i]["m_ID"].ToString());
                spell.m_spellMisc = int.Parse(da.Tables[0].Rows[i]["m_spellMisc"].ToString());
                spell.m_spellDescriptionVariableID = int.Parse(da.Tables[0].Rows[i]["m_spellDescriptionVariableID"].ToString());

                spell.m_name_lang = da.Tables[0].Rows[i]["m_name_lang"].ToString();
                spell.m_nameSubtext_lang = da.Tables[0].Rows[i]["m_nameSubtext_lang"].ToString();
                spell.m_description_lang = da.Tables[0].Rows[i]["m_description_lang"].ToString();
                spell.m_auraDescription_lang = da.Tables[0].Rows[i]["m_auraDescription_lang"].ToString();

                spellList.Add(spell);
            }

            Console.WriteLine("Finish");
        }


        public List<string> strlst = new List<string>();


        public string regexGlyph(string str)
        {


            string s = str;


            if (str.Contains("$?"))
            {


                // Console.WriteLine();

                s = glyphRegex.Replace(str, ReplaceCC);

                s = SGglyphRegex.Replace(s, sgRegex);

                strlst.Add(s);

            }

            return s;
        }

        public string sgRegex(Match t)
        {
            if (t.Groups[1].Length > 0)
            {
                if (t.Groups[1].Value.Equals(currentSpell.m_ID.ToString()))
                {
                    return t.Groups[2].Value;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return t.Value;
            }
        }

        public string ReplaceCC(Match t)
        // Replace each Regex cc match with the number of the occurrence.
        {


            Match mt = mountReg.Match(t.Value);

            if (mt.Value.StartsWith("$?j"))
            {

                string sql = @"SELECT dbc_Mount.m_mountType FROM dbc_Spell,dbc_Mount where dbc_Spell.m_auradescription_lang like '%$j%' and (dbc_Mount.m_spellID=dbc_Spell.m_ID or dbc_Spell.m_name_lang = dbc_Mount.m_name) and dbc_Spell.m_ID=" + currentSpell.m_ID + ";";

                DataSet da = getDataSet(sql);

                if (da.Tables[0].Rows.Count != 0)
                {

                    for (int i = 0; i < da.Tables[0].Rows.Count; i++)
                    {
                        int mountType = int.Parse(da.Tables[0].Rows[i]["m_mountType"].ToString());

                        switch (mountType)
                        {
                            case 230:
                            case 241:
                            case 291:
                            case 269:
                            case 284:
                                if (mt.Value.StartsWith("$?j1g"))
                                {
                                    return mt.Groups[1].Value + "\r";
                                }
                                break;
                            case 242:
                            case 247:
                            case 248:
                                if (mt.Value.StartsWith("$?j1g"))
                                {
                                    return mt.Groups[1].Value + "\r";
                                }
                                else if (mt.Value.StartsWith("$?j1f"))
                                {
                                    return mt.Groups[1].Value + "\r";
                                }
                                break;
                            case 231:
                                if (mt.Value.StartsWith("$?j1g"))
                                {
                                    return mt.Groups[1].Value + "\r";
                                }
                                else if (mt.Value.StartsWith("$?j1s"))
                                {
                                    return mt.Groups[1].Value + "\r";
                                }
                                break;
                            case 232:
                            case 254:
                                if (mt.Value.StartsWith("$?j1s"))
                                {
                                    return mt.Groups[1].Value + "\r";
                                }
                                break;
                            default:
                                return mt.Groups[2].Value + "\r";
                                break;
                        }
                    }
                }
                else
                {


                    if (mt.Value.StartsWith("$?j1g"))
                    {
                        //Console.WriteLine(currentSpell.m_ID);
                        return mt.Groups[1].Value + "\r";
                    }


                }



            }

            string temp = t.Value.Substring(t.Value.IndexOf("]["), t.Value.LastIndexOf("]") - t.Value.IndexOf("][")).Replace("][", "");

            //  Console.WriteLine(t.Value);
            //  Console.WriteLine(temp);

            return temp;
        }



        List<string> argsList = new List<string>();

        List<string> mutilList = new List<string>();

        public string regexMutilGen(string str)
        {


            string s = str;

            if (multiGenRegex.Matches(s).Count > 0)
            {



                s = multiGenRegex.Replace(str, ReplaceMutil);

                mutilList.Add(s);

            }

            return s;

        }

        public string ReplaceMutil(Match t)
        // Replace each Regex cc match with the number of the occurrence.
        {
            string[] temp = t.Value.Split(':');

            /// Console.WriteLine(t.Value);

            return temp[0].Substring(2);
        }


        public Spell currentSpell;

        public string ReplaceMultiCal(Match t)
        {

            string str = t.Value;


            try
            {
                string arg = t.Groups[1].Value;
                str = numberFormatCov(decimal.Parse(new DataTable().Compute(arg, null).ToString()));

                return str;
            }
            catch
            {

                return "[" + t.Groups[1].Value + "]";

            }




        }

        public string ReplaceABSMultiCal(Match t)
        {

            string str = t.Value;


            try
            {
                string arg = t.Groups[1].Value;
                str = numberFormatCov(decimal.Parse(new DataTable().Compute(arg, null).ToString()));

                str = numberFormatABSCov(str);

                return str;
            }
            catch
            {

                return "[" + t.Groups[1].Value + "]";

            }




        }


        public string recursionReplace(string str)
        {

            string temp = recursionRegex.Replace(str, ReplaceMultiCal);



            if (temp.Equals(str))
            {
                string final = recursionRegex.Replace(str, ReplaceABSMultiCal);

                return final;
            }
            else
            {
                return recursionReplace(temp);
            }


        }

        public string regexArgs(string str)
        {

            // Regex rx1 = new Regex(@"[$][^\u4e00-\u9fa5\s\%\.\,\。\，]+");



            string s = str;



            if (regex2.Matches(str).Count > 0)
            {
                s = regex2.Replace(str, ReplaceMultiArg);
                s = regex1.Replace(s, ReplaceArg);

                s = recursionReplace(s);

                // s = rx3.Replace(s,ReplaceMultiCal);

                return s;
                //return s;
            }


            s = regex1.Replace(str, ReplaceArg);




            return s;

            /*   if (s.Equals(str))
               {
                   argReplace.Add(s);
                   return s;
               }
               else
               {
                   return regexArgs(s);
               }
               */


            // Console.WriteLine(s);

            //   return s;
        }



        List<string> argReplace = new List<string>();


        public string timeDurationFormatCov(decimal time)
        {

            if (time < 60000)
            {

                if (time == -1 || time == 0)
                {
                    return "直到主动取消";
                }

                return numberFormatCov(Math.Abs(time / 1000)) + "秒";
            }


            TimeSpan tt = new TimeSpan(0, 0, 0, 0, (int)Math.Abs(time));

            String rtstr = "";

            if (tt.Days != 0)
            {
                rtstr = tt.TotalDays.ToString("#.##") + "天";
                return rtstr;
            }

            if (tt.Hours != 0)
            {
                rtstr = tt.TotalHours.ToString("#.##") + "小时";
                return rtstr;
            }

            if (tt.Minutes != 0)
            {
                rtstr = tt.TotalMinutes.ToString("#.##") + "分钟";
                return rtstr;
            }

            if (tt.Seconds != 0)
            {
                rtstr = tt.TotalSeconds.ToString("#.##") + "秒";
                return rtstr;
            }


            return rtstr;

        }

        public string numberFormatCov(decimal number)
        {

            if (number - Math.Floor(number) > 0)
            {
                return number.ToString("0.##");
            }
            else
            {
                return (number).ToString("0.##");
            }

        }

        public string numberFormatABSCov(string str)
        {

            decimal number;
            //try { 
            //decimal number = decimal.Parse(str);


            if (decimal.TryParse(str, out number))
            {
                if (Math.Abs(number) - Math.Floor(Math.Abs(number)) > 0)
                {
                    return Math.Abs(number).ToString("0.##");
                }
                else
                {
                    return Math.Abs(number).ToString("0.##");
                }
            }
            else
            {
                return str;
            }

            //}
            //catch
            //{
            //    return str;
            //}
        }


        List<string> missingList = new List<string>();


        public decimal RequestDuration(string arg1, string arg2)
        {
            string sql = "";

            // Console.WriteLine(arg1);

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellduration.m_ms_base FROM dbc_Spellduration,dbc_Spellmisc,dbc_Spell where dbc_Spell.m_ID =" + arg1 + " and dbc_Spell.m_spellmisc=dbc_Spellmisc.m_ID and dbc_Spellmisc.m_durationIndex = dbc_Spellduration.m_ID;";
            }
            else
            {
                sql = "SELECT dbc_Spellduration.m_ms_base FROM dbc_Spellduration,dbc_Spellmisc,dbc_Spell where dbc_Spell.m_ID =" + currentSpell.m_ID + " and dbc_Spell.m_spellmisc=dbc_Spellmisc.m_ID and dbc_Spellmisc.m_durationIndex = dbc_Spellduration.m_ID;";
            }

            //  Console.WriteLine(sql);

            // 
            DataSet da = getDataSet(sql);

            if (da.Tables[0].Rows.Count != 0)
            {

                for (int i = 0; i < da.Tables[0].Rows.Count; i++)
                {
                    decimal time = decimal.Parse(da.Tables[0].Rows[i]["m_ms_base"].ToString());

                    if (time == -1 || time == 0)
                    {
                        time = 0;
                    }

                    return time;

                }
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spellduration.m_ms_base FROM dbc_Spellduration,dbc_Spellmisc,dbc_Spell where dbc_Spell.m_ID =" + currentSpell.m_ID + " and dbc_Spell.m_spellmisc=dbc_Spellmisc.m_ID and dbc_Spellmisc.m_durationIndex = dbc_Spellduration.m_ID;";
                    da = getDataSet(sql);

                    if (da.Tables[0].Rows.Count != 0)
                    {

                        for (int i = 0; i < da.Tables[0].Rows.Count; i++)
                        {
                            decimal time = decimal.Parse(da.Tables[0].Rows[i]["m_ms_base"].ToString());

                            if (time == -1 || time == 0)
                            {
                                time = 0;
                            }

                            return time;

                        }
                    }
                }
            }


            missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang);

            return 0;


        }

        public string RequestTimePeriod(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectAuraPeriod FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectAuraPeriod FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);

            //   
            DataSet da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal time = decimal.Parse(da.Tables[0].Rows[i]["m_effectAuraPeriod"].ToString());

                if (time == 0)
                {
                    return "5";
                }

                return numberFormatCov(time / 1000);
            }


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestRange(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellrange.m_rangeMax1 FROM dbc_Spell,dbc_Spellmisc,dbc_Spellrange where dbc_Spell.m_spellMisc=dbc_Spellmisc.m_ID and dbc_Spellmisc.m_rangeIndex = dbc_Spellrange.m_ID and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellrange.m_rangeMax1 FROM dbc_Spell,dbc_Spellmisc,dbc_Spellrange where dbc_Spell.m_spellMisc=dbc_Spellmisc.m_ID and dbc_Spellmisc.m_rangeIndex = dbc_Spellrange.m_ID and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);

            //  
            DataSet da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal range = decimal.Parse(da.Tables[0].Rows[i]["m_rangeMax1"].ToString());

                return numberFormatCov(range);
            }


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";
        }

        public string RequestRadius1(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg1:" + arg1);
            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex1=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex1=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //   Console.WriteLine(sql);

            // 
            DataSet da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal range = decimal.Parse(da.Tables[0].Rows[i]["m_radius"].ToString());

                return numberFormatCov(range);
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex2=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex2=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal range = decimal.Parse(da.Tables[0].Rows[i]["m_radius"].ToString());

                return numberFormatCov(range);
            }

            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }


            return "0";
        }


        public string RequestRadius2(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex2=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex2=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal range = decimal.Parse(da.Tables[0].Rows[i]["m_radius"].ToString());

                return numberFormatCov(range);
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex1=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellradius.m_radius FROM dbc_Spell,dbc_Spelleffect,dbc_Spellradius where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spelleffect.m_radiusIndex1=dbc_Spellradius.m_ID and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal range = decimal.Parse(da.Tables[0].Rows[i]["m_radius"].ToString());

                return numberFormatCov(range);
            }


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";
        }


        public string sValuesCatcher(DataSet da)
        {
            if (da.Tables[0].Rows.Count != 0)
            {

                for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                {
                    decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                    decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                    decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                    decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                    int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());
                    int effectAuraType = int.Parse(da.Tables[0].Rows[i]["m_effectAuraType"].ToString());

                    decimal resultBase = basePoints + bonusPoints;

                    uint m_attributes9 = uint.Parse(da.Tables[0].Rows[i]["m_attributes9"].ToString());
                    if (m_attributes9 == 536870912)
                    {
                        if (bonusCoefficient != 0)
                        {

                            if (resultBase != 0)
                            {
                                return ("(" + resultBase + "+面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")");
                            }
                            else
                            {
                                return ("(面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")");
                            }


                        }
                        else
                        {
                            return numberFormatCov(resultBase);

                        }

                    }

                    switch (effectType)
                    {

                        case 2:
                        case 9:
                        case 10:
                            if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {

                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }

                            }
                            else if (bonusCoefficient != 0)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                            }
                            else if (bonusCoefficientFromAP != 0)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                            }
                            else
                            {
                                return numberFormatCov(resultBase);
                            }
                            break;

                        case 6:
                        case 27:
                            switch (effectAuraType)
                            {
                                case 3:
                                case 8:
                                case 89:
                                case 138:
                                    if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient != 1)
                                    {
                                        if (resultBase == 0)
                                        {
                                            return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                        }
                                        else
                                        {

                                            return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                        }

                                    }
                                    else if (bonusCoefficient != 0)
                                    {
                                        if (resultBase == 0)
                                        {
                                            return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                        }
                                        else
                                        {
                                            return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                        }
                                    }
                                    else if (bonusCoefficientFromAP != 0)
                                    {
                                        if (resultBase == 0)
                                        {
                                            return "(" + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                        }
                                        else
                                        {
                                            return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                        }
                                    }
                                    else
                                    {
                                        return numberFormatCov(resultBase);
                                    }
                                    break;
                                default:
                                    return numberFormatCov(resultBase);
                            }
                            break;
                        default:
                            return numberFormatCov(resultBase);
                            break;
                    }


                    // return "0";
                }
            }
            else
            {

                return "NULL";
            }
            return "NULL";
        }

        public string swValuesCatcher(DataSet da, string arg1, string sql)
        {
            if (da.Tables[0].Rows.Count != 0)
            {
                for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                {
                    decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                    decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                    decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                    decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                    int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                    decimal resultBase = basePoints + bonusPoints;

                    uint m_attributes9 = uint.Parse(da.Tables[0].Rows[i]["m_attributes9"].ToString());

                    if (m_attributes9 == 536870912)
                    {
                        if (bonusCoefficient != 0)
                        {
                            if (resultBase != 0)
                            {
                                return ("[(" + resultBase + "+面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")%武器伤害]");
                            }
                            else
                            {
                                return ("(面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")");
                            }
                        }
                        else
                        {
                            return numberFormatCov(resultBase);
                        }

                    }


                    switch (effectType)
                    {

                        case 2:
                        case 6:
                        case 10:
                        case 27:
                            if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {

                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }

                            }
                            else if (bonusCoefficient != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                            }
                            else if (bonusCoefficientFromAP != 0)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                            }
                            else
                            {
                                return numberFormatCov(resultBase);
                            }
                            break;
                        case 31:
                            return "(" + numberFormatCov(resultBase) + "%武器伤害)";
                            break;


                        case 58:
                        case 121:
                            if (resultBase == 0 || resultBase == 1)
                            {
                                if (arg1.Length > 0)
                                {
                                    //Console.WriteLine();

                                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType =31 and dbc_Spell.m_ID =" + arg1 + ";";
                                }
                                else
                                {
                                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType =31 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                                }

                                da = getDataSet(sql);
                                for (i = 0; i < da.Tables[0].Rows.Count; i++)

                                {
                                    basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                                    bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                                    bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                                    bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                                    effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                                    resultBase = basePoints + bonusPoints;

                                    return "(" + numberFormatCov(resultBase) + "%武器伤害)";
                                }

                                return "(" + numberFormatCov(100) + "%武器伤害)";
                            }
                            else
                            {

                                return "(" + numberFormatCov(resultBase) + "%武器伤害)";
                            }

                            break;
                        default:
                            return numberFormatCov(resultBase);
                            break;
                    }





                    //return (range).ToString();
                }
            }
            else
            {
                return "NULL";
            }

            return "NULL";
        }

        public string minValuesCatcher(DataSet da)
        {
            if (da.Tables[0].Rows.Count != 0)
            {

                for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                {
                    decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                    decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                    decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                    decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                    bonusCoefficient = 0;
                    bonusCoefficientFromAP = 0;

                    int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                    decimal resultBase = 0;

                    if (bonusPoints != 0)
                    {

                        resultBase = basePoints + 1;
                    }
                    else
                    {
                        resultBase = basePoints;
                    }

                    uint m_attributes9 = uint.Parse(da.Tables[0].Rows[i]["m_attributes9"].ToString());
                    if (m_attributes9 == 536870912)
                    {
                        if (bonusCoefficient != 0)
                        {

                            if (resultBase != 0)
                            {
                                return ("(" + resultBase + "+面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")");
                            }
                            else
                            {
                                return ("(面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")");
                            }


                        }
                        else
                        {
                            return numberFormatCov(resultBase);

                        }
                    }

                    switch (effectType)
                    {

                        case 2:
                        case 6:
                        case 10:
                        case 27:
                            if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {

                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }

                            }
                            else if (bonusCoefficient != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                            }
                            else if (bonusCoefficientFromAP != 0)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                            }
                            else
                            {
                                return numberFormatCov(resultBase);
                            }
                            break;

                        default:
                            return numberFormatCov(resultBase);
                            break;
                    }


                    //return (range).ToString();
                }
            }
            else
            {

                return "NULL";
            }
            return "NULL";
        }

        public string maxValuesCatcher(DataSet da)
        {
            if (da.Tables[0].Rows.Count != 0)
            {

                for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                {
                    decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                    decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                    decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                    decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                    bonusCoefficient = 0;

                    bonusCoefficientFromAP = 0;

                    int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                    decimal resultBase = basePoints + bonusPoints;

                    uint m_attributes9 = uint.Parse(da.Tables[0].Rows[i]["m_attributes9"].ToString());
                    if (m_attributes9 == 536870912)
                    {
                        if (bonusCoefficient != 0)
                        {

                            if (resultBase != 0)
                            {
                                return ("(" + resultBase + "+面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")");
                            }
                            else
                            {
                                return ("(面板精通*" + numberFormatCov(bonusCoefficient / 2) + ")");
                            }


                        }
                        else
                        {
                            return numberFormatCov(resultBase);

                        }

                    }

                    switch (effectType)
                    {

                        case 2:
                        case 6:
                        case 10:
                        case 27:
                            if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {

                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }

                            }
                            else if (bonusCoefficient != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                }
                            }
                            else if (bonusCoefficientFromAP != 0)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                }
                            }
                            else
                            {
                                return numberFormatCov(resultBase);
                            }
                            break;

                        default:
                            return numberFormatCov(resultBase);
                            break;
                    }


                    // return "0";
                }
            }
            else
            {

                return "NULL";
            }
            return "NULL";
        }

        public string oValuesCatcher(DataSet da, decimal duration, decimal timePeriod)
        {
            if (da.Tables[0].Rows.Count != 0)
            {
                for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                {
                    decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                    decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                    decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                    decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                    int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                    decimal resultBase = basePoints + bonusPoints;

                    decimal effectiveTimes = duration / timePeriod;


                    switch (effectType)
                    {

                        case 2:
                        case 6:
                        case 10:
                        case 27:
                            if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100 * effectiveTimes) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100 * effectiveTimes) + "%攻击强度)";
                                }
                                else
                                {

                                    return "(" + numberFormatCov(resultBase * effectiveTimes) + " + " + numberFormatCov(bonusCoefficient * 100 * effectiveTimes) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100 * effectiveTimes) + "%攻击强度)";
                                }

                            }
                            else if (bonusCoefficient != 0 && bonusCoefficient != 1)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficient * 100 * effectiveTimes) + "%法术强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase * effectiveTimes) + " + " + numberFormatCov(bonusCoefficient * 100 * effectiveTimes) + "%法术强度)";
                                }
                            }
                            else if (bonusCoefficientFromAP != 0)
                            {
                                if (resultBase == 0)
                                {
                                    return "(" + numberFormatCov(bonusCoefficientFromAP * 100 * effectiveTimes) + "%攻击强度)";
                                }
                                else
                                {
                                    return "(" + numberFormatCov(resultBase * effectiveTimes) + " + " + numberFormatCov(bonusCoefficientFromAP * 100 * effectiveTimes) + "%攻击强度)";
                                }
                            }
                            else
                            {
                                return numberFormatCov(resultBase * effectiveTimes);
                            }
                            break;

                        default:
                            return numberFormatCov(resultBase * effectiveTimes);
                            break;
                    }


                    //return (range).ToString();
                }
            }
            else
            {

                return "NULL";
            }
            return "NULL";
        }

        public string generalValuesCatcher(DataSet da, string key)
        {
            if (da.Tables[0].Rows.Count != 0)
            {

                for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                {
                    decimal value = decimal.Parse(da.Tables[0].Rows[i][key].ToString());



                    return numberFormatCov(value);
                }
            }
            else
            {

                return "NULL";
            }
            return "NULL";
        }

        public string RequestSValue(string arg1, string arg2)
        {
            string sql = "";




            //Console.WriteLine("arg1:" + arg1);
            //  Console.WriteLine("arg2:" + arg2);

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }



            //
            DataSet da = getDataSet(sql);

            string rtstr = sValuesCatcher(da);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + arg1 + ";";

                    da = getDataSet(sql);
                    rtstr = sValuesCatcher(da);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        //Console.WriteLine(sql);
                        da = getDataSet(sql);
                        rtstr = sValuesCatcher(da);
                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";

                            da = getDataSet(sql);
                            rtstr = sValuesCatcher(da);
                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
                else
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";

                    da = getDataSet(sql);
                    rtstr = sValuesCatcher(da);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }


            /*     for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                 {
                     decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                     decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                     decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                     decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                     int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                     decimal resultBase = basePoints + bonusPoints;


                     switch (effectType)
                     {

                         case 2:
                         case 6:
                         case 10:
                         case 27:
                             if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient!=1)
                             {
                                 if (resultBase == 0)
                                 {
                                     return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                 }
                                 else
                                 {

                                     return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                 }

                             }
                             else if (bonusCoefficient != 0 && bonusCoefficient != 1)
                             {
                                 if (resultBase == 0)
                                 {
                                     return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                 }
                                 else
                                 {
                                     return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
                                 }
                             }
                             else if (bonusCoefficientFromAP != 0)
                             {
                                 if (resultBase == 0)
                                 {
                                     return "(" + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                 }
                                 else
                                 {
                                     return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
                                 }
                             }
                             else
                             {
                                 return numberFormatCov(resultBase);
                             }
                             break;

                         default:
                             return numberFormatCov(resultBase);
                             break;
                     }


                     //return (range).ToString();
                 }
                 */

            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";
        }

        public string RequestSWValue(string arg1, string arg2)
        {
            string sql = "";

            //    Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType=31 and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType=31 and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //Console.WriteLine(sql);

            // 
            DataSet da = getDataSet(sql);

            string rtstr = swValuesCatcher(da, arg1, sql);

            //  Console.WriteLine("1");

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType=31 and dbc_Spell.m_ID =" + arg1 + ";";
                    //Console.WriteLine(sql);
                    da = getDataSet(sql);
                    rtstr = swValuesCatcher(da, arg1, sql);

                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        //Console.WriteLine(sql);
                        da = getDataSet(sql);
                        rtstr = swValuesCatcher(da, arg1, sql);

                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and  dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                            //  Console.WriteLine(sql);
                            da = getDataSet(sql);
                            rtstr = swValuesCatcher(da, arg1, sql);

                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
                else
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType=31 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";

                    da = getDataSet(sql);
                    rtstr = swValuesCatcher(da, arg1, sql);

                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        //Console.WriteLine(sql);
                        da = getDataSet(sql);
                        rtstr = swValuesCatcher(da, arg1, sql);
                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and  dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                            //  Console.WriteLine(sql);
                            da = getDataSet(sql);
                            rtstr = swValuesCatcher(da, arg1, sql);
                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
            }

            //for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            //{
            //    decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
            //    decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
            //    decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
            //    decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

            //    int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

            //    decimal resultBase = basePoints + bonusPoints;


            //    switch (effectType)
            //    {

            //        case 2:
            //        case 6:
            //        case 10:
            //        case 27:
            //            if (bonusCoefficient != 0 && bonusCoefficientFromAP != 0 && bonusCoefficient != 1)
            //            {
            //                if (resultBase == 0)
            //                {
            //                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
            //                }
            //                else
            //                {

            //                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度 + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
            //                }

            //            }
            //            else if (bonusCoefficient != 0 && bonusCoefficient != 1)
            //            {
            //                if (resultBase == 0)
            //                {
            //                    return "(" + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
            //                }
            //                else
            //                {
            //                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficient * 100) + "%法术强度)";
            //                }
            //            }
            //            else if (bonusCoefficientFromAP != 0)
            //            {
            //                if (resultBase == 0)
            //                {
            //                    return "(" + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
            //                }
            //                else
            //                {
            //                    return "(" + numberFormatCov(resultBase) + " + " + numberFormatCov(bonusCoefficientFromAP * 100) + "%攻击强度)";
            //                }
            //            }
            //            else
            //            {
            //                return numberFormatCov(resultBase);
            //            }
            //            break;
            //        case 31:
            //            return "(" + numberFormatCov(resultBase) + "%武器伤害)";
            //            break;


            //        case 58:
            //        case 121:
            //            if (resultBase==0||resultBase==1)
            //            {
            //                if (arg1.Length > 0)
            //                {
            //                    //Console.WriteLine();

            //                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType =31 and dbc_Spell.m_ID =" + arg1 + ";";
            //                }
            //                else
            //                {
            //                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectType =31 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            //                }

            //                da = getDataSet(sql);
            //                for ( i = 0; i < da.Tables[0].Rows.Count; i++)

            //                {
            //                    basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
            //                    bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
            //                    bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
            //                    bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

            //                    effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

            //                    resultBase = basePoints + bonusPoints;

            //                    return "(" + numberFormatCov(resultBase) + "%武器伤害)";
            //                }

            //                return "(" + numberFormatCov(100) + "%武器伤害)";
            //            }
            //            else
            //            {

            //                return "(" + numberFormatCov(resultBase) + "%武器伤害)";
            //            }

            //            break;
            //        default:
            //            return numberFormatCov(resultBase);
            //            break;
            //    }





            //    //return (range).ToString();
            //}


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";
        }

        public string RequestMinValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);

            DataSet da = getDataSet(sql);

            string rtstr = minValuesCatcher(da);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + arg1 + ";";
                    da = getDataSet(sql);
                    rtstr = minValuesCatcher(da);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        da = getDataSet(sql);
                        rtstr = minValuesCatcher(da);
                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                            da = getDataSet(sql);
                            rtstr = minValuesCatcher(da);
                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
                else
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = minValuesCatcher(da);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }



            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";
        }

        public string RequestMaxValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);

            string rtstr = maxValuesCatcher(da);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + arg1 + ";";
                    da = getDataSet(sql);
                    rtstr = maxValuesCatcher(da);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        da = getDataSet(sql);
                        rtstr = maxValuesCatcher(da);
                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                            da = getDataSet(sql);
                            rtstr = maxValuesCatcher(da);
                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
                else
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = maxValuesCatcher(da);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";
        }

        public string RequestOValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            decimal duration = Math.Abs(RequestDuration(arg1, arg2) / 1000);


            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectAuraPeriod FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectAuraPeriod FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);

            DataSet da = getDataSet(sql);


            decimal timePeriod = 0;

            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal time = decimal.Parse(da.Tables[0].Rows[i]["m_effectAuraPeriod"].ToString());

                timePeriod = Math.Abs(time / 1000);
            }


            if (timePeriod == 0)
            {
                timePeriod = 5;
            }



            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";


            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";


            }

            //  Console.WriteLine(sql);



            // 
            da = getDataSet(sql);

            string rtstr = oValuesCatcher(da, duration, timePeriod);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + arg1 + ";";
                    da = getDataSet(sql);
                    rtstr = oValuesCatcher(da, duration, timePeriod);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        da = getDataSet(sql);
                        rtstr = oValuesCatcher(da, duration, timePeriod);
                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                            da = getDataSet(sql);
                            rtstr = oValuesCatcher(da, duration, timePeriod);
                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
                else
                {
                    sql = "SELECT dbc_Spelleffect.m_effectBasePoints,dbc_Spelleffect.m_effectBonusPoints,dbc_Spelleffect.m_effectBonusCoefficient,dbc_Spelleffect.m_bonusCoefficientFromAP,dbc_Spelleffect.m_effectType,dbc_Spelleffect.m_effectAuraType,dbc_Spellmisc.m_attributes9 FROM dbc_Spell,dbc_Spelleffect,dbc_Spellmisc where dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = oValuesCatcher(da, duration, timePeriod);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }




            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";
        }

        public string RequestBValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
                return "\r";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectPointsPerResource FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectPointsPerResource FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal b = decimal.Parse(da.Tables[0].Rows[i]["m_effectPointsPerResource"].ToString());



                return numberFormatCov(b);
            }


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestEValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectAmplitude FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectAmplitude FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);

            string key = "m_effectAmplitude";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelleffect.m_effectAmplitude FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + arg1 + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectAmplitude FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        da = getDataSet(sql);
                        rtstr = generalValuesCatcher(da, key);
                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectAmplitude FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                            da = getDataSet(sql);
                            rtstr = generalValuesCatcher(da, key);
                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
                else
                {
                    sql = "SELECT dbc_Spelleffect.m_effectAmplitude FROM dbc_Spelleffect,dbc_Spell where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }



            /* for (int i = 0; i < da.Tables[0].Rows.Count; i++)

             {
                 decimal e = decimal.Parse(da.Tables[0].Rows[i]["m_effectAmplitude"].ToString());



                 return numberFormatCov(e);
             }*/


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestHValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellauraoptions.m_procChance FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellauraoptions.m_procChance FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);


            string key = "m_procChance";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spellauraoptions.m_procChance FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    return rtstr;
                }
            }



            /* for (int i = 0; i < da.Tables[0].Rows.Count; i++)

             {
                 decimal h = decimal.Parse(da.Tables[0].Rows[i]["m_procChance"].ToString());



                 return numberFormatCov(h);
             }*/


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestIValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelltargetrestrictions.m_maxTargets FROM dbc_Spelltargetrestrictions where dbc_Spelltargetrestrictions.m_spellID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelltargetrestrictions.m_maxTargets FROM dbc_Spelltargetrestrictions where dbc_Spelltargetrestrictions.m_spellID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);

            string key = "m_maxTargets";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelltargetrestrictions.m_maxTargets FROM dbc_Spelltargetrestrictions where dbc_Spelltargetrestrictions.m_spellID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    return rtstr;
                }
            }


            /*   for (int i = 0; i < da.Tables[0].Rows.Count; i++)

               {
                   decimal I = decimal.Parse(da.Tables[0].Rows[i]["m_maxTargets"].ToString());



                   return numberFormatCov(I);
               }*/


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestNValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellauraoptions.m_stackAmount FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellauraoptions.m_stackAmount FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);


            string key = "m_stackAmount";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spellauraoptions.m_stackAmount FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    return rtstr;
                }
            }


            /* for (int i = 0; i < da.Tables[0].Rows.Count; i++)

             {
                 decimal n = decimal.Parse(da.Tables[0].Rows[i]["m_stackAmount"].ToString());



                 return numberFormatCov(n);
             }*/


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestQValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectBasePoints FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + 0 + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectBasePoints FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + 0 + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);

            DataSet da = getDataSet(sql);


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBasePoints"].ToString());
                // decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                // decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                //  decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                // int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                decimal resultBase = basePoints;


                return numberFormatCov(resultBase);

                //return (range).ToString();
            }

            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }


            return "0";
        }

        public string RequestUValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellauraoptions.m_procCharges FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellauraoptions.m_procCharges FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);


            string key = "m_procCharges";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spellauraoptions.m_procCharges FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    return rtstr;
                }
            }

            /* for (int i = 0; i < da.Tables[0].Rows.Count; i++)

             {
                 decimal u = decimal.Parse(da.Tables[0].Rows[i]["m_procCharges"].ToString());



                 return numberFormatCov(u);
             }
             */

            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestVValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelltargetrestrictions.m_maxTargetLevel FROM dbc_Spelltargetrestrictions where dbc_Spelltargetrestrictions.m_spellID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelltargetrestrictions.m_maxTargetLevel FROM dbc_Spelltargetrestrictions where dbc_Spelltargetrestrictions.m_spellID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);


            string key = "m_maxTargetLevel";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelltargetrestrictions.m_maxTargetLevel FROM dbc_Spelltargetrestrictions where dbc_Spelltargetrestrictions.m_spellID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    return rtstr;
                }
            }

            /* for (int i = 0; i < da.Tables[0].Rows.Count; i++)

             {
                 decimal v = decimal.Parse(da.Tables[0].Rows[i]["m_maxTargetLevel"].ToString());



                 return numberFormatCov(v);
             }
             */

            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestXValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";
            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spelleffect.m_effectChainTargets FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spelleffect.m_effectChainTargets FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);

            DataSet da = getDataSet(sql);

            string key = "m_effectChainTargets";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spelleffect.m_effectChainTargets FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + arg1 + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        sql = "SELECT dbc_Spelleffect.m_effectChainTargets FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =" + (int.Parse(arg2) - 1) + " and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                        da = getDataSet(sql);
                        rtstr = generalValuesCatcher(da, key);
                        if (!rtstr.Equals("NULL"))
                        {
                            return rtstr;
                        }
                        else
                        {
                            sql = "SELECT dbc_Spelleffect.m_effectChainTargets FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                            da = getDataSet(sql);
                            rtstr = generalValuesCatcher(da, key);
                            if (!rtstr.Equals("NULL"))
                            {
                                return rtstr;
                            }
                            else
                            {
                                return "0";
                            }
                        }
                    }
                }
                else
                {
                    sql = "SELECT dbc_Spelleffect.m_effectChainTargets FROM dbc_Spell,dbc_Spelleffect where dbc_Spelleffect.m_spellID=dbc_Spell.m_ID and dbc_Spelleffect.m_effectIndex =0 and dbc_Spell.m_ID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    if (!rtstr.Equals("NULL"))
                    {
                        return rtstr;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }



            /*   for (int i = 0; i < da.Tables[0].Rows.Count; i++)

               {
                   decimal basePoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectChainTargets"].ToString());
                   // decimal bonusPoints = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusPoints"].ToString());
                   // decimal bonusCoefficient = decimal.Parse(da.Tables[0].Rows[i]["m_effectBonusCoefficient"].ToString());
                   //  decimal bonusCoefficientFromAP = decimal.Parse(da.Tables[0].Rows[i]["m_bonusCoefficientFromAP"].ToString());

                   // int effectType = int.Parse(da.Tables[0].Rows[i]["m_effectType"].ToString());

                   decimal resultBase = basePoints;


                   return numberFormatCov(resultBase);

                   //return (range).ToString();
               }*/

            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }


            return "0";
        }

        public string RequestRPPMValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellprocsperminute.m_baseProcRate FROM dbc_Spellauraoptions,dbc_Spellprocsperminute where dbc_Spellauraoptions.m_spellProcsPerMinuteID=dbc_Spellprocsperminute.m_ID and dbc_Spellauraoptions.m_spellID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellprocsperminute.m_baseProcRate FROM dbc_Spellauraoptions,dbc_Spellprocsperminute where dbc_Spellauraoptions.m_spellProcsPerMinuteID=dbc_Spellprocsperminute.m_ID and dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
            }


            DataSet da = getDataSet(sql);


            string key = "m_baseProcRate";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spellprocsperminute.m_baseProcRate FROM dbc_Spellauraoptions,dbc_Spellprocsperminute where dbc_Spellauraoptions.m_spellProcsPerMinuteID=dbc_Spellprocsperminute.m_ID and dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    return rtstr;
                }
            }


            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string RequestICDValue(string arg1, string arg2)
        {
            string sql = "";

            //  Console.WriteLine("arg2:"+arg2);

            if (arg2.Length == 0)
            {
                arg2 = "1";

            }

            if (arg1.Length > 0)
            {
                //Console.WriteLine();

                sql = "SELECT dbc_Spellauraoptions.m_ICD FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + arg1 + ";";
            }
            else
            {
                sql = "SELECT dbc_Spellauraoptions.m_ICD FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
            }

            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);


            string key = "m_ICD";

            string rtstr = generalValuesCatcher(da, key);

            if (!rtstr.Equals("NULL"))
            {
                return rtstr;
            }
            else
            {
                if (arg1.Length > 0)
                {
                    sql = "SELECT dbc_Spellauraoptions.m_ICD FROM dbc_Spellauraoptions where dbc_Spellauraoptions.m_spellID =" + currentSpell.m_ID + ";";
                    da = getDataSet(sql);
                    rtstr = generalValuesCatcher(da, key);
                    return rtstr;
                }
            }

            /* for (int i = 0; i < da.Tables[0].Rows.Count; i++)

             {
                 decimal u = decimal.Parse(da.Tables[0].Rows[i]["m_procCharges"].ToString());



                 return numberFormatCov(u);
             }
             */

            if (arg1.Length > 0 && arg2 != "1") { missingList.Add(currentSpell.m_ID + ":" + currentSpell.m_name_lang + "|" + currentSpell.m_description_lang); }

            return "0";

        }

        public string ReplaceMultiArg(Match t)
        {
            string str = t.Value;

            //Console.WriteLine(t.Value);


            foreach (Regex rx in regexList)
            {

                str = rx.Replace(str, ReplaceArgs);

                //Console.WriteLine(str);
                //  Console.WriteLine(m.Groups[1].Value);

            }


            // str = rx1.Replace(str, ReplaceArg);

            return str;
        }

        List<Regex> regexList = new List<Regex>();

        public string ReplaceArg(Match t)
        {

            /* Regex durationRx = new Regex(@"[$]([0-9]*)[dD]([0-9]*)");
             Regex timePeriodRx = new Regex(@"[$]([0-9]*)[tTp]([0-9]*)");
             Regex rangeRx = new Regex(@"[$]([0-9]*)[rR]([0-9]*)");
             Regex radius1Rx = new Regex(@"[$]([0-9]*)[a]([0-9]*)");
             Regex radius2Rx = new Regex(@"[$]([0-9]*)[A]([0-9]*)");
             Regex svalueRx = new Regex(@"[$]([0-9]*)[sSwW]([0-9]*)");
             Regex swvalueRx = new Regex(@"[$]([0-9]*)[sS][wW]([0-9]*)");
             Regex ovalueRx = new Regex(@"[$]([0-9]*)[oO]([0-9]*)");
             Regex bvalueRx = new Regex(@"[$]([0-9]*)[bB]([0-9]*)");
             Regex minvalueRx = new Regex(@"[$]([0-9]*)[m]([0-9]*)");
             Regex maxvalueRx = new Regex(@"[$]([0-9]*)[M]([0-9]*)");
             Regex cvalueRx = new Regex(@"[$]([0-9]*)[c]([0-9]*)");
             Regex evalueRx = new Regex(@"[$]([0-9]*)[e]([0-9]*)");
             Regex hvalueRx = new Regex(@"[$]([0-9]*)[hH]([0-9]*)");
             Regex ivalueRx = new Regex(@"[$]([0-9]*)[iI]([0-9]*)");
             Regex nvalueRx = new Regex(@"[$]([0-9]*)[nN]([0-9]*)");*/

            //Console.WriteLine(t.Value);

            foreach (Match m in rppmvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestRPPMValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in icdvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(numberFormatCov((decimal.Parse(RequestICDValue(m.Groups[1].Value, m.Groups[2].Value)) / 1000)));
                //  Console.WriteLine(m.Groups[1].Value);

            }


            foreach (Match m in durationRx.Matches(t.Value))
            {


                return timeDurationFormatCov(RequestDuration(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in timePeriodRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestTimePeriod(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in rangeRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestRange(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in radius1Rx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestRadius1(m.Groups[1].Value, m.Groups[2].Value));


            }

            foreach (Match m in radius2Rx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestRadius2(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in swvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestSWValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in svalueRx.Matches(t.Value))
            {

                //Console.WriteLine(t.Value);
                return numberFormatABSCov(RequestSValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in ovalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestOValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }


            foreach (Match m in bvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestBValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in minvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestMinValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in maxvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestMaxValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }


            foreach (Match m in evalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestEValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }


            foreach (Match m in hvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestHValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in ivalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestIValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in nvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestNValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in qvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestQValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in uvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestUValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in vvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestVValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in xvalueRx.Matches(t.Value))
            {


                return numberFormatABSCov(RequestXValue(m.Groups[1].Value, m.Groups[2].Value));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match c in zvalueRx.Matches(t.Value))
            {


                return "<炉石绑定点>";
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match c in cvalueRx.Matches(t.Value))
            {


                return "0";
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match c in absRx.Matches(t.Value))
            {


                return "";
                //  Console.WriteLine(m.Groups[1].Value);

            }

            //  Console.WriteLine(t.Value);

            return t.Value;

        }

        public string ReplaceArgs(Match t)
        {

            /* Regex durationRx = new Regex(@"[$]([0-9]*)[dD]([0-9]*)");
             Regex timePeriodRx = new Regex(@"[$]([0-9]*)[tTp]([0-9]*)");
             Regex rangeRx = new Regex(@"[$]([0-9]*)[rR]([0-9]*)");
             Regex radius1Rx = new Regex(@"[$]([0-9]*)[a]([0-9]*)");
             Regex radius2Rx = new Regex(@"[$]([0-9]*)[A]([0-9]*)");
             Regex svalueRx = new Regex(@"[$]([0-9]*)[sSwW]([0-9]*)");
             Regex swvalueRx = new Regex(@"[$]([0-9]*)[sS][wW]([0-9]*)");
             Regex ovalueRx = new Regex(@"[$]([0-9]*)[oO]([0-9]*)");
             Regex bvalueRx = new Regex(@"[$]([0-9]*)[bB]([0-9]*)");
             Regex minvalueRx = new Regex(@"[$]([0-9]*)[m]([0-9]*)");
             Regex maxvalueRx = new Regex(@"[$]([0-9]*)[M]([0-9]*)");
             Regex cvalueRx = new Regex(@"[$]([0-9]*)[c]([0-9]*)");
             Regex evalueRx = new Regex(@"[$]([0-9]*)[e]([0-9]*)");
             Regex hvalueRx = new Regex(@"[$]([0-9]*)[hH]([0-9]*)");
             Regex ivalueRx = new Regex(@"[$]([0-9]*)[iI]([0-9]*)");
             Regex nvalueRx = new Regex(@"[$]([0-9]*)[nN]([0-9]*)");*/



            foreach (Match m in rppmvalueRx.Matches(t.Value))
            {


                return RequestRPPMValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in icdvalueRx.Matches(t.Value))
            {


                return numberFormatCov((decimal.Parse(RequestICDValue(m.Groups[1].Value, m.Groups[2].Value)) / 1000));
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in durationRx.Matches(t.Value))
            {


                return (RequestDuration(m.Groups[1].Value, m.Groups[2].Value) / 1000).ToString();
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in timePeriodRx.Matches(t.Value))
            {


                return RequestTimePeriod(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in rangeRx.Matches(t.Value))
            {


                return RequestRange(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in radius1Rx.Matches(t.Value))
            {

                // Console.WriteLine(t.Value);

                //  Console.WriteLine(m.Value);

                return RequestRadius1(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in radius2Rx.Matches(t.Value))
            {


                return RequestRadius2(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in swvalueRx.Matches(t.Value))
            {


                return RequestSWValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in svalueRx.Matches(t.Value))
            {


                return RequestSValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in ovalueRx.Matches(t.Value))
            {


                return RequestOValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }


            foreach (Match m in bvalueRx.Matches(t.Value))
            {


                return RequestBValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in minvalueRx.Matches(t.Value))
            {


                return RequestMinValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in maxvalueRx.Matches(t.Value))
            {


                return RequestMaxValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }


            foreach (Match m in evalueRx.Matches(t.Value))
            {


                return RequestEValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }


            foreach (Match m in hvalueRx.Matches(t.Value))
            {


                return RequestHValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in ivalueRx.Matches(t.Value))
            {


                return RequestIValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in nvalueRx.Matches(t.Value))
            {


                return RequestNValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in qvalueRx.Matches(t.Value))
            {


                return RequestQValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in uvalueRx.Matches(t.Value))
            {


                return RequestUValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in vvalueRx.Matches(t.Value))
            {


                return RequestVValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match m in xvalueRx.Matches(t.Value))
            {


                return RequestXValue(m.Groups[1].Value, m.Groups[2].Value);
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match c in zvalueRx.Matches(t.Value))
            {


                return "<炉石绑定点>";
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match c in cvalueRx.Matches(t.Value))
            {


                return "0";
                //  Console.WriteLine(m.Groups[1].Value);

            }

            foreach (Match c in absRx.Matches(t.Value))
            {


                return "";
                //  Console.WriteLine(m.Groups[1].Value);

            }
            //  Console.WriteLine(t.Value);

            return t.Value;

        }

        private Spell errorSpellHandler(Spell sp)
        {
            switch (sp.m_ID)
            {
                case 1706:
                    sp.m_description_lang = sp.m_description_lang.Replace("$?a193067[任何伤害都会中断漂浮效果]", "$?a193067[][任何伤害都会中断漂浮效果]");
                    return sp;
                case 1719:
                    sp.m_description_lang = sp.m_description_lang.Replace(@"施放战吼，爆击几率提高$s1，持续$d。$?a202751", @"施放战吼，爆击几率提高$s1%，持续$d。$?a202751");
                    return sp;
                case 5221:
                    sp.m_description_lang = sp.m_description_lang.Replace("$?s48484", "");
                    return sp;

                case 7302:
                    sp.m_description_lang = sp.m_description_lang.Replace("$$205708s2%", "$205708s2%");
                    return sp;

                case 11328:
                    sp.m_description_lang = sp.m_description_lang.Replace("$?", "");
                    return sp;
                case 33763:
                    if (sp.m_description_lang.Contains("$?s121840[]["))
                    {
                        sp.m_description_lang = sp.m_description_lang.Replace("对目标施放治疗之触或愈合时，会刷新持续时间。", "对目标施放治疗之触或愈合时，会刷新持续时间。]");
                    }

                    return sp;
                case 50842:
                    sp.m_description_lang = sp.m_description_lang.Replace("]。[]", "][。]");
                    return sp;

                case 85739:
                    sp.m_auraDescription_lang = sp.m_auraDescription_lang.Replace("怒击和$?a206312[和狂暴]命中", "怒击$?a206312[和狂暴][]命中");

                    return sp;
                case 109215:
                    sp.m_description_lang = sp.m_description_lang.Replace("$?s190925[鱼叉猛刺[逃脱]", "$?s190925[鱼叉猛刺][逃脱]");
                    return sp;
                case 193359:
                    sp.m_description_lang = sp.m_description_lang.Replace("提高{$m1 *$76806m2 / 100}%", "提高${$m1 *$76806m2 / 100}%");
                    return sp;
                case 196103:
                    sp.m_description_lang = sp.m_description_lang.Replace("腐蚀术变为永久效果，造成的伤害提高$s2，对玩家的持续时间缩短至$s1秒。", "腐蚀术变为永久效果，造成的伤害提高$s2%，对玩家的持续时间缩短至$s1秒。");
                    return sp;
                case 202033:
                    if (sp.m_auraDescription_lang.EndsWith("冷却时间大大缩短。"))
                    {
                        sp.m_auraDescription_lang = sp.m_auraDescription_lang + "][]";
                    }
                    return sp;
                case 186371:

                    sp.m_description_lang = sp.m_description_lang.Replace("$?s121840[][", "$?s121840[][]");

                    return sp;

                case 200174:
                    sp.m_description_lang = sp.m_description_lang.Replace("[${$m2/100}.2", "[${$m2/100}].2");
                    return sp;

                case 185452:
                case 190927:
                case 193722:
                case 212331:
                case 212353:
                    sp.m_description_lang = sp.m_description_lang.Replace("$?s58357。", "");
                    return sp;
                case 216255:
                    if (sp.m_description_lang.IndexOf("${($m1/1000)*-1)}") >= 0)
                    {
                        sp.m_description_lang = sp.m_description_lang.Replace("${($m1/1000)*-1)}", "${(($m1/1000)*-1)}");
                    }

                    return sp;
                default:

                    return sp;

            }

        }

        public string digitModifyReplace(string str)
        {

            str = str.Replace("}.1", "}");
            str = str.Replace("}.2", "}");

            return str;
        }

        public string desVarReplace(string str)
        {
            switch (currentSpell.m_ID)
            {
                case 2098:
                    if (currentSpell.m_spellDescriptionVariableID == 0)
                    {
                        currentSpell.m_spellDescriptionVariableID = 169;
                    }
                    break;
                case 79972:
                case 165745:
                    if (currentSpell.m_spellDescriptionVariableID == 0)
                    {
                        currentSpell.m_spellDescriptionVariableID = 204;
                    }
                    break;
                case 103840:
                    if (currentSpell.m_spellDescriptionVariableID == 0)
                    {
                        currentSpell.m_spellDescriptionVariableID = 262;
                    }
                    break;
                case 188820:
                    if (currentSpell.m_spellDescriptionVariableID == 0)
                    {
                        currentSpell.m_spellDescriptionVariableID = 275;
                    }
                    break;
                default:
                    break;

            }

            // Console.WriteLine(currentSpell.m_spellDescriptionVariableID);

            if (currentSpell.m_spellDescriptionVariableID != 0)
            {
                string sql = "SELECT dbc_Spelldescriptionvariables.m_DecriptionVariable FROM dbc_Spell,dbc_Spelldescriptionvariables where dbc_Spell.m_ID=" + currentSpell.m_ID + " and dbc_Spelldescriptionvariables.m_ID=" + currentSpell.m_spellDescriptionVariableID + ";";


                //   Console.WriteLine(sql);

                DataSet da = getDataSet(sql);



                string rep = str;

                for (int i = 0; i < da.Tables[0].Rows.Count; i++)

                {
                    string m_DecriptionVariable = da.Tables[0].Rows[i]["m_DecriptionVariable"].ToString();

                    List<string> templst = new List<string>();

                    StringReader sr = new StringReader(m_DecriptionVariable);

                    string temp = sr.ReadLine();



                    while (temp != null)
                    {
                        if (temp.Length < 1)
                        {
                            temp = sr.ReadLine();
                            continue;
                        }

                        if (temp.IndexOf("$mult=?s76803") >= 0 && currentSpell.m_spellDescriptionVariableID == 198)
                        {
                            temp = temp.Replace("$mult=?s76803", "$mult=$?s76803");
                        }


                        templst.Add(temp);

                        temp = sr.ReadLine();
                    }

                    foreach (var s in templst)
                    {
                        string[] t = s.Split('=');


                        str = str.Replace((t[0].Replace("$", "$<") + ">"), regexGlyph(t[1]));
                    }


                }

                if (rep.Equals(str))
                {
                    return rep;
                }
                else
                {

                    return desVarReplace(str);
                }

            }
            else
            {
                return str;
            }

        }


        public string specialCalReplace(string str)
        {

            str = str.Replace("$@versadmg", "0");



            str = divisionReg.Replace(str, ReplaceDivision);

            str = multiplicationReg.Replace(str, ReplaceDivision);

            return str;

        }

        public string ReplaceMultiplication(Match t)
        {
            return "${$" + t.Groups[1] + t.Groups[3] + "*" + t.Groups[2] + "}";
        }

        public string ReplaceDivision(Match t)
        {

            //  Console.WriteLine(t.Value);

            /* foreach (Group g in t.Groups)
             {
                 Console.WriteLine(g.Value);
             }

             string []temp = t.Value.Split(';');

             Console.WriteLine();
             */
            return "${$" + t.Groups[1] + t.Groups[3] + "/" + t.Groups[2] + "}";

        }

        public string ReplaceGarrBuilding(string arg)
        {

            string buildID = arg.Replace("$@garrbuilding", "");


            string sql = "SELECT dbc_Garrbuilding.m_buildingIcon FROM dbc_Garrbuilding where m_ID =" + buildID + ";";



            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);

            decimal m_buildingIcon = 0;


            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                m_buildingIcon = decimal.Parse(da.Tables[0].Rows[i]["m_buildingIcon"].ToString());

            }

            sql = "SELECT m_levelDescription,m_level FROM dbc_Garrbuilding where m_buildingIcon = " + m_buildingIcon + " order by m_level;";

            da = getDataSet(sql);

            string temp = "\r";

            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                string m_levelDescription = da.Tables[0].Rows[i]["m_levelDescription"].ToString();
                string m_level = da.Tables[0].Rows[i]["m_level"].ToString();

                temp += "\r等级" + m_level + "\r";
                temp += m_levelDescription;
                if (i < da.Tables[0].Rows.Count - 1)
                {
                    temp += "\r";
                }
            }

            return temp;

        }

        public string ReplaceSpellICON(string arg)
        {

            string spellID = arg.Replace("$@spellicon", "");


            string sql = "SELECT dbc_Spellicon.m_Name FROM dbc_Spell,dbc_Spellmisc,dbc_Spellicon where  dbc_Spell.m_spellMisc=dbc_SpellMisc.m_ID and dbc_Spellmisc.m_spellIconID=dbc_Spellicon.m_ID and dbc_Spell.m_ID=" + spellID + ";";



            //  Console.WriteLine(sql);


            DataSet da = getDataSet(sql);

            string filename = "";



            for (int i = 0; i < da.Tables[0].Rows.Count; i++)

            {
                filename = da.Tables[0].Rows[i]["m_Name"].ToString();


                return "<img src=\"" + imageURL + filename.Replace("\\", "/").ToLower() + ".png\" width=\"32px\" height=\"32px\" />";

            }


            return "<img src=\"" + imageURL + filename.Replace("\\", "/").ToLower() + ".png\" width=\"32px\" height=\"32px\" />";

        }

        public string ReplaceSpecialArgs(Match t)
        {
            if (t.Value.Equals("$AP") || t.Value.Equals("$ap"))
            {
                return "攻击强度";
            }
            else if (t.Value.Equals("$SP"))
            {
                return "法术强度";
            }
            else if (t.Value.Equals("$RAP"))
            {
                return "远程攻击强度";
            }
            else if (t.Value.Equals("$PL") || t.Value.Equals("$pl"))
            {
                return "角色等级";
            }
            else if (t.Value.Equals("$rw"))
            {
                return "远程武器最小伤害";
            }
            else if (t.Value.Equals("$RW"))
            {
                return "远程武器最大伤害";
            }
            else if (t.Value.Equals("$rwb"))
            {
                return "远程武器最小基础伤害";
            }
            else if (t.Value.Equals("$RWB"))
            {
                return "远程武器最大基础伤害";
            }
            else if (t.Value.Equals("$SPH"))
            {
                return "神圣法术强度";
            }
            else if (t.Value.Equals("$SPN"))
            {
                return "自然法术强度";
            }
            else if (t.Value.Equals("$STR"))
            {
                return "力量";
            }
            else if (t.Value.Equals("$SPFR"))
            {
                return "冰霜法术强度";
            }
            else if (t.Value.Equals("$SPFI"))
            {
                return "火焰法术强度";
            }
            else if (t.Value.Equals("$spells"))
            {
                return "$s";
            }
            else if (t.Value.Equals("$spells1"))
            {
                return "$s1";
            }
            else if (t.Value.Equals("$spelld"))
            {
                return "$d";
            }
            else if (t.Value.Equals("$PS"))
            {
                return "暗影法术强度";
            }
            else if (t.Value.Equals("$pctH"))
            {
                return "当前生命百分比";
            }
            else if (t.Value.Equals("$MHP"))
            {
                return "最大生命值";
            }
            else if (t.Value.Equals("$mw"))
            {
                return "主手武器最小伤害";
            }
            else if (t.Value.Equals("$MW"))
            {
                return "主手武器最大伤害";
            }
            else if (t.Value.Equals("$mwb"))
            {
                return "主手武器最小基础伤害";
            }
            else if (t.Value.Equals("$MWB"))
            {
                return "主手武器最大基础伤害";
            }
            else if (t.Value.Equals("$mws"))
            {
                return "主手武器最小秒伤";
            }
            else if (t.Value.Equals("$MWS"))
            {
                return "主手武器最大秒伤";
            }
            else if (t.Value.Equals("$owb"))
            {
                return "副手武器最小基础伤害";
            }
            else if (t.Value.Equals("$OWB"))
            {
                return "副手武器最大基础伤害";
            }
            else if (t.Value.Equals("$ows"))
            {
                return "副手武器最小秒伤";
            }
            else if (t.Value.Equals("$OWS"))
            {
                return "副手武器最大秒伤";
            }
            else if (t.Value.Equals("$hnd"))
            {
                return "持有武器数";
            }
            else if (t.Value.Equals("$spec"))
            {
                return "专精";
            }
            else if (t.Value.Equals("$@lootspec"))
            {
                return "拾取专精";
            }
            else if (t.Value.Equals("$@class"))
            {
                return "职业";
            }
            else if (t.Value.Equals("$@classspec"))
            {
                return "职业专精";
            }
            else if (t.Value.Equals("$j1g"))
            {
                return "60%/100";
            }
            else if (t.Value.Equals("$j1f"))
            {
                return "150%/280%/310";
            }
            else if (t.Value.Equals("$j1s"))
            {
                return "60%/300";

            }
            else if (t.Value.StartsWith("$@garrbuilding"))
            {
                return ReplaceGarrBuilding(t.Value);
            }
            else if (t.Value.StartsWith("$@spellicon"))
            {
                return ReplaceSpellICON(t.Value);

            }
            else if (t.Value.StartsWith("$2spelldesc"))
            {
                return t.Value.Replace("$2spelldesc", "$@spelldesc");
            }
            else if (t.Value.StartsWith("$spellaura"))
            {
                return t.Value.Replace("$spellaura", "$@spellaura");
            }
            else if (t.Value.StartsWith("$spelldesc"))
            {
                return t.Value.Replace("$spelldesc", "$@spelldesc");
            }
            else if (t.Value.StartsWith("$spellname"))
            {
                return t.Value.Replace("$spellname", "$@spellname");
            }
            else
            {
                return t.Value;
            }

        }

        public string ReplaceSpellDes(Match t)
        {
            if (t.Value.StartsWith("$@spellname"))
            {

                try
                {
                    int spellID = int.Parse(t.Value.Replace("$@spellname", ""));

                    //if (spellID==currentSpell.m_ID)
                    //{
                    //    Console.WriteLine(currentSpell.m_ID);
                    //    return "";
                    //}

                    Spell searchSpell = spellList.Where(c => c.m_ID == spellID).First();

                    return searchSpell.m_name_lang;
                }
                catch
                {
                    return t.Value;
                }

            }
            else if (t.Value.StartsWith("$@spelldesc"))
            {
                try
                {
                    int spellID = int.Parse(t.Value.Replace("$@spelldesc", ""));
                    if (spellID == currentSpell.m_ID)
                    {
                        // Console.WriteLine(currentSpell.m_ID);
                        return "";
                    }
                    Spell searchSpell = spellList.Where(c => c.m_ID == spellID).First();

                    return searchSpell.m_description_lang;
                }
                catch
                {
                    return t.Value;
                }

            }
            else if (t.Value.StartsWith("$@spelltooltip"))
            {
                try
                {
                    int spellID = int.Parse(t.Value.Replace("$@spelltooltip", ""));
                    //if (spellID == currentSpell.m_ID)
                    //{
                    //    Console.WriteLine(currentSpell.m_ID);
                    //    return "";
                    //}
                    Spell searchSpell = spellList.Where(c => c.m_ID == spellID).First();

                    string rtsrt = "\r" + searchSpell.m_name_lang + "\r" + searchSpell.m_description_lang;

                    return rtsrt;
                }
                catch
                {
                    return t.Value;
                }

            }
            else if (t.Value.StartsWith("$@spellaura") || t.Value.StartsWith("$@auradesc"))
            {
                try
                {
                    int spellID = int.Parse(t.Value.Replace("$@spellaura", "").Replace("$@auradesc", ""));
                    //if (spellID == currentSpell.m_ID)
                    //{
                    //    Console.WriteLine(currentSpell.m_ID);
                    //    return "";
                    //}
                    Spell searchSpell = spellList.Where(c => c.m_ID == spellID).First();

                    return searchSpell.m_auraDescription_lang;
                }
                catch
                {
                    return t.Value;
                }

            }
            else
            {
                return t.Value;
            }
        }

        public string recursionSpellDes(string str)
        {
            string temp = specialReg.Replace(str, ReplaceSpellDes);



            if (temp.Equals(str))
            {

                return temp;
            }
            else
            {
                return recursionSpellDes(temp);
            }

        }

        public string changeLine(string str)
        {
            return str.Trim().Replace("\n", "<br>").Replace("\r", "<br>");
        }

        public string ReplaceColorRx(Match t)
        {

            return "<span style=\"color: #" + t.Groups[2].Value + "\">" + t.Groups[3].Value + "</span>";
        }

        public string ReplaceTiconRx(Match t)
        {


            return "<img src=\"" + imageURL + t.Groups[1].Value.Replace("\\", "/").ToLower().Replace(".blp", "") + ".png\" width=\"32px\" height=\"32px\" />";
        }


        public string colorReplace(string str)
        {

            return ticonRx.Replace(colorRx.Replace(str, ReplaceColorRx), ReplaceTiconRx);

        }


        public void replaceAll()
        {


            readallspell();

            int total = spellList.Count;



            for (int i = 0; i < spellList.Count; i++)
            {
                Spell s = errorSpellHandler(spellList[i]);

                currentSpell = s;


                s.m_description_lang = specialReg.Replace(regexArgs(specialCalReplace(digitModifyReplace(regexMutilGen(regexGlyph(desVarReplace(s.m_description_lang)))))), ReplaceSpecialArgs);



                s.m_auraDescription_lang = specialReg.Replace(regexArgs(specialCalReplace(digitModifyReplace(regexMutilGen(regexGlyph(desVarReplace(s.m_auraDescription_lang)))))), ReplaceSpecialArgs);

                //Console.WriteLine(i);

                spellList[i] = s;


            }


            for (int i = 0; i < spellList.Count; i++)
            {
                Spell s = spellList[i];

                currentSpell = s;


                string des = specialReg.Replace(regexArgs(specialCalReplace(digitModifyReplace(regexMutilGen(regexGlyph(s.m_description_lang))))), ReplaceSpecialArgs);




                // s.m_description_lang =  recursionSpellDes(des);

                s.m_description_lang = colorReplace(changeLine(recursionSpellDes(des)));

                foreach (Match m in specialReg.Matches(s.m_description_lang))
                {
                    argsList.Add(m.Value);
                }

                string aura = specialReg.Replace(regexArgs(specialCalReplace(digitModifyReplace(regexMutilGen(regexGlyph(s.m_auraDescription_lang))))), ReplaceSpecialArgs);


                //  s.m_auraDescription_lang = recursionSpellDes(aura);

                s.m_auraDescription_lang = colorReplace(changeLine(recursionSpellDes(aura)));

                foreach (Match m in specialReg.Matches(s.m_auraDescription_lang))
                {
                    argsList.Add(m.Value);
                }

                argReplace.Add(currentSpell.m_ID + ":" + s.m_description_lang);
                argReplace.Add(currentSpell.m_ID + ":" + s.m_auraDescription_lang);

                spellList[i] = s;


            }


        }

        public void SaveToSQL(string fname)
        {


            PluginInterface.DataTable m_dataTable = new PluginInterface.DataTable(fname);
            m_dataTable.Locale = CultureInfo.InvariantCulture;

            m_dataTable.Columns.Add("m_ID", typeof(uint));
            m_dataTable.Columns.Add("m_name_lang", typeof(string));
            m_dataTable.Columns.Add("m_nameSubtext_lang", typeof(string));
            m_dataTable.Columns.Add("m_description_lang", typeof(string));
            m_dataTable.Columns.Add("m_auraDescription_lang", typeof(string));
            m_dataTable.Columns.Add("m_spellMisc", typeof(uint));
            m_dataTable.Columns.Add("m_spellDescriptionVariableID", typeof(uint));
            //   Console.WriteLine(m_dbreader.RecordsCount);


            for (int i = 0; i < spellList.Count; ++i) // Add rows
            {
                DataRow dataRow = m_dataTable.NewRow();

                //    Console.WriteLine(m_dbreader.RecordsCount);
                //  Console.WriteLine(m_dbreader[i].BaseStream.Length);
                //dataRow.BeginEdit();
                dataRow[0] = spellList[i].m_ID;
                dataRow[1] = spellList[i].m_name_lang;
                dataRow[2] = spellList[i].m_nameSubtext_lang;
                dataRow[3] = spellList[i].m_description_lang;
                dataRow[4] = spellList[i].m_auraDescription_lang;
                dataRow[5] = spellList[i].m_spellMisc;
                dataRow[6] = spellList[i].m_spellDescriptionVariableID;

                m_dataTable.Rows.Add(dataRow);

                //int percent = (int)((float)m_dataTable.Rows.Count / (float)m_dbreader.RecordsCount * 100.0f);
                //(sender as BackgroundWorker).ReportProgress(percent);
            }

            var columns = new DataColumn[1];
            var columns_2 = new DataColumn[1];

            columns[0] = m_dataTable.Columns["m_ID"];
            columns_2[0] = m_dataTable.Columns["m_spellMisc"];

            m_dataTable.PrimaryKey = columns;

            m_dataTable.NonUniqueIndexs = columns_2;


            //if (extraData)
            //{
            //    MessageBox.Show("extra data detected!");
            //}

            Export2SQL tosql = new Export2SQL();
            tosql.Run(m_dataTable);


        }








    }
}
