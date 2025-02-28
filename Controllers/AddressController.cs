using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("address")]
    public class AddressController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AddressController(AppDbContext context) { _context = context; }

        // POST address/addAddress
        [Authorize(Roles = "Customer")]
        [HttpPost("addAddress")]
        public IActionResult AddAddress([FromBody] Address addr)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // Upsert: if an address of this type exists for user, update it; otherwise add new
            var existing = _context.Addresses.FirstOrDefault(a => a.UserId == userId && a.AddressType == addr.AddressType);
            if (existing != null)
            {
                existing.Name = addr.Name;
                existing.PhoneNumber = addr.PhoneNumber;
                existing.Pincode = addr.Pincode;
                existing.Locality = addr.Locality;
                existing.AddressLine = addr.AddressLine;
                existing.City = addr.City;
                existing.Landmark = addr.Landmark;
            }
            else
            {
                addr.UserId = userId;
                _context.Addresses.Add(addr);
            }
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Address saved" });
        }

        // GET address/getAddressByType?addressType=X
        [Authorize(Roles = "Customer")]
        [HttpGet("getAddressByType")]
        public IActionResult GetAddressByType(string addressType)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var addr = _context.Addresses.FirstOrDefault(a => a.UserId == userId && a.AddressType == addressType);
            if (addr == null) return NotFound(new { status = 404, message = "Address not found" });
            return Ok(new { status = 200, message = "Success", data = addr });
        }
    }

}
