using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum PipelineStages
    {
        BranchPredict,
        InstructionFetch,
        InstructionDispatch,
        Execution,
        Retirement
    }

    enum UnitCodes
    {
        Nop,
        ALU			= 1 << 24,
        Load		= 2 << 24,
        Store		= 3 << 24,
		Branch		= 4 << 24,
		Fetch		= 5 << 24,
		Interrupt	= 6 << 24
    }

    class CPUCore
    {
        uint m_instructionPointer;
		uint m_storedInstructionPointer;
        
        int[] m_registers;

		uint m_coreId;

		bool m_interruptWaiting;
		bool m_interrupted;

        PipelineStages m_currentStage;
        PipelineStages m_nextStage;

        InterconnectTerminal m_IOInterconnect;
        BranchUnit m_branchUnit;
        InstructionFetchUnit m_fetchUnit;
        InstructionDispatchUnit m_dispatchUnit;
        ArithmeticLogicUnit m_ALU;
        LoadUnit m_loadUnit;
        StoreUnit m_storeUnit;
        RetireUnit m_retireUnit;

		public PipelineStages CurrentStage { get { return m_currentStage; } }
		public PipelineStages NextStage { set { m_nextStage = value; } }
		public uint InstructionPointer { get { return m_instructionPointer; } }



        public CPUCore(InterconnectTerminal IOInterconnect, uint id, InterruptController interruptController)
		{
			m_coreId = id;
            m_instructionPointer = Program.biosStartAddress;
			interruptController.AddCore(Interrupt);
            m_registers = new int[10];
            m_currentStage = PipelineStages.InstructionFetch;
            m_nextStage = PipelineStages.InstructionFetch;
            m_IOInterconnect = IOInterconnect;
            m_retireUnit = new RetireUnit(this);
            m_ALU = new ArithmeticLogicUnit(this, m_registers);
            m_loadUnit = new LoadUnit(this, m_IOInterconnect, m_registers);
            m_storeUnit = new StoreUnit(this, IOInterconnect, m_registers);
            m_branchUnit = new BranchUnit(this, m_registers, (uint ip) => m_instructionPointer = ip );
            m_dispatchUnit = new InstructionDispatchUnit(this, m_branchUnit, m_ALU, m_loadUnit, m_storeUnit);
            m_fetchUnit = new InstructionFetchUnit(this, IOInterconnect, m_dispatchUnit, EndInterrupt);
        }

		void Interrupt()
		{
			m_interruptWaiting = true;
		}

		void EndInterrupt()
		{
			m_interrupted = false;
			m_instructionPointer = m_storedInstructionPointer;
			m_nextStage = PipelineStages.InstructionFetch;
		}

        public void Tick()
        {

            m_branchUnit.Tick();
            m_fetchUnit.Tick();
            m_dispatchUnit.Tick();
            m_ALU.Tick();
            m_loadUnit.Tick();
            m_storeUnit.Tick();
            m_retireUnit.Tick();

			// Only safe time to do this is right before a fetch
			if (m_interruptWaiting &&
			m_currentStage != PipelineStages.InstructionFetch && m_nextStage == PipelineStages.InstructionFetch
				&& !m_interrupted)
			{
				m_storedInstructionPointer = m_instructionPointer;
				m_fetchUnit.DoInterrupt();

				m_interruptWaiting = false;
				m_interrupted = true;
			}

            m_currentStage = m_nextStage;
        }
    }
}
