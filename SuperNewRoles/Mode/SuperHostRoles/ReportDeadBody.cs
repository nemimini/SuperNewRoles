﻿using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class ReportDeadBody
    {
        public static bool ReportDeadBodyPatch(PlayerControl __instance,GameData.PlayerInfo target)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            //会議ボタンでもレポートでも起こる処理

            if (target == null) {
                //会議ボタンのみで起こる処理

                return true;
            };

            //死体レポートのみで起こる処理
            DeadPlayer deadPlayer;
            deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == PlayerControl.LocalPlayer.PlayerId)?.FirstOrDefault();
            //if (RoleClass.Bait.ReportedPlayer.Contains(target.PlayerId)) return true;
            if (__instance.isRole(CustomRPC.RoleId.Minimalist))
            {
                var a = RoleClass.Minimalist.UseReport;
                return a;
            }
            //if (target.Object.isRole(CustomRPC.RoleId.Bait) && (!deadPlayer.killerIfExisting.isRole(CustomRPC.RoleId.Minimalist) || RoleClass.Minimalist.UseReport)) if (!RoleClass.Bait.ReportedPlayer.Contains(target.PlayerId)) { return false; } else { return true; }
            return true;
        }
    }
}
