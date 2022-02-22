using app.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace app.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PetitionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PetitionController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Petition>>> GetPetitions()
        {
            return await _context.Petitions
                    .Include(a => a.Points)
                    .Include(b => b.Units)
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Petition>> GetPetition(Guid id)
        {
            var petition = await _context.Petitions
                .Include(a => a.Points)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (petition == null)
            {
                return NotFound();
            }

            return petition;
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutPetition([FromBody] Petition petition)
        {
            if (await _context.Petitions.FindAsync(petition.Id) == null)
            {
                return BadRequest();
            }

            var _petition = await _context.Petitions
                .Include(a => a.Points)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(c => c.Id == petition.Id);
            
            _context.Units.RemoveRange(_petition.Units); //remove units before add new

            foreach(var unit in petition.Units)
            {
                unit.Id = null;
            }

            _petition.Title = petition.Title;
            _petition.Reason = petition.Reason;
            _petition.Vehicle = petition.Vehicle;
            _petition.Type = petition.Type;
            _petition.Checked = petition.Checked;
            _petition.Date = DateTime.Now.ToLocalTime(); //doc datetime
            _petition.DateFrom = Convert.ToDateTime(petition.DateFrom).ToLocalTime();
            _petition.DateTo = Convert.ToDateTime(petition.DateTo).ToLocalTime();
            _petition.Points.Clear(); //clear table petitionpoint m-to-m
            _petition.Units.Clear(); //clear table petitionunit m-to-m
            _petition.Points = TakePoints(petition.Points); //add points from list of points
            _petition.Units = petition.Units; //add units from 

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PetitionExists(_petition.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostPetition(Petition petition)
        {
            var _petition = new Petition
            {
                Title = petition.Title,
                Reason = petition.Reason,
                Vehicle = petition.Vehicle,
                Type = petition.Type,
                Checked = petition.Checked,
                Date = DateTime.Now.ToLocalTime(),
                DateFrom = Convert.ToDateTime(petition.DateFrom).ToLocalTime(),
                DateTo = Convert.ToDateTime(petition.DateTo).ToLocalTime(),
                Points = TakePoints(petition.Points),
                Units = petition.Units
            };

            var add = _context.Petitions.Add(_petition);

            if (add.State == EntityState.Added)
            {
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePetition(Guid id)
        {
            var petition = await _context.Petitions
                .Include(a => a.Units)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (petition == null)
            {
                return NotFound();
            }

            _context.Units.RemoveRange(petition.Units);
            _context.Petitions.Remove(petition);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PetitionExists(Guid? id)
        {
            return _context.Petitions.Any(e => e.Id == id);
        }

        private List<Point> TakePoints(List<Point> points)
        {
            List<Point> _points = new List<Point>();
            points.ForEach(async x => _points.Add(await _context.Points.FindAsync(x.Id)));
            return _points;
        }
    } 
}
