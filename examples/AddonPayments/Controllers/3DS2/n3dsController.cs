using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AddonPayments.Controllers._3DS2
{
    [Route("api/[controller]")]
    [ApiController]
    public class n3dsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Notification3Ds(MethodUrlResponse detalles)
        {
            /*
              * Este código de muestra está pensado como un simple ejemplo y no debe tratarse como un código listo para producción 
              * Necesitarás añadir tu propio análisis de mensajes y seguridad en línea con tu aplicación o sitio web
              */
            var threeDSMethodData = Request.Form["threeDSMethodData"];

            // Ejemplo de la respuesta del ACS para la notificación
            // threeDSMethodData = "eyJ0aHJlZURTU2VydmVyVHJhbnNJRCI6ImFmNjVjMzY5LTU5YjktNGY4ZC1iMmY2LTdkN2Q1ZjVjNjlkNSJ9";

            try
            {
                byte[] data = Convert.FromBase64String(threeDSMethodData);
                string methodUrlResponseString = Encoding.UTF8.GetString(data);

                // Mpaear la respuesta a la clase personalizada MethodUrlResponse
                MethodUrlResponse methodUrlResponse = JsonConvert.DeserializeObject<MethodUrlResponse>(methodUrlResponseString);

                string threeDSServerTransID = methodUrlResponse.ThreeDSServerTransID; // af65c369-59b9-4f8d-b2f6-7d7d5f5c69d5

                // TODO: notificar al cliente que el paso de la URL del método se ha completado
                return Ok(threeDSServerTransID);
            }

            catch (Exception exce)
            {
                return BadRequest(exce);
                // TODO: agregue su manejo de excepciones aquí
            }

        }
    }
}
