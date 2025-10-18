using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TeXiuSi.Model
{
    public class Sports
    {
        public Sports() { }

        public Sports(string name) { }

        public string Name { get; set; }

        public string Description { get; set; }

        public SportType Type {  get; set; }


    }
}
