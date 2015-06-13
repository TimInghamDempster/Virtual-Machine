using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    interface IIODevice
    {
        int Read(uint address);
        void Write(int value, uint address);
    }
}
