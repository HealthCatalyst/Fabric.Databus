// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValuesController.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ValuesController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.TestApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Microsoft.AspNetCore.Mvc;

    /// <inheritdoc />
    /// <summary>
    /// The values controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// GET api/values
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Console.WriteLine("GET");
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// GET api/values/5
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// POST api/values
        /// </summary>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        [HttpPost]
        public JsonResult Post()
        {
            Console.WriteLine("--------------- POST ------------------------");
            var headerDictionary = this.Request.Headers;
            if (headerDictionary != null)
            {
                foreach (var requestHeader in headerDictionary)
                {
                    Console.WriteLine($"{requestHeader.Key}: {requestHeader.Value}");
                }
            }

            var requestBody = this.Request.Body;
            if (requestBody != null)
            {
                using (var reader = new StreamReader(requestBody, Encoding.UTF8))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }

            Console.WriteLine("--------------- END POST ------------------------");
            return new JsonResult(new { Message = "Received" });
        }

        /// <summary>
        /// PUT api/values/5
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        [HttpPut("{id}")]
        public JsonResult Put(int id, [FromBody] string value)
        {
            Console.WriteLine("------------------ PUT -------------");
            var headerDictionary = this.Request.Headers;
            if (headerDictionary != null)
            {
                foreach (var requestHeader in headerDictionary)
                {
                    Console.WriteLine($"{requestHeader.Key}: {requestHeader.Value}");
                }
            }

            var requestBody = this.Request.Body;
            if (requestBody != null)
            {
                using (var reader = new StreamReader(requestBody, Encoding.UTF8))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }

            Console.WriteLine("------------------ END PUT -------------");
            return new JsonResult(new { Message = "Received" });
        }

        /// <summary>
        /// DELETE api/values/5
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
