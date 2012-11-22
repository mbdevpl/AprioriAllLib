using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib
{
    public class Litemset
    {
        public int Support;
        public List<Item> Items; // litemset

        public Litemset()
        {
        }

        public Litemset(List<Item> items)
        {
            Items = items;
        }
    }
}
