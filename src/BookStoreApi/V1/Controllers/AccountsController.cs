using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookStore.Infrastructure;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Models;
using BookStoreApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BookStoreApiV1.V1.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/" + ApiConstants.ServiceName + "/v{api-version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountsController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context,
            TokenValidationParameters tokenValidationParams,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tokenValidationParams = tokenValidationParams;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                var existingUserName = await _userManager.FindByNameAsync(model.UserName);

                if (existingEmail != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Email already in use"
                            },
                        Success = false
                    });
                }

                if (existingUserName != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Username already in use"
                            },
                        Success = false
                    });
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var jwtToken = await GenerateJwtToken(user);
                    return Ok(jwtToken);
                }
                else
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = result.Errors.Select(x => x.Description).ToList(),
                        Success = false
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                        "Invalid payload."
                    },
                Success = false
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user;
                if (model.Email.Contains("@"))
                    user = await _userManager.FindByEmailAsync(model.Email);
                else
                    user = await _userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Invalid login request"
                            },
                        Success = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(user, model.Password);

                if (!isCorrect)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Invalid login request"
                            },
                        Success = false
                    });
                }

                var jwtToken = await GenerateJwtToken(user);

                return Ok(jwtToken);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                        "Invalid payload"
                    },
                Success = false
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return LocalRedirect("~/");
            }
            //return Ok();
        }

        [HttpGet("exist/{email}/{name}")]
        public async Task<IActionResult> CredentialsExist(string email, string name)
        {
            if (ModelState.IsValid)
            {
                var userEmail = await _userManager.FindByEmailAsync(email);
                var userName = await _userManager.FindByNameAsync(name);

                if (userEmail != null && userName == null)
                {
                    return Ok(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                $"Email {email} is taken."
                            },
                        Success = false
                    });
                }

                if (userName != null && userEmail == null)
                {
                    return Ok(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                $"User name {name} is taken."
                            },
                        Success = false
                    });
                }

                if (userEmail != null && userName != null)
                {
                    return Ok(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                $"User name {name} is taken.",
                                $"Email {email} is taken."
                            },
                        Success = false
                    });
                }

                if (userEmail == null && userName == null)
                {
                    return Ok(new RegistrationResponse()
                    {
                        Success = true
                    });
                }
            }

            return Ok(new RegistrationResponse()
            {
                Errors = ModelState.Values.Where(e => e.Errors.Count > 0).SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList(),
                Success = false
            }); ;
        }

        [HttpPost]
        [Route("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyAndGenerateToken(tokenRequest);

                if (result == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                            "Invalid tokens"
                        },
                        Success = false
                    });
                }

                return Ok(result);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                    "Invalid payload"
                },
                Success = false
            });
        }

        private async Task<AuthResult> GenerateJwtToken(ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(Constants.JwtToken.Secret);

            var claims = await GetAllValidClaims(user);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // 5-10
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }

        // Get all valid claims for the corresponding user
        private async Task<List<Claim>> GetAllValidClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim("FullName", user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.NameId, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Getting the claims that we have assigned to the user
            var userClaims = await _userManager.GetClaimsAsync(user);

            if (userClaims != null)
            {
                claims.AddRange(userClaims);
            }

            // Get the user role and add it to the claims
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles != null)
            {
                foreach (var userRole in userRoles)
                {
                    var role = await _roleManager.FindByNameAsync(userRole);
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole));

                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        foreach (var roleClaim in roleClaims)
                        {
                            claims.Add(roleClaim);
                        }
                    }
                }
            }



            return claims;
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validation 1 - Validation JWT token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParams, out var validatedToken);

                // Validation 2 - Validate encryption alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return null;
                    }
                }

                // Validation 3 - validate expiry date
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() {
                            "Token has not yet expired"
                        }
                    };
                }

                // validation 4 - validate existence of the token
                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

                if (storedToken == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() {
                            "Token does not exist"
                        }
                    };
                }

                // Validation 5 - validate if used
                if (storedToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() {
                            "Token has been used"
                        }
                    };
                }

                // Validation 6 - validate if revoked
                if (storedToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() {
                            "Token has been revoked"
                        }
                    };
                }

                // Validation 7 - validate the id
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (storedToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() {
                            "Token doesn't match"
                        }
                    };
                }

                // update current token

                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                // Generate a new token
                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed. The token is expired."))
                {

                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() {
                            "Token has expired please re-login"
                        }
                    };

                }
                else
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() {
                            "Something went wrong."
                        }
                    };
                }
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();

            return dateTimeVal;
        }

        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(x => x[random.Next(x.Length)]).ToArray());
        }

    }
}
