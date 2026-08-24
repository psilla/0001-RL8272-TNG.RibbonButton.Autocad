using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.Main;

namespace TYPSA.PS.RibbonButton.Autocad
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
            cls_00_GetDataCadModelChecker.ProcessProjectUnits(
                selectedOptions, keys, db, fileName, warningChecksLog, resultsfromcad, extractedData,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessLayersInUse(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessLayerZero(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessVersion(
                selectedOptions, keys, db, fileName, warningChecksLog, resultsfromcad, extractedData,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessXrefs(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessPaperTextFont(
                selectedOptions, keys, tr, db, fileName, warningChecksLog, resultsfromcad, extractedData,
                TngModelCheckerDefaults.ExpectedPaperTextFont,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessEntitiesByLayer(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                isSpanish,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessRevisionClouds(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessBlockAttributes(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                TngModelCheckerDefaults.BlockAttributesName,
                applyCleanCheckName: false
            );

            cls_00_GetDataCadModelChecker.ProcessPlotTag(
                selectedOptions, keys, tr, db, bt, fileName, warningChecksLog, resultsfromcad, extractedData,
                TngModelCheckerDefaults.PlotTagReferenceTexts,
                TngModelCheckerDefaults.PlotTagBlockName,
                applyCleanCheckName: false
            );
        }
    }
}
