﻿using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace ServerSide
{
    partial class MainClass
    {
        [RemoteEvent("diver_startJob")]
        public void Diver_StartJob(Player player)
        {
            Diver.StartJob(player);
        }
        [RemoteEvent("diver_payment")]
        public void Diver_Payment(Player player, int priceMult)
        {
            PayoutManager.DiverPayment(player, priceMult);
        }
    }
}
