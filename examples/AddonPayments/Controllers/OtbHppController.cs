using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class OtbHppController : ControllerBase
    {
        [HttpGet]
        public IActionResult Otbhpp() {

            // configure client & request settings
            var service = new HostedService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "internet",
                SharedSecret = "secret",
                ServiceUrl = "https://hpp.sandbox.addonpayments.com/pay",
                HostedPaymentConfig = new HostedPaymentConfig
                {
                    Version = "2"
                }
            });

            try {
                // process an auto-capture authorization
               var hppJson = service.Verify()
                    .WithCurrency("EUR")
                    .Serialize();

                return Content(hppJson);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return Ok(respuesta);
            }
        }
    }
}
