using System;
using System.Net;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.SNMP;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using EthernetSwitch.Infrastructure.SNMP.Queries;
using EthernetSwitch.Models.SNMP;
using Lextm.SharpSnmpLib;
using Microsoft.AspNetCore.Mvc;

namespace EthernetSwitch.Controllers
{
    public class SNMPController : Controller
    {
        private readonly SNMPServices _services;

        public SNMPController(SNMPServices services)
        {
            _services = services;
        }

        public IActionResult GetSNMPv3() => View(new GetSNMPv3ViewModel {IpAddress = HttpContext.Connection.LocalIpAddress.ToString()});

        public IActionResult SetSNMPv3() => View(new SetSNMPv3ViewModel {IpAddress = HttpContext.Connection.LocalIpAddress.ToString()});

        public IActionResult WalkSNMPv1() => View(new WalkSNMPv1ViewModel {IpAddress = HttpContext.Connection.LocalIpAddress.ToString()});

        [HttpPost]
        public async Task<IActionResult> GetSNMPv1(WalkSNMPv1ViewModel viewModel)
        {
            viewModel.Error = "";

            try
            {
                viewModel.OIDs = await _services.Handle(new WalkV1Query(
                    viewModel.Group,
                    viewModel.VersionCode,
                    new ObjectIdentifier(viewModel.StartObjectId),
                    IPAddress.Parse(viewModel.IpAddress),
                    viewModel.Port));
            }
            catch (Exception e)
            {
                viewModel.Error = e.Message;
            }

            return View("WalkSNMPv1", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetSNMPv3(GetSNMPv3ViewModel viewModel)
        {
            viewModel.Error = "";

            try
            {
                viewModel.OID = await _services.Handle(new GetV3Query(
                    viewModel.UserName,
                    viewModel.VersionCode,
                    IPAddress.Parse(viewModel.IpAddress),
                    viewModel.Port,
                    viewModel.Password,
                    viewModel.Encryption,
                    viewModel.OID.Id));
            }
            catch (Exception e)
            {
                viewModel.Error = e.Message;
            }

            return View("GetSNMPv3", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SetSNMPv3(SetSNMPv3ViewModel viewModel)
        {
            viewModel.Error = "";

            try
            {
                await _services.Handle(new SetV3Command(
                    viewModel.UserName,
                    viewModel.VersionCode,
                    IPAddress.Parse(viewModel.IpAddress),
                    viewModel.Port,
                    viewModel.Password,
                    viewModel.Encryption,
                    viewModel.OID));
            }
            catch (Exception e)
            {
                viewModel.Error = e.Message;
            }

            return View("SetSNMPv3", viewModel);
        }
    }
}