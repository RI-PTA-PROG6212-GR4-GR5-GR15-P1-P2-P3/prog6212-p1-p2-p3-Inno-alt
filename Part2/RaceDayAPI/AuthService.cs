using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Services
{
    public class AuthService
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

            if (existingUser != null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Email already registered."
                };
            }

            // Validate role
            if (registerDto.Role != "Participant" && registerDto.Role != "Organiser")
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Role must be either 'Participant' or 'Organiser'."
                };
            }

            // Hash the password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create the User
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Participant or Organiser profile
            if (registerDto.Role == "Participant")
            {
                var participant = new Participant
                {
                    UserID = user.UserID,
                    FullName = registerDto.FullName ?? "Unknown",
                    IDNumber = registerDto.IDNumber ?? "0000000000000",
                    PhoneNumber = registerDto.PhoneNumber,
                    DateOfBirth = registerDto.DateOfBirth,
                    Gender = registerDto.Gender,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Participants.Add(participant);
            }
            else if (registerDto.Role == "Organiser")
            {
                var organiser = new Organiser
                {
                    UserID = user.UserID,
                    OrganisationName = registerDto.OrganisationName ?? "Unknown",
                    RegistrationNumber = registerDto.RegistrationNumber,
                    ContactPerson = registerDto.ContactPerson ?? "Unknown",
                    PhoneNumber = registerDto.PhoneNumber,
                    Email = registerDto.Email,
                    Website = registerDto.Website,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Organisers.Add(organiser);
            }

            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Registration successful.",
                UserID = user.UserID,
                Email = user.Email,
                Role = user.Role,
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            // Verify password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Account is inactive."
                };
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Login successful.",
                UserID = user.UserID,
                Email = user.Email,
                Role = user.Role,
                Token = GenerateJwtToken(user)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "ThisIsASecretKeyForJwtTokenGenerationForRaceDayApi2026!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _configuration["Jwt:Issuer"] ?? "RaceDayAPI",
                Audience = _configuration["Jwt:Audience"] ?? "RaceDayClient",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
