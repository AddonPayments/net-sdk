using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using System;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CustomerApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult CustomerCreate(Datos detalles) {
            
            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // Cliente
            var customer = new Customer
            {
                Key = Guid.NewGuid().ToString(),
                Title = detalles.Cliente.titulo,
                FirstName = detalles.Cliente.nombre,
                LastName = detalles.Cliente.apellidos,
                DateOfBirth = detalles.Cliente.cumple,
                CustomerPassword = detalles.Cliente.pass,
                Email = detalles.Cliente.email,
                HomePhone = detalles.Cliente.homephone,
                DeviceFingerPrint = detalles.Cliente.device,
                Fax = detalles.Cliente.fax,
                WorkPhone = detalles.Cliente.workphone,
                Comments = detalles.Cliente.comments,
                Company = detalles.Cliente.company,
                // Dirección de envío
                Address = new Address
                {
                    StreetAddress1 = detalles.Shipping.street1,
                    StreetAddress2 = detalles.Shipping.street2,
                    StreetAddress3 = detalles.Shipping.street3,
                    City = detalles.Shipping.enviocity,
                    Province = detalles.Shipping.envioprovince,
                    PostalCode = detalles.Shipping.enviocode,
                    Country = detalles.Shipping.enviocountry
                }
            };

            try {
                // Creamos el cliente
                var response = customer.Create();
                var payerRef = response.Key;

                Respuesta respuesta = new Respuesta { result = response.Key };

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
    
}
