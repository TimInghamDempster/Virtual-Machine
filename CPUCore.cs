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

    enum ExecutionUnitCodes
    {
        SimpleALU,
        ComplexALU,
        Load,
        Store
    }

    class CPUCore
    {
        public uint m_instructionPointer;
        
        CPU m_cpu;
        
        public int[] m_registers;

        public PipelineStages m_currentStage;

        CoreMemeoryController m_memoryController;
        BranchUnit m_branchUnit;
        InstructionFetchUnit m_fetchUnit;
        InstructionDispatchUnit m_dispatchUnit;
        ArithmeticLogicUnit m_simpleALU;
        ArithmeticLogicUnit m_complexALU;
        LoadUnit m_loadUnit;
        StoreUnit m_storeUnit;
        RetireUnit m_retireUnit;



        public CPUCore(CPU cpu)
        {
            m_cpu = cpu;
            m_instructionPointer = Program.biosStartAddress;
            m_registers = new int[8];
            m_currentStage = PipelineStages.InstructionFetch;

            m_memoryController = new CoreMemeoryController(m_cpu.m_northbridge);
            m_retireUnit = new RetireUnit(this);
            m_simpleALU = new ArithmeticLogicUnit(false, this);
            m_complexALU = new ArithmeticLogicUnit(true, this);
            m_loadUnit = new LoadUnit(this);
            m_storeUnit = new StoreUnit(this, m_memoryController);
            m_dispatchUnit = new InstructionDispatchUnit(this, m_simpleALU, m_complexALU, m_loadUnit, m_storeUnit);
            m_fetchUnit = new InstructionFetchUnit(this, m_memoryController, m_dispatchUnit);
            m_branchUnit = new BranchUnit(this);
        }

        public void Tick()
        {
            m_memoryController.Tick();
            switch (m_currentStage)
            {
                case PipelineStages.BranchPredict:
                    m_branchUnit.Tick();
                    break;
                case PipelineStages.InstructionFetch:
                    m_fetchUnit.Tick();
                    break;
                case PipelineStages.InstructionDispatch:
                    m_dispatchUnit.Tick();
                    break;
                case PipelineStages.Execution:
                    m_simpleALU.Tick();
                    m_complexALU.Tick();
                    m_loadUnit.Tick();
                    m_storeUnit.Tick();
                    break;
                case PipelineStages.Retirement:
                    m_retireUnit.Tick();
                    break;
            }
        }
    }
}
