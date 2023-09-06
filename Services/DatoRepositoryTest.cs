using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TFGv3Net7.Models;
using TFGv3Net7.Data;
using TFGv3Net7.Services;
using TFGv3Net7.Registros;

namespace TFGv3Net7.Tests.Services
{
    public class DatoRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly DatoRepository _repo;

        public DatoRepositoryTests()
        {
            // Crear un nuevo proveedor de servicios y registrar el proveedor InMemory
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Configurar las opciones para usar InMemory con el nuevo proveedor de servicios
            var options = new DbContextOptionsBuilder<TfgPrimeroContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _context = new TfgPrimeroContext(options);
            _repo = new DatoRepository(_context);
        }


        private void InitializeSampleData()
        {
            _context.Datos.AddRange(new List<Dato>
            {
                new Dato
                {
                    AtributoIdAtributo = 1,
                    SexoIdSexo = 1,
                    FechaIdFecha = 1,
                    IdSeccion = "Seccion1",
                    IdDistrito = "Distrito1",
                    IdMunicipio = "Municipio1",
                    IdProvincia = "Provincia1",
                    Valor = "1000"
                },
                new Dato
                {
                    AtributoIdAtributo = 2,
                    SexoIdSexo = 2,
                    FechaIdFecha = 2,
                    IdSeccion = "Seccion2",
                    IdDistrito = "Distrito2",
                    IdMunicipio = "Municipio2",
                    IdProvincia = "Provincia2",
                    Valor = "2000"
                },
                new Dato
                {
                    AtributoIdAtributo = 3,
                    SexoIdSexo = 3,
                    FechaIdFecha = 3,
                    IdSeccion = "Seccion3",
                    IdDistrito = "Distrito3",
                    IdMunicipio = "Municipio3",
                    IdProvincia = "Provincia3",
                    Valor = "3000"
                }
            });

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task ObtenerTodosLosDatosAsync_ShouldReturnAllDatos()
        {
            // Arrange
            InitializeSampleData();

            // Act
            var result = await _repo.ObtenerTodosLosDatosAsync();

            // Assert
            Assert.Equal(3, result.Count()); // Esperamos que haya 3 datos en la base de datos
        }

        [Fact]
        public async Task ObtenerTodosLosDatosAsync_ShouldReturnEmptyList_WhenNoData()
        {
            // Act
            var result = await _repo.ObtenerTodosLosDatosAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerDatoPorIdAsync_ShouldReturnCorrectDato()
        {
            // Arrange
            InitializeSampleData();
            int atrIdAtributo = 2;
            int sexIdSexo = 2;
            int fechaIdFecha = 2;
            string secCensIdSeccion = "Seccion2";
            string secCensDistIdDistrito = "Distrito2";
            string secCensDistMunIdMunicipio = "Municipio2";
            string secCensDistMunProvIdProv = "Provincia2";

            // Act
            var result = await _repo.ObtenerDatoPorIdAsync(atrIdAtributo, sexIdSexo, fechaIdFecha, secCensIdSeccion, secCensDistIdDistrito, secCensDistMunIdMunicipio, secCensDistMunProvIdProv);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(atrIdAtributo, result.AtributoIdAtributo);
            Assert.Equal(sexIdSexo, result.SexoIdSexo);
            //... asertar las otras propiedades
        }

        [Fact]
        public async Task ObtenerDatoPorIdAsync_ShouldReturnNull_WhenDatoNotFound()
        {
            // Arrange
            InitializeSampleData();
            int atrIdAtributo = 99; // Valores que sabemos no existen en la base de datos
            int sexIdSexo = 99;
            int fechaIdFecha = 99;
            string secCensIdSeccion = "Seccion99";
            string secCensDistIdDistrito = "Distrito99";
            string secCensDistMunIdMunicipio = "Municipio99";
            string secCensDistMunProvIdProv = "Provincia99";

            // Act
            var result = await _repo.ObtenerDatoPorIdAsync(atrIdAtributo, sexIdSexo, fechaIdFecha, secCensIdSeccion, secCensDistIdDistrito, secCensDistMunIdMunicipio, secCensDistMunProvIdProv);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerDatosNoCoincidentesPorLoteAsync_ShouldConvertRegistroRentaToDatoCorrectly()
        {
            // Arrange
            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta
                {
                    Atributo = "Atributo1",
                    Sexo = "Masculino",
                    Fecha = "2021",
                    ID_Seccion = "Seccion1",
                    ID_Distrito = "Distrito1",
                    ID_Municipio = "Municipio1",
                    ID_Provincia = "Provincia1",
                    Dato = "100"
                }
            };

            var fechas = new Dictionary<string, int> { { "2021", 1 } };
            var sexos = new Dictionary<string, int> { { "Masculino", 1 } };
            var atributos = new Dictionary<string, int> { { "Atributo1", 1 } };

            // Act
            var result = await _repo.ObtenerDatosNoCoincidentesPorLoteAsync(registrosRenta, fechas, sexos, atributos);

            // Assert
            var dato = result.First();
            Assert.Equal(atributos["Atributo1"], dato.AtributoIdAtributo);
            Assert.Equal(sexos["Masculino"], dato.SexoIdSexo);
            Assert.Equal(fechas["2021"], dato.FechaIdFecha);
            //... assert other properties as necessary
        }

        [Fact]
        public async Task AñadirDatoAsync_ShouldRollbackTransactionOnFailure()
        {
            // Arrange
            var dato = new Dato
            {
                AtributoIdAtributo = 1, 
                // Omitiendo algunos valores necesarios para causar un error.
                // Puede omitir aquellos que, si faltan, causarían un error de validación de Entity Framework.
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => _repo.AñadirDatoAsync(dato));

            var result = await _context.Datos.FindAsync(dato.Id);
            Assert.Null(result); // Asegurando que el dato no se haya agregado
        }






    }
}
