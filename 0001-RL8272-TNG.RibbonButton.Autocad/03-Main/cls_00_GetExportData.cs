using System.Collections.Generic;
using System.Linq;
using TYPSA.SharedLib.Autocad.Main;

namespace TYPSA.PS.RibbonButton.Autocad
{
    internal class cls_00_GetExportData
    {
        public static Dictionary<string, object> GetExportData(

            ModelCheckerKeys keys,
            List<string> selectedOptions,
            ModelCheckerResults resultsfromcad
        )
        {
            Dictionary<string, object> exportData = new Dictionary<string, object>();

            // -----------------------------
            // Project Units
            // -----------------------------

            if (selectedOptions.Contains(keys.ProjectUnits) && resultsfromcad.ProjectUnits.Any())
            {
                exportData.Add(keys.ProjectUnits, resultsfromcad.ProjectUnits);
            }

            // -----------------------------
            // Version
            // -----------------------------

            if (selectedOptions.Contains(keys.Version) && resultsfromcad.Version.Any())
            {
                exportData.Add(keys.Version, resultsfromcad.Version);
            }

            // -----------------------------
            // Xrefs
            // -----------------------------

            if (selectedOptions.Contains(keys.Xrefs) && resultsfromcad.Xrefs.Any())
            {
                exportData.Add(keys.Xrefs, resultsfromcad.Xrefs);
            }

            // -----------------------------
            // Layer Zero
            // -----------------------------

            if (selectedOptions.Contains(keys.LayerZero) && resultsfromcad.LayerZero.Any())
            {
                exportData.Add(keys.LayerZero, resultsfromcad.LayerZero);
            }

            // -----------------------------
            // Layers In Use
            // -----------------------------

            if (selectedOptions.Contains(keys.LayersInUse) && resultsfromcad.LayersInUse.Any())
            {
                exportData.Add(keys.LayersInUse, resultsfromcad.LayersInUse);
            }

            // -----------------------------
            // Paper Text Font
            // -----------------------------

            if (selectedOptions.Contains(keys.PaperTextFont) && resultsfromcad.PaperTextFont.Any())
            {
                exportData.Add(keys.PaperTextFont, resultsfromcad.PaperTextFont);
            }

            // -----------------------------
            // ByLayer
            // -----------------------------

            if (selectedOptions.Contains(keys.EntByLayer) && resultsfromcad.ByLayer.Any())
            {
                exportData.Add(keys.EntByLayer, resultsfromcad.ByLayer);
            }

            // -----------------------------
            // Revision Clouds
            // -----------------------------

            if (selectedOptions.Contains(keys.RevCloud) && resultsfromcad.RevisionClouds.Any())
            {
                exportData.Add(keys.RevCloud, resultsfromcad.RevisionClouds);
            }

            // -----------------------------
            // Block Attributes
            // -----------------------------

            if (selectedOptions.Contains(keys.AttrBlockRef) && resultsfromcad.BlockAttributes.Any())
            {
                exportData.Add(keys.AttrBlockRef, resultsfromcad.BlockAttributes);
            }

            // -----------------------------
            // Plot Tags
            // -----------------------------

            if (selectedOptions.Contains(keys.PlotTag) && resultsfromcad.PlotTags.Any())
            {
                exportData.Add(keys.PlotTag, resultsfromcad.PlotTags);
            }

            // return
            return exportData;
        }
    }
}
