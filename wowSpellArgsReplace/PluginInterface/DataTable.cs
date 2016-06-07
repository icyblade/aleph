using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace PluginInterface
{
    public class DataTable:System.Data.DataTable
    {
        protected DataColumn[] nui;

        public DataColumn[] NonUniqueIndexs {


            get
            {

                return this.nui;
            }

            set {

                

                int count = 0;

                foreach (DataColumn dc in value)
                {
                    if (dc!=null)
                    {
                        count++;
                    }
                }
                

                DataColumn[] da = new DataColumn[count];


                count = 0;
                for (int i=0;i<value.Length;i++)
                {
                    if (value[i]!=null)
                    {
                        da[count] = value[i];
                        count++;
                    }
                }

                this.nui = da;
            }
        }

        public DataTable(string tablename)
        {
            this.TableName = tablename;
        }

    }
}
