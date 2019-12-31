﻿using SeasonBackend.Protos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeasonBackend.Database
{
    public class HosterInformation
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public HosterType HosterType { get; set; }
    }
}
