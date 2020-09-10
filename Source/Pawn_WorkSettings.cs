﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimThreaded
{

    public class Pawn_WorkSettings_Patch
	{
        public static AccessTools.FieldRef<Pawn_WorkSettings, List<WorkGiver>> workGiversInOrderNormal =
            AccessTools.FieldRefAccess<Pawn_WorkSettings, List<WorkGiver>>("workGiversInOrderNormal");
        public static AccessTools.FieldRef<Pawn_WorkSettings, bool> workGiversDirty =
            AccessTools.FieldRefAccess<Pawn_WorkSettings, bool>("workGiversDirty");

        public static bool CacheWorkGiversInOrder(Pawn_WorkSettings __instance)
        {
            //Pawn_WorkSettings.wtsByPrio.Clear();
            List<WorkTypeDef> wtsByPrio = new List<WorkTypeDef>();
            List<WorkTypeDef> defsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            int num1 = 999;
            for (int index = 0; index < defsListForReading.Count; ++index)
            {
                WorkTypeDef w = defsListForReading[index];
                int priority = __instance.GetPriority(w);
                if (priority > 0)
                {
                    if (priority < num1 && w.workGiversByPriority.Any<WorkGiverDef>((Predicate<WorkGiverDef>)(wg => !wg.emergency)))
                        num1 = priority;
                    wtsByPrio.Add(w);
                }
            }
            wtsByPrio.InsertionSort<WorkTypeDef>((Comparison<WorkTypeDef>)((a, b) =>
            {
                float num2 = (float)(a.naturalPriority + (4 - __instance.GetPriority(a)) * 100000);
                return ((float)(b.naturalPriority + (4 - __instance.GetPriority(b)) * 100000)).CompareTo(num2);
            }));
            //this.workGiversInOrderEmerg.Clear();
            List<WorkGiver> workGiversInOrderEmerg = new List<WorkGiver>();
            for (int index1 = 0; index1 < wtsByPrio.Count; ++index1)
            {
                WorkTypeDef workTypeDef = wtsByPrio[index1];
                for (int index2 = 0; index2 < workTypeDef.workGiversByPriority.Count; ++index2)
                {
                    WorkGiver worker = workTypeDef.workGiversByPriority[index2].Worker;
                    if (worker.def.emergency && __instance.GetPriority(worker.def.workType) <= num1)
                        workGiversInOrderEmerg.Add(worker);
                }
            }
            lock (workGiversInOrderNormal(__instance))
            {
                workGiversInOrderNormal(__instance).Clear();
            }
            for (int index1 = 0; index1 < wtsByPrio.Count; ++index1)
            {
                WorkTypeDef workTypeDef = wtsByPrio[index1];
                for (int index2 = 0; index2 < workTypeDef.workGiversByPriority.Count; ++index2)
                {
                    WorkGiver worker = workTypeDef.workGiversByPriority[index2].Worker;
                    if (!worker.def.emergency || __instance.GetPriority(worker.def.workType) > num1)
                    {
                        lock (workGiversInOrderNormal(__instance))
                        {
                            workGiversInOrderNormal(__instance).Add(worker);
                        }
                    }
                }
            }
            workGiversDirty(__instance) = false;
            return false;
        }


    }
}
