namespace ClubNet.API.Security;

public class JwtSettings
{
    public string SecretKey { get; set; } = "ClubNet_Super_Secret_Key_For_Jwt_Token_Signing_2026_Minimum_256_Bits";
    public string Issuer { get; set; } = "ClubNet.API";
    public string Audience { get; set; } = "ClubNet.Clients";
    public int ExpiryDays { get; set; } = 7;
}

public interface IJwtService
{
    string GenerateToken(int userId, string nome, string email, string perfil, int? idClube, int? idAssociado, int? idDependente = null);
    bool ValidateToken(string token, out System.Security.Claims.ClaimsPrincipal? principal);
}
