using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	class InstructionDispatchUnit
	{
		CPUCore m_CPUCore;

		BranchUnit m_branchUnit;
		ArithmeticLogicUnit m_simpleALU;
		ArithmeticLogicUnit m_complexALU;
		LoadUnit m_loadUnit;
		StoreUnit m_storeUnit;

		int[] m_currentInstruction;

		public InstructionDispatchUnit(CPUCore cPUCore,
			BranchUnit branchUnit,
			ArithmeticLogicUnit simpleALU,
			ArithmeticLogicUnit complexALU,
			LoadUnit loadUnit,
			StoreUnit storeUnit)
		{
			m_CPUCore = cPUCore;
			m_branchUnit = branchUnit;
			m_complexALU = complexALU;
			m_simpleALU = simpleALU;
			m_loadUnit = loadUnit;
			m_storeUnit = storeUnit;
		}

		public void Tick()
		{
			if (m_CPUCore.m_currentStage == PipelineStages.InstructionDispatch)
			{
				ExecutionUnitCodes executionUnitCode = (ExecutionUnitCodes)(m_currentInstruction[0] & 0xff000000);
				switch (executionUnitCode)
				{
					case ExecutionUnitCodes.ALU:
						{
							m_simpleALU.SetInstruction(m_currentInstruction);
							m_CPUCore.m_nextStage = PipelineStages.Execution;
						} break;
					case ExecutionUnitCodes.Load:
						{
							m_loadUnit.SetInstruction(m_currentInstruction);
							m_CPUCore.m_nextStage = PipelineStages.Execution;
						} break;
					case ExecutionUnitCodes.Store:
						{
							m_storeUnit.SetInstruction(m_currentInstruction);
							m_CPUCore.m_nextStage = PipelineStages.Execution;
						} break;
					case ExecutionUnitCodes.Branch:
						{
							m_branchUnit.SetInstruction(m_currentInstruction);
							m_CPUCore.m_nextStage = PipelineStages.BranchPredict;
						} break;
					case ExecutionUnitCodes.Interrupt:
						{
							m_CPUCore.m_interruptController.SetInstruction(m_currentInstruction);
							m_CPUCore.m_nextStage = PipelineStages.BranchPredict;
						}break;
				}
			}
		}

		public void SetInstruction(int[] instruction)
		{
			m_currentInstruction = instruction;
		}
	}
}
