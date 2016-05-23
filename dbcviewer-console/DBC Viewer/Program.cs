using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DBCViewer
{
    class Program
    {


        static void Main(string[] args)
        {
            
            if (args.Length > 0)
            {
                DBViewer dbviwer = new DBViewer();

                dbviwer.Export(args[0]);
            }

        }



    }
}
