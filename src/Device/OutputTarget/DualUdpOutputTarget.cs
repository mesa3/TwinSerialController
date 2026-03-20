using SimpleJSON;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.Device.OutputTarget
{
    public class DualUdpOutputTarget : IOutputTarget
    {
        private UITextInput AddressInputA;
        private UITextInput PortInputA;
        private JSONStorableString IpTextA;
        private JSONStorableString PortTextA;

        private UITextInput AddressInputB;
        private UITextInput PortInputB;
        private JSONStorableString IpTextB;
        private JSONStorableString PortTextB;

        private UIHorizontalGroup ButtonGroup;

        private JSONStorableAction StartUdpAction;
        private JSONStorableAction StopUdpAction;

        private UdpClient _clientA;
        private UdpClient _clientB;

        public void CreateUI(IUIBuilder builder)
        {
            AddressInputA = builder.CreateTextInput("OutputTarget:DualUdp:AddressA", "Device A Address:", "tcode.local", 50);
            PortInputA = builder.CreateTextInput("OutputTarget:DualUdp:PortA", "Device A Port:", "8000", 50);
            IpTextA = AddressInputA.storable;
            PortTextA = PortInputA.storable;

            AddressInputB = builder.CreateTextInput("OutputTarget:DualUdp:AddressB", "Device B Address:", "tcode2.local", 50);
            PortInputB = builder.CreateTextInput("OutputTarget:DualUdp:PortB", "Device B Port:", "8000", 50);
            IpTextB = AddressInputB.storable;
            PortTextB = PortInputB.storable;

            ButtonGroup = builder.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => builder.CreateButtonEx());
            var startSerialButton = ButtonGroup.items[0].GetComponent<UIDynamicButton>();
            startSerialButton.label = "Start Dual Udp";
            startSerialButton.button.onClick.AddListener(StartUdp);

            var stopSerialButton = ButtonGroup.items[1].GetComponent<UIDynamicButton>();
            stopSerialButton.label = "Stop Dual Udp";
            stopSerialButton.button.onClick.AddListener(StopUdp);

            StartUdpAction = UIManager.CreateAction("Start Dual Udp", StartUdp);
            StopUdpAction = UIManager.CreateAction("Stop Dual Udp", StopUdp);
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(AddressInputA);
            builder.Destroy(PortInputA);
            builder.Destroy(AddressInputB);
            builder.Destroy(PortInputB);
            builder.Destroy(ButtonGroup);

            UIManager.RemoveAction(StartUdpAction);
            UIManager.RemoveAction(StopUdpAction);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(IpTextA);
            config.Restore(PortTextA);
            config.Restore(IpTextB);
            config.Restore(PortTextB);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(IpTextA);
            config.Store(PortTextA);
            config.Store(IpTextB);
            config.Store(PortTextB);
        }

        private void StartUdp()
        {
            if (_clientA != null || _clientB != null)
                return;

            try
            {
                _clientA = CreateClient(IpTextA.val, int.Parse(PortTextA.val));
                SuperController.LogMessage($"Dual Upd A started on port: {PortTextA.val}");

                _clientB = CreateClient(IpTextB.val, int.Parse(PortTextB.val));
                SuperController.LogMessage($"Dual Upd B started on port: {PortTextB.val}");
            }
            catch (Exception e)
            {
                SuperController.LogError(e.ToString());
                StopUdp();
            }
        }

        private UdpClient CreateClient(string ip, int port)
        {
            var client = new UdpClient
            {
                ExclusiveAddressUse = false
            };

            client.Client.Blocking = false;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            var ipAddress = default(IPAddress);
            if (IPAddress.TryParse(ip, out ipAddress))
                client.Connect(ipAddress, port);
            else
                client.Connect(ip, port);

            return client;
        }

        private void StopUdp()
        {
            try
            {
                if (_clientA != null) _clientA.Close();
                if (_clientB != null) _clientB.Close();
            }
            catch(Exception e)
            {
                SuperController.LogError(e.ToString());
            }

            SuperController.LogMessage("Dual Upd stopped");
            _clientA = null;
            _clientB = null;
        }

        public void Write(string data)
        {
            // If data contains special delimiters for Device A and Device B, we should split it here.
            // A simple protocol could be "A:L050\nB:L050" or separated by a specific character like '|'.
            // Here, we split by '|'. Device A data is on the left, Device B data is on the right.
            var parts = data.Split('|');

            if (parts.Length == 2)
            {
                if (_clientA != null && !string.IsNullOrEmpty(parts[0]))
                {
                    var bytesA = Encoding.ASCII.GetBytes(parts[0]);
                    _clientA.Send(bytesA, bytesA.Length);
                }

                if (_clientB != null && !string.IsNullOrEmpty(parts[1]))
                {
                    var bytesB = Encoding.ASCII.GetBytes(parts[1]);
                    _clientB.Send(bytesB, bytesB.Length);
                }
            }
            else
            {
                // Fallback: send same data to both if not splittable.
                var bytes = Encoding.ASCII.GetBytes(data);
                if (_clientA != null) _clientA.Send(bytes, bytes.Length);
                if (_clientB != null) _clientB.Send(bytes, bytes.Length);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            StopUdp();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
