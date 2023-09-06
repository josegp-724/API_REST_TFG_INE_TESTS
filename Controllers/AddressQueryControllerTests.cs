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
    public class AddressQueryControllerTests
    {
        // Mocks para los repositorios
        private readonly Mock<ILogger<AddressQueryController>> _mockLogger;
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

        public AddressQueryControllerTests()
        {
            _mockLogger = new Mock<ILogger<AddressQueryController>>();
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

        // Método de ayuda para crear el AddressQueryController con las dependencias mockeadas:
        private AddressQueryController CreateController()
        {
            return new AddressQueryController(
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
        public async Task CreateConsultaLoteFromJson_ReturnsUnauthorized_WhenClaimNotFoundOrEmpty()
        {
            // Preparar
            var loteAndConsultasDTO = new LoteAndConsultasDTO();
            
            var controller = CreateController();

            // Actuar - Caso cuando el claim es null
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
            var resultNoClaim = await controller.CreateConsultaLoteFromJson(loteAndConsultasDTO);

            // Verificar
            Assert.IsType<UnauthorizedObjectResult>(resultNoClaim);

            // Actuar - Caso cuando el valor del claim es vacío
            var userEmptyClaim = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] 
            {
                new Claim(ClaimTypes.NameIdentifier, "")
            }, "authenticationType"));
            controller.ControllerContext.HttpContext.User = userEmptyClaim;
            var resultEmptyClaim = await controller.CreateConsultaLoteFromJson(loteAndConsultasDTO);

            // Verificar
            Assert.IsType<UnauthorizedObjectResult>(resultEmptyClaim);
        }


        [Fact]
        public async Task CreateConsultaLoteFromJson_ReturnsUnauthorized_WhenUserIdNotFound()
        {
            // Preparar
            var loteAndConsultasDTO = new LoteAndConsultasDTO(); 
            _mockUsuarioRepo.Setup(r => r.ObtenerIdUsuarioPorEmailAsync(It.IsAny<string>())).ReturnsAsync(1);

            var controller = CreateController();
            ConfigureUserControllerContext(controller, null);

            // Actuar
            var result = await controller.CreateConsultaLoteFromJson(loteAndConsultasDTO);

            // Verificar
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        private void ConfigureUserControllerContext(AddressQueryController controller, string id)
        {
            var claims = string.IsNullOrEmpty(id) 
                ? new Claim[] { } 
                : new Claim[] { new Claim(ClaimTypes.NameIdentifier, id) };

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "authenticationType"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateConsultaLoteFromJson_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Preparar
            var loteAndConsultasDTO = new LoteAndConsultasDTO();
            var controller = CreateController();
            controller.ModelState.AddModelError("TestError", "Test error message");

            // Actuar
            var result = await controller.CreateConsultaLoteFromJson(loteAndConsultasDTO);

            // Verificar
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateConsultaLoteFromJson_ReturnsBadRequest_WhenMaxQueriesExceeded()
        {
            // Preparar
            var loteAndConsultasDTO = new LoteAndConsultasDTO();

            // Asegurándonos de crear el controlador antes de configurar su contexto
            var controller = CreateController();
            ConfigureUserControllerContext(controller, "test@example.com");
            
            _mockUsuarioRepo.Setup(r => r.ObtenerIdUsuarioPorEmailAsync(It.IsAny<string>())).ReturnsAsync(1);
            _mockLoteRepo.Setup(r => r.ContarConsultasDelDiaPorUsuarioAsync(It.IsAny<int>())).ReturnsAsync(50);
            _mockUsuarioRepo.Setup(r => r.ObtenerNumeroMaximoConsultasAsync(It.IsAny<int>())).ReturnsAsync(40);

            // Actuar
            var result = await controller.CreateConsultaLoteFromJson(loteAndConsultasDTO);

            // Verificar
            Assert.IsType<BadRequestObjectResult>(result);
        }



        [Fact]
        public async Task CreateConsultaLoteFromJson_ReturnsBadRequest_WhenAddingLoteFails()
        {
            // Preparar
            var loteAndConsultasDTO = new LoteAndConsultasDTO();
            
            var controller = CreateController(); // Inicializar el controlador antes de usarlo.
            ConfigureUserControllerContext(controller, "test@example.com");
            
            _mockUsuarioRepo.Setup(r => r.ObtenerIdUsuarioPorEmailAsync(It.IsAny<string>())).ReturnsAsync(1);
            _mockLoteRepo.Setup(r => r.AgregarLoteAsync(It.IsAny<LoteDTO>(), It.IsAny<Dictionary<string, Fecha>>(), It.IsAny<Dictionary<string, Sexo>>(), It.IsAny<Dictionary<string, Atributo>>(), It.IsAny<int>())).ReturnsAsync((Lote)null);

            // Actuar
            var result = await controller.CreateConsultaLoteFromJson(loteAndConsultasDTO);

            // Verificar
            Assert.IsType<BadRequestObjectResult>(result);
        }


        [Fact]
        public async Task CreateConsultaLoteFromJson_ReturnsOk_WhenProcessIsSuccessful()
        {
            // Preparar
            var loteAndConsultasDTO = new LoteAndConsultasDTO
            {
                Lote = new LoteDTO
                {
                    Anyos = new List<string> { "2021", "2022" },
                    Atributos = new List<string> { "Atributo1", "Atributo2" },
                    Sexos = new List<string> { "Masculino", "Femenino" }
                },
                Consultas = new List<JsonConsulta>
                {
                    new JsonConsulta 
                    {
                        CodPostal = "28001",
                        TipoVia = "Calle",
                        NombVia = "Gran Vía",
                        NumVia = 1,
                        NombProvincia = "Madrid",
                        NombMunicipio = "Madrid"
                    },
                    new JsonConsulta 
                    {
                        CodPostal = "46001",
                        TipoVia = "Avenida",
                        NombVia = "Del Cid",
                        NumVia = 45,
                        NombProvincia = "Valencia",
                        NombMunicipio = "Valencia"
                    }
                }
            };
            var controller = CreateController();
            ConfigureUserControllerContext(controller, "test@example.com");

            _mockUsuarioRepo.Setup(r => r.ObtenerIdUsuarioPorEmailAsync(It.IsAny<string>())).ReturnsAsync(1);
            _mockLoteRepo.Setup(r => r.AgregarLoteAsync(It.IsAny<LoteDTO>(), It.IsAny<Dictionary<string, Fecha>>(), It.IsAny<Dictionary<string, Sexo>>(), It.IsAny<Dictionary<string, Atributo>>(), It.IsAny<int>())).ReturnsAsync(new Lote() { LoteId = 1 });
            _mockDatoRepo.Setup(r => r.ObtenerDatosSocioeconomicosBDPorLoteAsync(It.IsAny<LoteAndConsultasDTO>(), It.IsAny<int>())).ReturnsAsync(new OutputConsultasDTO());

            // Añade los mocks para las consultas
            _mockLoteRepo.Setup(r => r.ContarConsultasDelDiaPorUsuarioAsync(It.IsAny<int>())).ReturnsAsync(0);
            _mockUsuarioRepo.Setup(r => r.ObtenerNumeroMaximoConsultasAsync(It.IsAny<int>())).ReturnsAsync(50);

            // Actuar
            var result = await controller.CreateConsultaLoteFromJson(loteAndConsultasDTO);

            // Verificar
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsType<OutputConsultasDTO>(okResult.Value);
        }



        [Fact]
        public async Task GetProvincias_ReturnsOk_WithListOfProvincias()
        {
            // Preparar
            var expectedProvincias = new List<string> { "Provincia1", "Provincia2", "Provincia3" };
            _mockProvinciaRepo.Setup(r => r.ObtenerTodasLasProvinciasAsync()).ReturnsAsync(expectedProvincias);

            var controller = CreateController();

            // Actuar
            var result = await controller.GetProvincias();

            // Verificar
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(expectedProvincias, okResult.Value);
        }


        [Fact]
        public async Task GetProvincias_ReturnsOk_WithEmptyList_WhenNoProvinciasAvailable()
        {
            // Preparar
            _mockProvinciaRepo.Setup(r => r.ObtenerTodasLasProvinciasAsync()).ReturnsAsync(new List<string>());

            var controller = CreateController();

            // Actuar
            var result = await controller.GetProvincias();

            // Verificar
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Empty(okResult.Value as IEnumerable<string>);
        }


        [Fact]
        public async Task GetProvincias_ReturnsServerError_WhenExceptionThrown()
        {
            // Preparar
            _mockProvinciaRepo.Setup(r => r.ObtenerTodasLasProvinciasAsync()).ThrowsAsync(new Exception("Database error"));

            var controller = CreateController();

            // Actuar
            var result = await controller.GetProvincias();

            // Verificar
            Assert.IsType<StatusCodeResult>(result);
            var errorResult = result as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.InternalServerError, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetMunicipiosPorNombreProvincia_ReturnsMunicipiosList_WhenNombreProvinciaIsValid()
        {
            // Preparar
            var provincia = "Sevilla";
            var municipiosExpected = new List<string> { "Municipio1", "Municipio2" };
            _mockMunicipioRepo.Setup(r => r.ObtenerNombresMunicipiosPorNombreProvinciaAsync(provincia)).ReturnsAsync(municipiosExpected);
            var controller = CreateController();

            // Actuar
            var result = await controller.GetMunicipiosPorNombreProvincia(provincia);

            // Verificar
            var okResult = Assert.IsType<OkObjectResult>(result);
            var municipiosReturned = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(municipiosExpected, municipiosReturned);
        }

        [Fact]
        public async Task GetMunicipiosPorNombreProvincia_ReturnsNotFound_WhenNombreProvinciaIsNotValid()
        {
            // Preparar
            var provincia = "Inexistente";
            _mockMunicipioRepo.Setup(r => r.ObtenerNombresMunicipiosPorNombreProvinciaAsync(provincia)).ReturnsAsync((List<string>)null);
            var controller = CreateController();

            // Actuar
            var result = await controller.GetMunicipiosPorNombreProvincia(provincia);

            // Verificar
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetMunicipiosPorNombreProvincia_ReturnsServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockMunicipioRepo.Setup(m => m.ObtenerNombresMunicipiosPorNombreProvinciaAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = CreateController();

            // Act
            var result = await controller.GetMunicipiosPorNombreProvincia("SomeProvincia");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
            Assert.Equal("Database error", objectResult.Value);
        }

        [Fact]
        public async Task GetFechas_ReturnsOk_WhenDatesAreFound()
        {
            // Arrange
            _mockViasRepo.Setup(repo => repo.ObtenerFechasViasAsync()).ReturnsAsync(new List<string> { "2021", "2022" });
            var controller = CreateController();

            // Act
            var result = await controller.GetFechas();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<HashSet<string>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetFechas_ReturnsNotFound_WhenNoDatesFound()
        {
            // Arrange
            _mockViasRepo.Setup(repo => repo.ObtenerFechasViasAsync()).ReturnsAsync(new List<string>());
            var controller = CreateController();

            // Act
            var result = await controller.GetFechas();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFechas_ReturnsServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockViasRepo.Setup(repo => repo.ObtenerFechasViasAsync()).Throws(new Exception("Database error"));
            var controller = CreateController();

            // Act
            var result = await controller.GetFechas();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("Database error", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetAtributos_ReturnsOk_WhenAttributesAreFound()
        {
            // Arrange
            _mockAtributoRepo.Setup(repo => repo.ObtenerNombresAtributosAsync()).ReturnsAsync(new List<string> { "Atributo1", "Atributo2" });
            var controller = CreateController();

            // Act
            var result = await controller.GetAtributos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetAtributos_ReturnsNotFound_WhenNoAttributesFound()
        {
            // Arrange
            _mockAtributoRepo.Setup(repo => repo.ObtenerNombresAtributosAsync()).ReturnsAsync(new List<string>());
            var controller = CreateController();

            // Act
            var result = await controller.GetAtributos();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAtributos_ReturnsServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockAtributoRepo.Setup(repo => repo.ObtenerNombresAtributosAsync()).Throws(new Exception("Database error"));
            var controller = CreateController();

            // Act
            var result = await controller.GetAtributos();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("Database error", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetSexos_ReturnsOk_WhenSexosAreFound()
        {
            // Arrange
            _mockSexoRepo.Setup(repo => repo.ObtenerValoresSexoAsync()).ReturnsAsync(new List<string> { "Masculino", "Femenino" });
            var controller = CreateController();

            // Act
            var result = await controller.GetSexos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetSexos_ReturnsNotFound_WhenNoSexosFound()
        {
            // Arrange
            _mockSexoRepo.Setup(repo => repo.ObtenerValoresSexoAsync()).ReturnsAsync(new List<string>());
            var controller = CreateController();

            // Act
            var result = await controller.GetSexos();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetSexos_ReturnsServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockSexoRepo.Setup(repo => repo.ObtenerValoresSexoAsync()).Throws(new Exception("Database error"));
            var controller = CreateController();

            // Act
            var result = await controller.GetSexos();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("Database error", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetTipoVias_ReturnsOk_WhenTiposAreFound()
        {
            // Arrange
            _mockViasRepo.Setup(repo => repo.ObtenerTiposViasAsync()).ReturnsAsync(new List<string> { "Avenida", "Calle" });
            var controller = CreateController();

            // Act
            var result = await controller.GetTipoVias();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetTipoVias_ReturnsNotFound_WhenNoTiposFound()
        {
            // Arrange
            _mockViasRepo.Setup(repo => repo.ObtenerTiposViasAsync()).ReturnsAsync(new List<string>());
            var controller = CreateController();

            // Act
            var result = await controller.GetTipoVias();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTipoVias_ReturnsServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockViasRepo.Setup(repo => repo.ObtenerTiposViasAsync()).Throws(new Exception("Database error"));
            var controller = CreateController();

            // Act
            var result = await controller.GetTipoVias();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("Database error", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetTramosByMunicipio_ReturnsOk_WhenTramosAreFound()
        {
            // Arrange
            var expectedTramos = new TramoInfo
            {
                CodigosPostales = new List<string> { "28001", "28002" },
                NombresVias = new List<string> { "Calle A", "Calle B" },
                TiposVias = new List<string> { "Calle", "Avenida" }
            };
            _mockTramosRepo.Setup(repo => repo.ObtenerTramosConViasPorNombreMunicipio(It.IsAny<string>())).ReturnsAsync(expectedTramos);
            var controller = CreateController();

            // Act
            var result = await controller.GetTramosByMunicipio("Madrid");

            // Assert
            var actionResult = Assert.IsType<ActionResult<TramoInfo>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<TramoInfo>(okResult.Value);

            Assert.Equal(expectedTramos.CodigosPostales, returnValue.CodigosPostales);
            Assert.Equal(expectedTramos.NombresVias, returnValue.NombresVias);
            Assert.Equal(expectedTramos.TiposVias, returnValue.TiposVias);
        }

        [Fact]
        public async Task GetTramosByMunicipio_ReturnsNotFound_WhenNoTramosFound()
        {
            // Arrange
            _mockTramosRepo.Setup(repo => repo.ObtenerTramosConViasPorNombreMunicipio(It.IsAny<string>())).ReturnsAsync((TramoInfo)null);
            var controller = CreateController();

            // Act
            var result = await controller.GetTramosByMunicipio("Madrid");

            // Assert
            var actionResult = Assert.IsType<ActionResult<TramoInfo>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetDatosPorNombreVia_ReturnsOk_WhenDatosAreFound()
        {
            // Arrange
            var mockRequest = new ViaRequest 
            { 
                NombreVia = "SomeName", 
                NombreMunicipio = "SomeMunicipio", 
                NombreProvincia = "SomeProvincia" 
            };
            _mockTramosRepo.Setup(repo => repo.ObtenerTramosPorNombreVia(mockRequest.NombreVia, mockRequest.NombreMunicipio, mockRequest.NombreProvincia)).ReturnsAsync(new TramoInfo());
            var controller = CreateController();

            // Act
            var result = await controller.GetDatosPorNombreVia(mockRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TramoInfo>>(result);
            Assert.IsType<OkObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetDatosPorNombreVia_ReturnsNotFound_WhenNoDatosFound()
        {
            // Arrange
            var mockRequest = new ViaRequest 
            { 
                NombreVia = "SomeName", 
                NombreMunicipio = "SomeMunicipio", 
                NombreProvincia = "SomeProvincia" 
            };
            _mockTramosRepo.Setup(repo => repo.ObtenerTramosPorNombreVia(mockRequest.NombreVia, mockRequest.NombreMunicipio, mockRequest.NombreProvincia)).ReturnsAsync((TramoInfo)null);
            var controller = CreateController();

            // Act
            var result = await controller.GetDatosPorNombreVia(mockRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TramoInfo>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }





        // Fin del archivo...
    }
}
