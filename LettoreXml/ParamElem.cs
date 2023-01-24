using System;
using System.Collections.Generic;
using System.Text;

namespace LettoreXml
{
    internal class ParamElem : BasicElem
    {
        public string value { get; set; }
        public ParamElem(string name, string value) : base(name)
        {
            this.value = value;
        }
    }
}
