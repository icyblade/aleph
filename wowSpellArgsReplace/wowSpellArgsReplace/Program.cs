using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wowSpellArgsReplace
{
    static class Program
    {

        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                repSpell rps = new repSpell(args[0]);

                rps.replaceAll();

                rps.SaveToSQL("rep_Spell");


            }

        }

    }
}
