using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class Bus : IIODevice
    {
        List<IIODevice> m_devices;
        List<uint> m_pages;

        public Bus()
        {
            m_devices = new List<IIODevice>();
            m_pages = new List<uint>();
        }

        IIODevice GetDeviceAtAddress(uint address)
        {
            IIODevice device = m_devices[0];
            int current = 0;
            bool found = false;
            while (!found)
            {
                current++;
                if (m_pages.Count <= current || m_pages[current] > address)
                {
                    break;
                }
                device = m_devices[current];
            }
            return device;
        }

        public int Read(uint address)
        {
            IIODevice device = GetDeviceAtAddress(address);
            return device.Read(address);
        }

        public void Write(int value, uint address)
        {
            IIODevice device = GetDeviceAtAddress(address);
            device.Write(value, address);
        }

        public void Add(IIODevice device, uint address)
        {
            m_devices.Add(device);
            m_pages.Add(address);
        }
    }
}
