using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CarPooling.Domain.Entities;

namespace CarPooling.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        DateTime GetTokenExpiration();
        string GenerateRefreshToken();
    }
}
