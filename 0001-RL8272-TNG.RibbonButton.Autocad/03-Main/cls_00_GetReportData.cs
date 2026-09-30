using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad;
using TYPSA.SharedLib.EndPoints;

namespace TNG.RibbonButton.Autocad
{
    public class cls_00_GetReportData
    {
        public static Dictionary<string, object> GetReportData(
            ModelCheckerKeys keys,
            List<string> selectedOptions,
            ModelCheckerResults resultsfromcad,
            List<WarningCheckLogResult> warningChecksLog,
            Dictionary<string, TimeSpan> processDurations,
            bool isSpanish
        )
        {
            // -----------------------------
            // Validar informacion
            // -----------------------------

            Stopwatch processStopwatch = Stopwatch.StartNew();

            string msg = cls_00_ProcessMessages.ShowProcessMessage(
                isSpanish, cls_00_ProcessMessages.ValidateCollectedData
            );

            bool hasData =
                resultsfromcad.Audit.Any() ||
                resultsfromcad.PurgeableItemsCount.Any() ||
                resultsfromcad.ProjectUnits.Any() ||
                resultsfromcad.LayersInUse.Any() ||
                resultsfromcad.LayerZero.Any() ||
                resultsfromcad.Version.Any() ||
                resultsfromcad.Xrefs.Any() ||
                resultsfromcad.PaperTextFont.Any() ||
                resultsfromcad.ByLayer.Any() ||
                resultsfromcad.RevisionClouds.Any() ||
                resultsfromcad.BlockAttributes.Any() ||
                resultsfromcad.PlotTags.Any();

            cls_00_ProcessMessages.AddProcessDuration(
                processDurations, msg, processStopwatch
            );

            // -----------------------------
            // Validar datos
            // -----------------------------

            if (!hasData && !warningChecksLog.Any())
            {
                MessageBox.Show(
                    "No data found for the selected checks.",
                    "Atenea Model Checker",
                    MessageBoxButtons.OK, MessageBoxIcon.Information
                );
                return null;
            }

            // ---------------------------------
            // Preparar datos exportacion
            // ---------------------------------

            Dictionary<string, object> exportData = cls_00_GetExportData.GetExportData(
                keys, selectedOptions, resultsfromcad
            );

            // -----------------------------
            // Añadir warnings
            // -----------------------------

            if (warningChecksLog.Any())
            {
                exportData.Add(
                    "Warning Selected Checks Log", warningChecksLog
                );
            }

            // -----------------------------
            // Return
            // -----------------------------

            return exportData;
        }


    }
}