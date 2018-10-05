using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace WpfApp1
{



    public class Record
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Zp { get; set; }
        public string Comp { get; set; }
        public string Town { get; set; }
        public string Resp1 { get; set; }
        public string Req1 { get; set; }
        public string Dat { get; set; }
        public string Opt { get; set; }
        public StringBuilder Desc { get; set; } = new StringBuilder();
        public bool Sharp { get; set; }
        public bool JavaScript { get; set; }
        public bool Distant { get; set; }

        public string AllInfo() => Name + Zp + Comp + Town + Resp1 + Req1 + Dat + Opt + Desc.ToString();
    }

    public class q
    {
        public string Name { get; set; }
        public int count { get; set; }
        public q(string s)
        { Name = s; count = 0; }

        public string NameRus() => Name.Replace("C", "С");
    }

}