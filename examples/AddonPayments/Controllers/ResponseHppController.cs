using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class ResponseHppController : ControllerBase
    {
        [HttpGet]
        public IActionResult ResponseHpp() {

            // configure client & request settings
            var service = new HostedService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://hpp.sandbox.addonpayments.com/pay"
            });

            var responseJson = Request.Form["hppResponse"];
            
            try {
                // Obtenemos la respuesta
                Transaction response = service.ParseResponse(responseJson, true);

                var orderId = response.OrderId; // GTI5Yxb0SumL_TkDMCAxQA
                var responseCode = response.ResponseCode; // 00
                var responseMessage = response.ResponseMessage; // [ test system ] Authorised
                var responseValues = response.ResponseValues; // get values accessible by key
                var fraudFilterResult = responseValues["HPP_FRAUDFILTER_RESULT"]; // PASS


                return Content(responseJson);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return Ok(respuesta);
            }
        }
    }
}
