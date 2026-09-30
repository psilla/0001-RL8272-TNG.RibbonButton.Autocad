using System.Collections.Generic;
using TYPSA.SharedLib.Autocad;

namespace TNG.RibbonButton.Autocad
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
            // Drawing Audit
            // -----------------------------

            if (selectedOptions.Contains(keys.Audit))
            {
                exportData.Add(keys.Audit, resultsfromcad.Audit);
            }

            // -----------------------------
            // Purgeable Items
            // -----------------------------

            if (selectedOptions.Contains(keys.PurgeableItems))
            {
                exportData.Add(keys.PurgeableItems, resultsfromcad.PurgeableItemsCount);
            }

            // -----------------------------
            // Project Units
            // -----------------------------

            if (selectedOptions.Contains(keys.ProjectUnits))
            {
                exportData.Add(keys.ProjectUnits, resultsfromcad.ProjectUnits);
            }

            // -----------------------------
            // Version
            // -----------------------------

            if (selectedOptions.Contains(keys.Version))
            {
                exportData.Add(keys.Version, resultsfromcad.Version);
            }

            // -----------------------------
            // Xrefs
            // -----------------------------

            if (selectedOptions.Contains(keys.Xrefs))
            {
                exportData.Add(keys.Xrefs, resultsfromcad.Xrefs);
            }

            // -----------------------------
            // Layer Zero
            // -----------------------------

            if (selectedOptions.Contains(keys.LayerZero))
            {
                exportData.Add(keys.LayerZero, resultsfromcad.LayerZero);
            }

            // -----------------------------
            // Layers In Use
            // -----------------------------

            if (selectedOptions.Contains(keys.LayersInUse))
            {
                exportData.Add(keys.LayersInUse, resultsfromcad.LayersInUse);
            }

            // -----------------------------
            // Paper Text Font
            // -----------------------------

            if (selectedOptions.Contains(keys.PaperTextFont))
            {
                exportData.Add(keys.PaperTextFont, resultsfromcad.PaperTextFont);
            }

            // -----------------------------
            // ByLayer
            // -----------------------------

            if (selectedOptions.Contains(keys.EntByLayer))
            {
                exportData.Add(keys.EntByLayer, resultsfromcad.ByLayer);
            }

            // -----------------------------
            // Revision Clouds
            // -----------------------------

            if (selectedOptions.Contains(keys.RevCloud))
            {
                exportData.Add(keys.RevCloud, resultsfromcad.RevisionClouds);
            }

            // -----------------------------
            // Block Attributes
            // -----------------------------

            if (selectedOptions.Contains(keys.AttrBlockRef))
            {
                exportData.Add(keys.AttrBlockRef, resultsfromcad.BlockAttributes);
            }

            // -----------------------------
            // Plot Tags
            // -----------------------------

            if (selectedOptions.Contains(keys.PlotTag))
            {
                exportData.Add(keys.PlotTag, resultsfromcad.PlotTags);
            }

            // return
            return exportData;
        }

        
    }
}
