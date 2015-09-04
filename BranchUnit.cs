using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	enum BranchOperations
	{
		Nop,
		Jump			= 1 << 16,
		JumpNotEqual	= 2 << 16,
		Break			= 3 << 16
	}

	class BranchUnit
	{
		CPUCore m_CPUCore;
		int[] m_currentOp;
		bool m_hasInstruction;

		public BranchUnit(CPUCore cPUCore)
		{
			m_CPUCore = cPUCore;
		}

		public void Tick()
		{
			if (m_CPUCore.m_currentStage == PipelineStages.BranchPredict)
			{
				if (m_hasInstruction)
				{
					switch ((BranchOperations)(m_currentOp[0] & 0x00ff0000))
					{
						case BranchOperations.Nop:
							{
								m_CPUCore.m_instructionPointer += 2;
							} break;
						case BranchOperations.Jump:
							{
								m_CPUCore.m_instructionPointer = (uint)m_currentOp[1];
								m_hasInstruction = false;
							} break;
						case BranchOperations.JumpNotEqual:
							{
								int register1 = (m_currentOp[0] >> 8) & 0x000000ff;
								int register2 = m_currentOp[0] & 0x000000ff;

								if(m_CPUCore.m_registers[register1] == m_CPUCore.m_registers[register2])
								{
									m_CPUCore.m_instructionPointer += 2;
								}
								else
								{
									m_CPUCore.m_instructionPointer = (uint)m_currentOp[1];
								}
								m_hasInstruction = false;
							}break;
						case BranchOperations.Break:
							{
								System.Diagnostics.Debugger.Break();
								m_CPUCore.m_instructionPointer += 2;
								m_hasInstruction = false;
							}break;
					}
				}
				else
				{
					m_CPUCore.m_instructionPointer += 2;
				}
				m_CPUCore.m_nextStage = PipelineStages.InstructionFetch;
			}
		}

		public void SetInstruction(int[] instruction)
		{
			m_currentOp = instruction;
			m_hasInstruction = true;
		}
	}
}
