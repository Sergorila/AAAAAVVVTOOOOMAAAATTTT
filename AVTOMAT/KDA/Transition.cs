using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDA
{
    public class Transition
    {
        public string StartState { get; set; }
        //конечное состояние
        public string EndState { get; set; }
        //условие перехода 
        public string Condition { get; set; }
    }
}
