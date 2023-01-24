using System;
using System.Collections.Generic;
using System.Text;

namespace LettoreXml
{
    internal class GroupElem : BasicElem
    {
        public GroupElem inngerGrooup;
        public List<ParamElem> paramElems = new List<ParamElem>();
    }
}
