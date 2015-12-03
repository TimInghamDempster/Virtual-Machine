using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	class BlockDevice
	{
		InterconnectTerminal m_SystemInterconnect;
		int m_size;
		int m_address;

		public BlockDevice(int address, InterconnectTerminal systemInterconnect, string backingFilename, int size)
		{
		}

		public void Tick()
		{
		}
	}
}
