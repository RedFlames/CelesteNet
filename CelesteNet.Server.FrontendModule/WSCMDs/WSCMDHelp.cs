﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteNet.Server.Control {
    public class WSCMDHelp : WSCMD {
        public override bool Auth => false;
        public override object? Run(object? input) {
            return WS?.Commands.All.Keys.ToList();
        }
    }
}
