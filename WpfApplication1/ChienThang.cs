using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class cls5OWin
    {
        //5 ô liên tiếp
        public Node[] GiaTri;
        int thuTu;
        public cls5OWin()
        {
            GiaTri = new Node[10];
            for (int i = 0; i < 10; i++)
            {
                GiaTri[i] = new Node();
            }
            thuTu = 0;
        }
        public void Add(Node n)
        {
            GiaTri[thuTu] = n;
            thuTu++;
        }
        public void Reset()
        {
            thuTu = 0;
            GiaTri = new Node[10];
            for (int i = 0; i < 10; i++)
            {
                GiaTri[i] = new Node();
            }
        }
    }
}
