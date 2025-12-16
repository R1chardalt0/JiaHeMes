using Microsoft.IdentityModel.Tokens;
using ChargePadLine.Common.TokenModule.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Common.TokenModule
{
    public static class TokenHelper
    {
        public static string CreateToken(JwtTokenModel jwtTokenModel)
        {
            if (jwtTokenModel == null) throw new ArgumentNullException(nameof(jwtTokenModel));
            if (string.IsNullOrEmpty(jwtTokenModel.Security)) throw new ArgumentException("Security key不能为空");

            try
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Id", jwtTokenModel.Id.ToString()),
                    new Claim("UserName", jwtTokenModel.UserName),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenModel.Security));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtTokenModel.Issuer,
                    audience: jwtTokenModel.Audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(jwtTokenModel.Expires),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // 记录日志
                throw new Exception("Token生成失败", ex);
            }
        }
    }
}