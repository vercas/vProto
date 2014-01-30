using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Test_Shizzle
{
    public interface RMI_Interface
    {
        int Test(string asd);
        void Fail();
        bool Blah(string asd, out int lawl);
    }

    public class RMI_Object
        : RMI_Interface
    {
        public int Test(string asd)
        {
            Console.WriteLine("Got the string {0}", asd);

            return ~asd.Length;
        }

        public void Fail()
        {
            throw new AccessViolationException("*frowny face*");
        }

        public bool Blah(string asd, out int lawl)
        {
            return int.TryParse(asd, out lawl);
        }
    }
}
