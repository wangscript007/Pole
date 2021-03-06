﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Product.Api.Grains.Abstraction;

namespace Product.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IClusterClient clusterClient;
        public ProductTypeController(IClusterClient clusterClient)
        {
            this.clusterClient = clusterClient;
        }
        [HttpGet("AddProductType")]
        public void AddProductType(string name = "test")
        {
            var newId = "da8a489fa7b4409294ee1b358fbbfba5";
            var grain = clusterClient.GetGrain<IProductTypeGrain>(newId);
            grain.AddProductType(newId, name);
        }
    }
}