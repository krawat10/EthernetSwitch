using System;
using System.Net;
using System.Threading.Tasks;
using EthernetSwitch.Data;
using EthernetSwitch.Infrastructure.SNMP;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using EthernetSwitch.Infrastructure.SNMP.Queries;
using EthernetSwitch.Models.SNMP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EthernetSwitch.Data.Models;
using System.Linq;
using EthernetSwitch.Infrastructure.Settings;
using EthernetSwitch.Infrastructure.Bash.Exceptions;

namespace EthernetSwitch.Controllers
{
    public class SNMPController : Controller
    {
        private readonly SNMPServices _services;
        private readonly EthernetSwitchContext _context;
        private readonly ITrapUsersRepository trapUsersRepository;
        private readonly ISNMPUsersRepository snmpUsersRepository;
        private readonly ISettingsRepository settingsRepository;

        public SNMPController(SNMPServices services, EthernetSwitchContext context,
            ITrapUsersRepository trapUsersRepository,
            ISNMPUsersRepository snmpUsersRepository,
            ISettingsRepository settingsRepository)
        {
            _services = services;
            _context = context;
            this.trapUsersRepository = trapUsersRepository;
            this.snmpUsersRepository = snmpUsersRepository;
            this.settingsRepository = settingsRepository;
        }

        public IActionResult GetSNMPv3() => View(new GetSNMPv3ViewModel { IpAddress = HttpContext.Connection.LocalIpAddress.ToString() });

        public IActionResult SetSNMPv3() => View(new SetSNMPv3ViewModel { IpAddress = HttpContext.Connection.LocalIpAddress.ToString() });

        public IActionResult WalkSNMPv1() => View(new WalkSNMPv1ViewModel { IpAddress = HttpContext.Connection.LocalIpAddress.ToString() });

        public async Task<IActionResult> TrapSNMPv3()
        {
            ViewData["Messages"] = await _context.TrapMessages
                .OrderByDescending(x => x.TimeStamp)
                .Include(x => x.Variables)
                .ToListAsync();

            ViewData["ActiveUsers"] = await trapUsersRepository.GetUsers();

            return View(new TrapSNMPv3ViewModel());
        }

        public async Task<IActionResult> ClearTrapMessages()
        {
            _context.TrapMessages.RemoveRange(_context.TrapMessages);
            await _context.SaveChangesAsync();

            return RedirectToAction("TrapSNMPv3");
        }

        public async Task<IActionResult> RemoveTrapUser(long id)
        {
            await trapUsersRepository.Remove(id);

            return RedirectToAction("TrapSNMPv3");
        }

        public async Task<IActionResult> SetupSNMP()
        {
            var settings = await settingsRepository.GetSettings();

            return View(settings.SNMPConfiguration);
        }

        [HttpPost]
        public async Task<IActionResult> SetupSNMP(SNMPConfiguration configuration)
        {
            try
            {
                await _services.Handle(configuration);
            }
            catch (ProcessException ex)
            {
                ViewData["Error"] = ex.Message;
            }

            return View(configuration);
        }

        public async Task<IActionResult> AddSNMPv3User()
        {
            ViewData["Users"] = await snmpUsersRepository.GetUsers();

            return View(new SNMPUser());
        }

        [HttpPost]
        public async Task<IActionResult> AddSNMPv3User(SNMPUser user)
        {
            try
            {
                await _services.Handle(user);
            }
            catch (ProcessException ex)
            {
                ViewData["Error"] = ex.Message;
            }

            ViewData["Users"] = await snmpUsersRepository.GetUsers();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> TrapSNMPv3(TrapSNMPv3ViewModel viewModel)
        {
            try
            {
                await trapUsersRepository.AddUser(new SNMPTrapUser(viewModel.UserName, viewModel.Port, viewModel.Password, viewModel.Encryption, viewModel.EngineId));
            }
            catch (Exception e)
            {
                viewModel.Error = e.Message;
            }

            ViewData["Messages"] = await _context.TrapMessages
                .OrderByDescending(x => x.TimeStamp)
                .Include(x => x.Variables)
                .ToListAsync();

            ViewData["ActiveUsers"] = await trapUsersRepository.GetUsers();

            return View("TrapSNMPv3", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetSNMPv1(WalkSNMPv1ViewModel viewModel)
        {
            viewModel.Error = "";

            try
            {
                viewModel.OIDs = await _services.Handle(new WalkQuery(
                    viewModel.Group,
                    viewModel.StartObjectId,
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