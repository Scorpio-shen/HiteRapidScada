﻿/*
 * Copyright 2015 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : SCADA-Communicator Service
 * Summary  : ScadaCommSvc service implementation
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2006
 * Modified : 2015
 */

using Scada.Comm.Engine;
using System.ServiceProcess;

namespace Scada.Comm.Svc
{
    /// <summary>
    /// ScadaCommSvc service implementation
    /// <para>Реализация службы ScadaCommSvc</para>
    /// </summary>
    public partial class SvcMain : ServiceBase
    {
        private Manager manager; // менеджер, управляющий работой приложения

        public SvcMain()
        {
            InitializeComponent();
            manager = new Manager();
        }
        public void StartSerice()
        {
            manager.StartService();
        }
        protected override void OnStart(string[] args)
        {
            manager.StartService();
        }

        protected override void OnStop()
        {
            manager.StopService();
        }

        protected override void OnShutdown()
        {
            manager.ShutdownService();
        }
    }
}
