﻿using System;
using System.Threading.Tasks;

namespace Server_Control
{
    internal class Start
    {
        static async Task Main(string[] args)
        {
            Server server = new Server("192.168.0.136", 5345);
            await server.Start();
        }
    }
}