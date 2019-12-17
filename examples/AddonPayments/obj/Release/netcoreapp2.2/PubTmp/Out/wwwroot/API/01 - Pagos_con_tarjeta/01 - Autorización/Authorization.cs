    using GlobalPayments.Api;
    using GlobalPayments.Api.Entities;
    using GlobalPayments.Api.PaymentMethods;

namespace AddonPayments.net.API._01___Autorización {




    public class Authorization {
        public void Auth() {
            // Configura los ajustes de cliente y request
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonphptest",                                    // Idenftificador del comercio
                AccountId = "api",                                              // Subcuenta
                SharedSecret = "secret",                                        // Contraseña para realizar transacciones
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"  // URL de Addon Payments donde se envían las peticiones

            });

            // Creamos el objeto Card
            var card = new CreditCardData {
                Number = "4263970000005262",
                ExpMonth = 12,
                ExpYear = 2025,
                Cvn = "131",
                CardHolderName = "James Mason"
            };

            try {
                // procesamos una auto-liquidacion para la authorization
                var response = card.Charge(129.99m)
                               .WithCurrency("EUR")
                               .Execute();

                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED

                // obtenemos los detalles de la respuesta para guardarlos en la BDD para futuras solicitudes de administracion de la transaccion
                var orderId = response.OrderId;
                var authCode = response.AuthorizationCode;
                var paymentsReference = response.TransactionId; // pasref

                // Otros datos devueltos por la respuesta son                
                var authorizationCode = response.AuthorizationCode;
                var avsResponseCode = response.AvsResponseCode;
                var cvnResponseCode = response.CvnResponseCode;
                var multicapture = response.MultiCapture;           // su valor debe ser en esta caso = false
                var paymentMethodType = response.PaymentMethodType; // su valor debe ser en este caso = Credit
                var schemeId = response.SchemeId;
                var timeStamp = response.Timestamp;
                var transactionId = response.TransactionId;
            }

            catch (ApiException exce) {
                // TODO: añade tu manejador de errores aqui
            }
        }
    }
}
