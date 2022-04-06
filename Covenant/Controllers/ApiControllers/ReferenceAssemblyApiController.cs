﻿// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using EasyPeasy.Core;
using EasyPeasy.Models.Grawls;

namespace EasyPeasy.Controllers.ApiControllers
{
    [ApiController, Route("api/referenceassemblies"), Authorize(Policy = "RequireJwtBearer")]
    public class ReferenceAssemblyApiController : Controller
    {
        private readonly IEasyPeasyService _service;

        public ReferenceAssemblyApiController(IEasyPeasyService service)
        {
            _service = service;
        }

        // GET: api/referenceassemblies
        [HttpGet(Name = "GetReferenceAssemblies")]
        public async Task<ActionResult<IEnumerable<ReferenceAssembly>>> GetReferenceAssemblies()
        {
            return Ok(await _service.GetReferenceAssemblies());
        }

        // GET api/referenceassemblies/{id}
        [HttpGet("{id}", Name = "GetReferenceAssembly")]
        public async Task<ActionResult<ReferenceAssembly>> GetReferenceAssembly(int id)
        {
            try
            {
                return await _service.GetReferenceAssembly(id);
            }
            catch (ControllerNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ControllerBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (ControllerUnauthorizedException)
            {
                return new UnauthorizedResult();
            }
        }

        // POST api/referenceassemblies
        [HttpPost(Name = "CreateReferenceAssembly")]
        public async Task<ActionResult<ReferenceAssembly>> CreateReferenceAssembly([FromBody]ReferenceAssembly assembly)
        {
            try
            {
                ReferenceAssembly createdAssembly = await _service.CreateReferenceAssembly(assembly);
                return CreatedAtRoute(nameof(GetReferenceAssembly), new { id = createdAssembly.Id }, createdAssembly);
            }
            catch (ControllerNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ControllerBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (ControllerUnauthorizedException)
            {
                return new UnauthorizedResult();
            }
        }

        // PUT api/referenceassemblies
        [HttpPut(Name = "EditReferenceAssembly")]
        public async Task<ActionResult<ReferenceAssembly>> EditReferenceAssembly([FromBody]ReferenceAssembly assembly)
        {
            try
            {
                return await _service.EditReferenceAssembly(assembly);
            }
            catch (ControllerNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ControllerBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (ControllerUnauthorizedException)
            {
                return new UnauthorizedResult();
            }
        }

        // DELETE api/referenceassemblies/{id}
        [HttpDelete("{id}", Name = "DeleteReferenceAssembly")]
        public async Task<ActionResult> DeleteReferenceAssembly(int id)
        {
            try
            {
                await _service.DeleteReferenceAssembly(id);
                return new NoContentResult();
            }
            catch (ControllerNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ControllerBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (ControllerUnauthorizedException)
            {
                return new UnauthorizedResult();
            }
        }
    }
}
