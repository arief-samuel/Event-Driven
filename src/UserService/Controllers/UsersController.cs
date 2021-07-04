using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Data;
using UserService.Entities;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserServiceContext _context;
        private readonly IntegrationEventSenderService _integrationEventSenderService;

        public UsersController(
            UserServiceContext context, 
            IntegrationEventSenderService integrationEventSenderService
            )
        {
            _context = context;
            _integrationEventSenderService = integrationEventSenderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _context.User.ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)

        {
            using var transaction = _context.Database.BeginTransaction();
            //var existingUser = await _context.User.Where(x => x.ID == id).SingleOrDefaultAsync();
            var existingUser = await _context.User.FindAsync(id);
            if (existingUser is null)
                return NotFound();

            existingUser.Name = user.Name;
            existingUser.Mail = user.Mail;
            existingUser.OtherData = user.OtherData;
            existingUser.Version += 1;

            await _context.SaveChangesAsync();

            var integrationEventData = JsonConvert.SerializeObject(new
            {
                id = user.ID,
                newname = user.Name,
                version = existingUser.Version
            });
            _context.IntegrationEventOutbox.Add(
                new IntegrationEvent()
                {
                    Event = "user.update",
                    Data = integrationEventData
                });

            _context.SaveChanges();
            transaction.Commit();
            _integrationEventSenderService.StartPublishingOutstandingIntegrationEvents();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user.Version = 1;
            using var transaction = _context.Database.BeginTransaction();
            _context.User.Add(user);
            _context.SaveChanges();

            var integrationEventData = JsonConvert.SerializeObject(new
            {
                id = user.ID,
                name = user.Name,
                version = user.Version
            });

            _context.IntegrationEventOutbox.Add(
                new IntegrationEvent()
                {
                    Event = "user.add",
                    Data = integrationEventData
                });

            await _context.SaveChangesAsync();
            transaction.Commit();
            _integrationEventSenderService.StartPublishingOutstandingIntegrationEvents();
            return CreatedAtAction("GetUser", new { id = user.ID }, user);
        }
    }
}