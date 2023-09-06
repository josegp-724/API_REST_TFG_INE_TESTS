
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using System.IO.Compression;
using System.Text;
using System.Diagnostics;
using ExcelDataReader;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using TFGv3Net7.Controllers;
using TFGv3Net7.Models;
using TFGv3Net7.Services;
using TFGv3Net7.Services.Interfaces;
using Xunit;
using System.Collections.Generic;
using TFGv3Net7.Registros;


namespace TFGv3Net7.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        // Mocks para los repositorios
        private readonly Mock<ILogger<AuthenticationController>> _mockLogger;
        private readonly Mock<ISexoRepository> _mockSexoRepo;
        private readonly Mock<IAtributoRepository> _mockAtributoRepo;
        private readonly Mock<IDatoRepository> _mockDatoRepo;
        private readonly Mock<IDistritoRepository> _mockDistritoRepo;
        private readonly Mock<IFechaRepository> _mockFechaRepo;
        private readonly Mock<IMunicipioRepository> _mockMunicipioRepo;
        private readonly Mock<IProvinciaRepository> _mockProvinciaRepo;
        private readonly Mock<ISeccionCensalRepository> _mockSeccionCensalRepo;
        private readonly Mock<IViasRepository> _mockViasRepo;
        private readonly Mock<ITramosRepository> _mockTramosRepo;
        private readonly Mock<IConsultaRepository> _mockConsultasRepo;
        private readonly Mock<ILoteRepository> _mockLoteRepo;
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IEmailService> _mockEmailService; // <- Agregar esta línea

        public AuthenticationControllerTests()
        {
            _mockLogger = new Mock<ILogger<AuthenticationController>>();
            _mockSexoRepo = new Mock<ISexoRepository>();
            _mockAtributoRepo = new Mock<IAtributoRepository>();
            _mockDatoRepo = new Mock<IDatoRepository>();
            _mockDistritoRepo = new Mock<IDistritoRepository>();
            _mockFechaRepo = new Mock<IFechaRepository>();
            _mockMunicipioRepo = new Mock<IMunicipioRepository>();
            _mockProvinciaRepo = new Mock<IProvinciaRepository>();
            _mockSeccionCensalRepo = new Mock<ISeccionCensalRepository>();
            _mockViasRepo = new Mock<IViasRepository>();
            _mockTramosRepo = new Mock<ITramosRepository>();
            _mockConsultasRepo = new Mock<IConsultaRepository>();
            _mockLoteRepo = new Mock<ILoteRepository>();
            _mockUsuarioRepo = new Mock<IUsuarioRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _mockEmailService = new Mock<IEmailService>(); // <- Agregar esta línea
        }

        // Método de ayuda para crear el AuthenticationController con las dependencias mockeadas:
        private AuthenticationController CreateController()
        {
            return new AuthenticationController(
                _mockLogger.Object,
                _mockSexoRepo.Object,
                _mockAtributoRepo.Object,
                _mockDatoRepo.Object,
                _mockDistritoRepo.Object,
                _mockFechaRepo.Object,
                _mockMunicipioRepo.Object,
                _mockProvinciaRepo.Object,
                _mockSeccionCensalRepo.Object,
                _mockViasRepo.Object,
                _mockTramosRepo.Object,
                _mockConsultasRepo.Object,
                _mockLoteRepo.Object,
                _mockUsuarioRepo.Object,
                _mockConfig.Object,
                _mockEmailService.Object // <- Agregar esta línea

            );
        }

        // Continuación de tu código...

        [Fact]
        public async Task Register_UserExists_ReturnsBadRequest()
        {
            // Arrange
            var testEmail = "test@email.com";
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(testEmail)).ReturnsAsync(new Usuario());

            var controller = CreateController();
            
            // Act
            var result = await controller.Register(new LoginModel { Email = testEmail, Password = "Test123" });
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("El usuario ya existe", badRequestResult.Value);
        }

        [Fact]
        public async Task Register_Success_ReturnsOk()
        {
            // Arrange
            var testEmail = "test@email.com";
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(testEmail)).ReturnsAsync((Usuario)null);

            var controller = CreateController();
            
            // Act
            var result = await controller.Register(new LoginModel { Email = testEmail, Password = "Test123" });
            
            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Register_ThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var testEmail = "test@email.com";
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(testEmail)).Throws(new Exception("Error"));

            var controller = CreateController();
            
            // Act
            var result = await controller.Register(new LoginModel { Email = testEmail, Password = "Test123" });
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Registro erroneo", badRequestResult.Value);
        }






        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var testEmail = "valid@email.com";
            var testToken = "valid_token";
            _mockUsuarioRepo.Setup(repo => repo.IniciarSesionAsync(testEmail, "Valid123")).ReturnsAsync(testToken);

            var controller = CreateController();
            
            // Act
            var result = await controller.Login(new LoginModel { Email = testEmail, Password = "Valid123" });
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedToken = okResult.Value as string; // Convertimos el valor a string directamente
            Assert.Equal(testToken, returnedToken);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var testEmail = "invalid@email.com";
            _mockUsuarioRepo.Setup(repo => repo.IniciarSesionAsync(testEmail, "Invalid123")).ReturnsAsync((string)null);

            var controller = CreateController();
            
            // Act
            var result = await controller.Login(new LoginModel { Email = testEmail, Password = "Invalid123" });
            
            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid username or password", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_ThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var testEmail = "error@email.com";
            _mockUsuarioRepo.Setup(repo => repo.IniciarSesionAsync(testEmail, "Error123")).Throws(new Exception("Error"));

            var controller = CreateController();
            
            // Act
            var result = await controller.Login(new LoginModel { Email = testEmail, Password = "Error123" });
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Inicio de sesión incorrecto.", badRequestResult.Value);
        }





        // Método auxiliar para simular un usuario autenticado.
        private void MockAuthenticatedUser(AuthenticationController controller, string email)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, email)
            }));
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task ChangeEmail_NoUserContext_ReturnsUnauthorized()
        {
            // Arrange
            var controller = CreateController();
            var emailModel = new EmailModel { NuevoCorreo = "newtest@email.com" };
            
            // Act
            var result = await controller.ChangeEmail(emailModel);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ChangeEmail_NewEmailSameAsCurrent_ReturnsBadRequest()
        {
            // Arrange
            var currentEmail = "test@email.com";
            var controller = CreateController();
            MockAuthenticatedUser(controller, currentEmail);

            var emailModel = new EmailModel { NuevoCorreo = currentEmail };

            // Act
            var result = await controller.ChangeEmail(emailModel);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ChangeEmail_NewEmailAlreadyInUse_ReturnsBadRequest()
        {
            // Arrange
            var currentEmail = "test@email.com";
            var newEmail = "newtest@email.com";
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(newEmail)).ReturnsAsync(new Usuario());

            var controller = CreateController();
            MockAuthenticatedUser(controller, currentEmail);

            var emailModel = new EmailModel { NuevoCorreo = newEmail };

            // Act
            var result = await controller.ChangeEmail(emailModel);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ChangeEmail_SuccessfulChange_ReturnsOk()
        {
            // Arrange
            var currentEmail = "test@email.com";
            var newEmail = "newtest@email.com";
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(newEmail)).ReturnsAsync((Usuario)null);
            _mockUsuarioRepo.Setup(repo => repo.ObtenerIdUsuarioPorEmailAsync(currentEmail)).ReturnsAsync(123); // Suponiendo 123 es el ID de usuario
            _mockUsuarioRepo.Setup(repo => repo.CambiarCorreoAsync(123, newEmail)).Returns(Task.CompletedTask);

            var controller = CreateController();
            MockAuthenticatedUser(controller, currentEmail);

            var emailModel = new EmailModel { NuevoCorreo = newEmail };

            // Act
            var result = await controller.ChangeEmail(emailModel);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }



        [Fact]
        public async Task ChangePassword_NoUserContext_ReturnsUnauthorized()
        {
            var controller = CreateController();
            
            var result = await controller.ChangePassword(new PasswordChangeModel());
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("No se pudo obtener la información del usuario.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task ChangePassword_UserNotFound_ReturnsNotFound()
        {
            var testEmail = "test@email.com";

            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(testEmail)).ReturnsAsync((Usuario)null);

            var controller = CreateController();
            MockAuthenticatedUser(controller, testEmail);

            var result = await controller.ChangePassword(new PasswordChangeModel());

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Usuario no encontrado.", notFoundResult.Value);
        }

        [Fact]
        public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequest()
        {
            var testEmail = "test@email.com";
            var usuario = new Usuario
            {
                HashPassword = "salt:hashedPassword"
            };

            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(testEmail)).ReturnsAsync(usuario);

            var controller = CreateController();
            MockAuthenticatedUser(controller, testEmail);

            var result = await controller.ChangePassword(new PasswordChangeModel { CurrentPassword = "wrongPassword" });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("La contraseña actual es incorrecta.", badRequestResult.Value);
        }


        

        [Fact]
        public async Task ForgotPassword_EmailNotAssociated_ReturnsBadRequest()
        {
            // Arrange
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            var controller = CreateController();

            // Act
            var result = await controller.ForgotPassword(new ForgotPasswordRequest { Email = "test@test.com" });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("El correo electrónico no está asociado a ninguna cuenta.", badRequestResult.Value);
        }
        
        [Fact]
        public async Task ForgotPassword_EmailValid_SendsRecoveryEmail()
        {
            // Arrange
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(It.IsAny<string>())).ReturnsAsync(new Usuario());
            _mockEmailService.Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockConfig.Setup(config => config["webDomain"]).Returns("http://example.com");
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("your_secret_key_here");

            var controller = CreateController();

            // Act
            var result = await controller.ForgotPassword(new ForgotPasswordRequest { Email = "test@test.com" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Correo de recuperación enviado.", okResult.Value);
            _mockEmailService.Verify(service => service.SendEmailAsync(It.IsAny<string>(), "Recuperación de Contraseña", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_EmailServiceError_ReturnsBadRequest()
        {
            // Arrange
            _mockUsuarioRepo.Setup(repo => repo.ObtenerUsuarioPorEmailAsync(It.IsAny<string>())).ReturnsAsync(new Usuario());
            _mockEmailService.Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Error de prueba"));
            _mockConfig.Setup(config => config["webDomain"]).Returns("http://example.com");
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("your_secret_key_here");

            var controller = CreateController();

            // Act
            var result = await controller.ForgotPassword(new ForgotPasswordRequest { Email = "test@test.com" });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error al enviar el correo de recuperación. Error de prueba", badRequestResult.Value);
        }

        

        // Fin del archivo...
    }
}
