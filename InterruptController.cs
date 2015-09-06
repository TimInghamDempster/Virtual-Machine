using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	enum InterruptInstructions
	{
		SetInterrupt,
		CallInterrupt = 1 << 16,
		InterruptReturn = 2 << 16
	}

	class InterruptController
	{
		private List<CPUCore> m_CPUCores;
		List<Action<uint>> m_setIPList;
		public List<uint> m_interruptVector;
		private uint m_interruptNumber;
		private bool m_interrupt;

		public InterruptController()
		{
			m_CPUCores = new List<CPUCore>();
			m_setIPList = new List<Action<uint>>();
			m_interruptVector = new List<uint>();
		}

		public void AddCore(CPUCore core, Action<uint> setInstructionPointer)
		{
			m_CPUCores.Add(core);
			m_setIPList.Add(setInstructionPointer);
		}

		public void SetInstruction(int[] instruction)
		{
			switch ((InterruptInstructions)(instruction[0] & 0x00ff0000))
			{
				case InterruptInstructions.SetInterrupt:
				{
					uint interruptNumber = (uint)instruction[0] & 0xff;
					while(m_interruptVector.Count <= interruptNumber)
					{
						m_interruptVector.Add(0);
					}
					m_interruptVector[(int)interruptNumber] = (uint)instruction[1];
				}break;
				case InterruptInstructions.InterruptReturn:
				{
					m_setIPList[0](m_CPUCores[0].m_storedIPointer);
				}break;
			}
		}

		public void Interrupt(uint interruptNumber)
		{
			m_interrupt = true;
			m_interruptNumber = interruptNumber;
		}

		internal void Tick()
		{
			// We tick before everything else so safe to jump in first.  Will be a minefield
			// when pipelining.
			if(m_CPUCores[0].CurrentStage == PipelineStages.InstructionFetch)
			{
				if (m_interrupt && m_interruptNumber < m_interruptVector.Count)
				{
					m_CPUCores[0].m_storedIPointer = m_CPUCores[0].InstructionPointer;
					m_setIPList[0](m_interruptVector[(int)m_interruptNumber]);
					m_interrupt = false;
				}
			}
		}
	}
}
