using System.Threading.Tasks;

namespace VabelMitienditaEsc.Services
{
    public class MockAuthenticationService
    {
        public async Task<bool> ValidateEmailAsync(string email)
        {
            await Task.Delay(500); // Simulación de latencia de red
            if (string.IsNullOrWhiteSpace(email)) return false;
            return email.Contains("@") && email.EndsWith(".com");
        }

        public async Task<bool> ValidatePinAsync(string email, string pin)
        {
            await Task.Delay(500);
            return pin == "1234"; // PIN dummy de prueba estático
        }
    }
}
