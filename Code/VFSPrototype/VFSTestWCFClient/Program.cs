﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSTestWCFClient.DiskServiceReference;

namespace VFSTestWCFClient
{
    static class Program
    {
        static void Main(string[] args)
        {
            var d = new DiskServiceClient();
            Console.WriteLine(d.Ish("bubu"));
            Console.ReadLine();
        }
    }
}
