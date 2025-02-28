using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context) { _context = context; }

        // POST users/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] User newUserDto)
        {
            // Decrypt the incoming password (Angular sends encrypted password)
            string plainPassword = SecurityHelpers.DecryptPassword(newUserDto.Password);
            // Validate if email or username already exists
            if (_context.Users.Any(u => u.Email == newUserDto.Email))
            {
                return Conflict(new { status = 409, message = "Email already registered" });
            }
            if (_context.Users.Any(u => u.UserName == newUserDto.UserName))
            {
                return Conflict(new { status = 409, message = "Username already taken" });
            }
            // Create new User entity
            var user = new User
            {
                Name = newUserDto.Name,
                Email = newUserDto.Email,
                UserName = newUserDto.UserName,
                MobileNumber = newUserDto.MobileNumber,
                Role = newUserDto.Role,
                PasswordHash = SecurityHelpers.HashPassword(plainPassword),
                IsVerified = true,
                ImageURL = newUserDto.ImageURL,
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            // TODO: send verification email with token link (for simplicity, we skip actual email sending)
            // Generate an email verification token (could reuse JWT or a GUID)
            // e.g., user.VerificationToken = Guid.NewGuid().ToString(); save and email link containing it.

            return StatusCode(201, new { status = 201, message = "Registration successful." });
        }

        // POST users/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginDto)
        {
            string plainPassword = SecurityHelpers.DecryptPassword(loginDto.Password);
            // Find user by email or username and role
            var user = _context.Users.FirstOrDefault(u =>
                (u.Email == loginDto.LoginId || u.UserName == loginDto.LoginId)
                 && u.Role == loginDto.Role);
            if (user == null || !SecurityHelpers.VerifyPassword(plainPassword, user.PasswordHash))
            {
                return Unauthorized(new { status = 401, message = "Invalid credentials" });
            }
            if (!user.IsVerified)
            {
                // If email not verified yet, prevent login
                return StatusCode(403, new { status = 403, message = "Please verify your email before login" });
            }
            // Generate JWT token
            string token = SecurityHelpers.GenerateJwtToken(user);
            // Prepare response data (excluding password)
            var data = new
            {
                id = user.Id,
                name = user.Name,
                userName = user.UserName,
                email = user.Email,
                mobileNumber = user.MobileNumber,
                imageUrl = user.ImageURL,
                userStatus = user.IsVerified  // true/false
            };
            return Ok(new { status = 200, message = "Login successful", token = token, data = data });
        }

        // PUT users/forgotpassword?email=...
        [HttpPut("forgotpassword")]
        public IActionResult ForgotPassword([FromQuery] string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound(new { status = 404, message = "Email not found" });
            }
            // Generate a reset token (GUID)
            string resetToken = Guid.NewGuid().ToString();
            // Save token and expiration (e.g., 1-hour) - for demo, store in user fields
            user.PasswordHash = user.PasswordHash; // no change to password
            user.ImageURL = user.ImageURL; // preserve (just to illustrate updating user)
                                           // (In a real app, add ResetToken and ResetTokenExpiry fields to User or a separate table)
            user.IsVerified = user.IsVerified;
            _context.SaveChanges();
            // TODO: send email to user with reset link containing the token
            // e.g., link = <frontend_url>/reset-password/{resetToken}

            return Ok(new { status = 200, message = "Password reset link sent to email" });
        }

        // PUT users/resetpassword?token=...
        [HttpPut("resetpassword")]
        public IActionResult ResetPassword([FromQuery] string token, [FromBody] ResetPasswordDto resetDto)
        {
            // Find user by reset token (here, we assume token might match user's Id or stored token)
            // For simplicity, assume token is actually user’s email verification or some stored token.
            var user = _context.Users.FirstOrDefault(u => u.Email == resetDto.Email);
            if (user == null /*|| token != user.StoredResetToken*/ )
            {
                return BadRequest(new { status = 400, message = "Invalid or expired reset token" });
            }
            // Decrypt new password and update
            string newPlainPwd = SecurityHelpers.DecryptPassword(resetDto.NewPassword);
            user.PasswordHash = SecurityHelpers.HashPassword(newPlainPwd);
            // Optionally, clear the reset token so it can't be reused
            // user.StoredResetToken = null;
            _context.SaveChanges();
            return Accepted(new { status = 202, message = "Password has been reset successfully" });
        }

        // GET users/verify?token=...
        [HttpGet("verify")]
        public IActionResult VerifyEmail([FromQuery] string token)
        {
            // Verify the email confirmation token and activate the user
            var user = _context.Users.FirstOrDefault(u => /*u.VerificationToken == token*/ u.IsVerified == false && u.Id.ToString() == token);
            if (user == null)
            {
                return BadRequest(new { status = 400, message = "Invalid verification token" });
            }
            user.IsVerified = true;
            // user.VerificationToken = null;
            _context.SaveChanges();
            return Accepted(new { status = 202, message = "Email verified! You can now log in." });
        }

        // PUT users/logout
        //[Authorize]
        [HttpPut("logout")]
        public IActionResult Logout()
        {
            // In a stateless JWT approach, logout is handled client-side by discarding the token.
            // If we needed to blacklist the token, we'd store it, but here we simply return success.
            return Ok(new { status = 200, message = "Logged out successfully" });
        }

        // PUT users/update (profile update)
        //[Authorize]
        [HttpPut("update")]
        public IActionResult UpdateProfile([FromBody] UpdateUserDto profileDto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound(new { status = 404, message = "User not found" });
            // Update allowed fields
            if (!string.IsNullOrEmpty(profileDto.FullName)) user.Name = profileDto.FullName;
            if (!string.IsNullOrEmpty(profileDto.MobileNumber)) user.MobileNumber = profileDto.MobileNumber;
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Profile updated successfully" });
        }

        // POST users/uploadImage (profile or book image upload)
        //[Authorize]
        [HttpPost("uploadImage")]
        public IActionResult UploadImage([FromForm] IFormFile file, [FromQuery] bool isProfile)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { status = 400, message = "No file uploaded" });
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            string uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            Directory.CreateDirectory(uploadsDir);
            // create unique file name
            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                file.CopyTo(stream);
            }
            string relativePath = $"/images/{fileName}";
            if (isProfile)
            {
                // Update user's profile image
                var user = _context.Users.Find(userId);
                if (user == null) return NotFound(new { status = 404, message = "User not found" });
                user.ImageURL = relativePath;
                _context.SaveChanges();
                return Ok(new { status = 200, message = "Profile image uploaded", data = relativePath });
            }
            else
            {
                // Update the latest book (by this vendor) without image
                var book = _context.Books
                            .Where(b => b.SellerId == userId && (b.ImageURL == null || b.ImageURL == ""))
                            .OrderByDescending(b => b.BookId)
                            .FirstOrDefault();
                if (book != null)
                {
                    book.ImageURL = relativePath;
                    _context.SaveChanges();
                }
                return Ok(new { status = 200, message = "Book image uploaded", data = relativePath });
            }
        }
    }

    // DTO classes for clarity
    public class LoginRequest
    {
        public string LoginId { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
    }
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
    }

}
