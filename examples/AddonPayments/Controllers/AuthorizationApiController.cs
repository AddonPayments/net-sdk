using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;

namespace AddonPayments.Controllers {
    [ApiController]
    [Route("[Controller]")]
    public class AuthorizationApiController: ControllerBase {
        [HttpPost]
        public IActionResult Auth(Tarjeta detalles) {

            // Configuramos los datos de nuestro comercio. Los datos son proporcionados por el equipo de Addon Payments, 
            // en caso de duda debe llamar al 914 353 028 e indicar su número de comercio o Merchant ID
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // Creamos el objeto de la tarjeta
            var card = new CreditCardData {
                Number = detalles.cardNumber,
                ExpMonth = detalles.month,
                ExpYear = detalles.year,
                Cvn = detalles.cvn,
                CardHolderName = detalles.cardholderName
            };

            try {
                // Proceso de autorización automática
                Transaction response = card.Authorize(detalles.importe)
                   .WithCurrency("EUR")
                   .Execute();

                // Obtenemos la respuesta y la mostramos
                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED
                var orderId = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw
                var authCode = response.AuthorizationCode; // 12345
                var paymentsReference = response.TransactionId; // pasref 14622680939731425
                var schemeReferenceData = response.SchemeId; // MMC0F00YE4000000715

                // Aconsejamos guardar los datos de la operacion en su base de datos en caso de querer gestionar de forma interna sus transacciones

                // Devolvemos el resultado de la operación al cliente
                Respuesta respuesta = new Respuesta {
                    result = result,
                    message = message,
                    orderId = orderId,
                    authCode = authCode,
                    paymentsReference = paymentsReference,
                    schemeReferenceData = schemeReferenceData
                };

                return Ok(respuesta);

            } catch (ApiException exce) {
                // En caso de error informamos al cliente
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return Ok(respuesta);
            }
        }
    }
    public class Tarjeta
    {
        public string cardNumber { set; get; }
        public string cardholderName { set; get; }
        public string cvn { set; get; }
        public int month { set; get; }
        public int year { set; get; }
        public decimal importe { set; get; }

    }

    public class Respuesta
    {
        public string result { set; get; }
        public string message { set; get; }
        public string orderId { set; get; }
        public string authCode { set; get; }
        public string paymentsReference { set; get; }
        public string schemeReferenceData { set; get; }
        public string hppPayByLink { set; get; }
    }

    public class RespuestaVersion3DS2
    {
        public string acsEndVersion { set; get; }
        public string acsStartVersion { set; get; }
        public string acsTransactionId { set; get; }
        public int algorithm { set; get; }
        public string authenticationSource { set; get; }
        public string authenticationType { set; get; }
        public string authenticationValue { set; get; }
        public string cardHolderResponseInfo { set; get; }
        public string cavv { set; get; }
        public bool challengeMandated { set; get; }
        public string criticalityIndicator { set; get; }
        public string directoryServerEndVersion { set; get; }
        public string directoryServerStartVersion { set; get; }
        public string directoryServerTransactionId { set; get; }
        public int? eci { set; get; }
        public string enrolled { set; get; }
        public string issuerAcsUrl { set; get; }
        public string messageCategory { set; get; }
        public string messageExtensionId { set; get; }
        public string messageExtensionName { set; get; }
        public string messageVersion { set; get; }
        public string payerAuthenticationRequest { set; get; }
        public string paymentDataSource { set; get; }
        public string paymentDataType { set; get; }
        public string sdkInterface { set; get; }
        public System.Collections.Generic.IEnumerable<string> sdkUiType { set; get; }
        public string serverTransactionId { set; get; }
        public string status { set; get; }
        public string statusReason { set; get; }
        public string xid { set; get; }
    }

    public class NavegadorCliente
    {
        public ColorDepth colorDepth { set; get; }
        public bool javaEnabled { set; get; }
        public string browserLanguage { set; get; }
        public int screenHeight { set; get; }
        public int screenWidth { set; get; }
        public string userAgent { set; get; }
        public string browserTimezoneZoneOffset { set; get; }
    }

    public class Op
    {
        public string importe { set; get; }
        public string OrderID { set; get; }
        public string pasref { set; get; }
        public string authCode { set; get; }

    }

    public class Fraud
    {
        public string custnum { set; get; }
        public string varref { set; get; }
        public string prodid { set; get; }
        public string billCp { set; get; }
        public string billCo { set; get; }
        public string shipCp { set; get; }
        public string shipCo { set; get; }
        public string billingstreet { set; get; }
        public string orderID { set; get; }
        public string Pasref { set; get; }
    }

    public class RespuestaError
    {
        public string resultado { set; get; }
    }

    public class Cliente
    {
        public string titulo { set; get; }
        public string nombre { set; get; }
        public string apellidos { set; get; }
        public string cumple { set; get; }
        public string pass { set; get; }
        public string email { set; get; }
        public string homephone { set; get; }
        public string device { set; get; }
        public string fax { set; get; }
        public string workphone { set; get; }
        public string comments { set; get; }
        public string company { set; get; }
        public string payerref { set; get; }
        public string paymentmethod { set; get; }
        public string suplementary { set; get; }
        public string lang { set; get; }

    }

    public class Billing
    {
        public string billing1 { set; get; }
        public string billing2 { set; get; }
        public string billing3 { set; get; }
        public string billingcity { set; get; }
        public string billingprovince { set; get; }
        public string billingstate { set; get; }
        public string billingcode { set; get; }
        public string billingcountry { set; get; }
    }

    public class Shipping
    {
        public string street1 { set; get; }
        public string street2 { set; get; }
        public string street3 { set; get; }
        public string enviocity { set; get; }
        public string envioprovince { set; get; }
        public string enviostate { set; get; }
        public string enviocode { set; get; }
        public string enviocountry { set; get; }
    }

    public class Datos
    {
        public Tarjeta Tarjeta { set; get; }
        public Op Op { set; get; }
        public Fraud Fraud { set; get; }
        public Cliente Cliente { set; get; }
        public Billing Billing { set; get; }
        public Shipping Shipping { set; get; }

    }

    public class MethodUrlResponse
    {
        public string methodUrlResponseString { set; get; }
        public string ThreeDSServerTransID { set; get; }
    }

    public class ChallengeUrlResponse
    {
        public string ThreeDSServerTransID { set; get; }
        public string AcsTransID { set; get; }
        public string ChallengeCompletionInd { set; get; }
        public string MessageType { set; get; }
        public string MessageVersion { set; get; }
        public string TransStatus { set; get; }
    }
}
