using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EthernetSwitch.Models.SNMP;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Microsoft.AspNetCore.Mvc;

namespace EthernetSwitch.Controllers
{
    public class SNMPController : Controller
    {
        public IActionResult GetSNMPv3()
        {
            return View(new GetSNMPv3ViewModel { IpAddress = HttpContext.Connection.LocalIpAddress.ToString() });
        }

        public IActionResult WalkSNMPv1()
        {
            return View(new WalkSNMPv1ViewModel { IpAddress = HttpContext.Connection.LocalIpAddress.ToString() });
        }

        [HttpPost]
        public IActionResult GetSNMPv1(WalkSNMPv1ViewModel viewModel)
        {
            IList<Variable> result = new List<Variable>();
            viewModel.Error = "";

            try
            {
                Messenger.Walk(VersionCode.V1,
                    new IPEndPoint(IPAddress.Parse(viewModel.IpAddress), viewModel.Port),
                    new OctetString(viewModel.Group),
                    new ObjectIdentifier(viewModel.StartObjectId),
                    result,
                    10000,
                    WalkMode.WithinSubtree);
            }
            catch (System.Exception e)
            {
                viewModel.Error = e.Message;
            }

            viewModel.OIDs = result
                .Select(variable => new OID { Id = variable.Id.ToString(), Value = variable.Data.ToString() })
                .ToList();

            return View("WalkSNMPv1", viewModel);
        }

        [HttpPost]
        public IActionResult GetSNMPv3(GetSNMPv3ViewModel viewModel)
        {
            var ipAddress = IPAddress.Parse(viewModel.IpAddress);
            viewModel.Error = "";

            try
            {
                Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                ReportMessage report = discovery.GetResponse(10000, new IPEndPoint(ipAddress, viewModel.Port));

                GetRequestMessage request = new GetRequestMessage(
                    VersionCode.V3,
                    Messenger.NextMessageId,
                    Messenger.NextRequestId,
                    new OctetString(viewModel.UserName),
                    new List<Variable> { new Variable(new ObjectIdentifier(viewModel.OID.Id)) },
                    new DESPrivacyProvider(
                        new OctetString(viewModel.Encryption),
                        new MD5AuthenticationProvider(new OctetString(viewModel.Password))
                    ),
                    Messenger.MaxMessageSize,
                    report);

                ISnmpMessage reply = request.GetResponse(10000, new IPEndPoint(ipAddress, 161));

                var valiable = reply
                    .Pdu().Variables
                    .FirstOrDefault(variable => variable.Id.ToString() == viewModel.OID.Id);

                if (valiable != null)
                {
                    viewModel.OID.Value = valiable?.Data.ToString();
                }

                if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
                {
                    throw ErrorException.Create("error in response", ipAddress, reply);
                }

            }
            catch (System.Exception e)
            {
                viewModel.Error = e.Message;
            }

            return View("GetSNMPv3", viewModel);
        }
    }
}