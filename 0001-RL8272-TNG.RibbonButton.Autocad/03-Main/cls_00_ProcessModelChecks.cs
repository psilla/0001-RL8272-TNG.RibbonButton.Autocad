using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad;
using static TYPSA.SharedLib.Autocad.cls_00_ProcessCommonModelChecks;

namespace TNG.RibbonButton.Autocad
{
    internal class cls_00_ProcessModelChecks
    {
        public static void ProcessModelChecks(
            List<string> selectedOptions,
            ModelCheckerKeys keys,
            Transaction tr,
            Database db,
            BlockTable bt,
            string fileName,
            List<WarningCheckLogResult> warningChecksLog,
            ModelCheckerResults resultsfromcad,
            List<Dictionary<string, object>> extractedData,
            bool isSpanish
        )
        {
            // -----------------------------
            // Procesar chequeos comunes
            // -----------------------------

            ProcessCommonCadChecks(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                isSpanish, TngModelCheckerDefaults.ExpectedPaperTextFont, applyCleanCheckName: false
            );

            // -----------------------------
            // Procesar checks CAD
            // -----------------------------

            ProcessTngCadChecks(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData
            );
        }


    }
}
