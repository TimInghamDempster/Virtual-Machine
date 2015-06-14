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
        List<uint> m_addresses;

        public Bus()
        {
            m_devices = new List<IIODevice>();
            m_addresses = new List<uint>();
        }

        IIODevice GetDeviceAtAddress(uint address)
        {
            IIODevice device = m_devices[0];
            int current = 0;
            bool found = false;
            while (!found)
            {
                current++;
                if (m_addresses.Count <= current || m_addresses[current] > address)
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
            m_addresses.Add(address);

            for (int i = m_addresses.Count - 1; i > 0; i--)
            {
                uint first = m_addresses[i - 1];
                uint second = m_addresses[i];

                if (first > second)
                {
                    uint tempAddrres = m_addresses[i - 1];
                    m_addresses[i - 1] = m_addresses[i];
                    m_addresses[i] = tempAddrres;

                    IIODevice tempDevice = m_devices[i - 1];
                    m_devices[i - 1] = m_devices[i];
                    m_devices[i] = tempDevice;
                }
            }
        }
    }
}
