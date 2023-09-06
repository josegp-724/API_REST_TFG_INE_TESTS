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
    public class UserAdminControllerTests
    {
        // Mocks para los repositorios
        private readonly Mock<ILogger<UserAdminController>> _mockLogger;
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

        public UserAdminControllerTests()
        {
            _mockLogger = new Mock<ILogger<UserAdminController>>();
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

        // Método de ayuda para crear el UserAdminController con las dependencias mockeadas:
        private UserAdminController CreateController()
        {
            return new UserAdminController(
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
                _mockUsuarioRepo.Object
            );
        }

        // Continuación de tu código...
        

        [Fact]
        public async Task GetAllUsers_ReturnsListOfUsers()
        {
            // Preparar
            var testUsers = new List<UsuarioDTO>
            {
                new UsuarioDTO { Email = "test1@example.com" },
                new UsuarioDTO { Email = "test2@example.com" }
            };

            _mockUsuarioRepo.Setup(repo => repo.ObtenerTodosLosUsuariosAsync()).ReturnsAsync(testUsers);

            var controller = CreateController();

            // Actuar
            var result = await controller.GetAllUsers();

            // Verificar
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<UsuarioDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetAllUsers_ThrowsException_ReturnsServerError()
        {
            // Preparar
            _mockUsuarioRepo.Setup(repo => repo.ObtenerTodosLosUsuariosAsync()).ThrowsAsync(new Exception("DB Connection Error"));

            var controller = CreateController();

            // Actuar
            var result = await controller.GetAllUsers();

            // Verificar
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error interno del servidor.", statusCodeResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOkResult_WhenUserIsDeleted()
        {
            // Preparar
            var testEmail = "test@example.com";

            // No necesitamos definir un comportamiento específico para el método, ya que si no lanza excepción,
            // simplemente devolverá Ok
            _mockUsuarioRepo.Setup(repo => repo.EliminarUsuarioPorEmailAsync(testEmail)).Returns(Task.CompletedTask);

            var controller = CreateController();

            // Actuar
            var result = await controller.DeleteUser(testEmail);

            // Verificar
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsServerError_WhenExceptionThrown()
        {
            // Preparar
            var testEmail = "test@example.com";
            _mockUsuarioRepo.Setup(repo => repo.EliminarUsuarioPorEmailAsync(testEmail)).ThrowsAsync(new Exception("DB Connection Error"));

            var controller = CreateController();

            // Actuar
            var result = await controller.DeleteUser(testEmail);

            // Verificar
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error interno del servidor.", statusCodeResult.Value);
        }

        [Fact]
        public async Task EditUser_ReturnsOkResult_WhenUserIsEditedSuccessfully()
        {
            // Preparar
            var testEditUser = new EditUser 
            {
                OldEmail = "oldTest@example.com",
                NewEmail = "newTest@example.com",
                BoolAdmin = true,
                MaxConsultas = 5
            };

            var userId = 1; // ID simulado para este test

            _mockUsuarioRepo.Setup(repo => repo.ObtenerIdUsuarioPorEmailAsync(testEditUser.OldEmail)).ReturnsAsync(userId);
            _mockUsuarioRepo.Setup(repo => repo.EditarUsuarioPorIdAsync(userId, It.IsAny<UsuarioDTO>())).Returns(Task.CompletedTask);

            var controller = CreateController();

            // Actuar
            var result = await controller.EditUser(testEditUser);

            // Verificar
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task EditUser_ReturnsServerError_WhenExceptionThrown()
        {
            // Preparar
            var testEditUser = new EditUser 
            {
                OldEmail = "oldTest@example.com",
                NewEmail = "newTest@example.com",
                BoolAdmin = true,
                MaxConsultas = 5
            };

            _mockUsuarioRepo.Setup(repo => repo.ObtenerIdUsuarioPorEmailAsync(testEditUser.OldEmail)).ThrowsAsync(new Exception("DB Connection Error"));

            var controller = CreateController();

            // Actuar
            var result = await controller.EditUser(testEditUser);

            // Verificar
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error interno del servidor.", statusCodeResult.Value);
        }


        [Fact]
        public async Task UpdateAllMaxConsultas_ReturnsOkResultWithMessage_WhenMaxConsultasIsUpdatedSuccessfully()
        {
            // Preparar
            int testMaxConsultas = 10;

            _mockUsuarioRepo.Setup(repo => repo.ActualizarMaxConsultasAsync(testMaxConsultas)).Returns(Task.CompletedTask);

            var controller = CreateController();

            // Actuar
            var result = await controller.UpdateAllMaxConsultas(testMaxConsultas);

            // Verificar
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("MaxConsultas de todos los usuarios actualizados correctamente.", okResult.Value);
        }

        [Fact]
        public async Task UpdateAllMaxConsultas_ReturnsServerError_WhenExceptionThrown()
        {
            // Preparar
            int testMaxConsultas = 10;

            _mockUsuarioRepo.Setup(repo => repo.ActualizarMaxConsultasAsync(testMaxConsultas)).ThrowsAsync(new Exception("DB Connection Error"));

            var controller = CreateController();

            // Actuar
            var result = await controller.UpdateAllMaxConsultas(testMaxConsultas);

            // Verificar
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error interno del servidor.", statusCodeResult.Value);
        }
        // Fin del archivo...
    }
}
