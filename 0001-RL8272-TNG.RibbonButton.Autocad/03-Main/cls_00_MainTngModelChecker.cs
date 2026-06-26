using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Newtonsoft.Json;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Excel;
using TYPSA.SharedLib.UserForms;
using static TYPSA.SharedLib.Autocad.Main.cls_00_CadInfoHelper;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckAcadVersion;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckBlockAttr;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckByLayerProp;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckEntInLayerZero;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckLayersInUse;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckPaperTextFont;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckProjUnits;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckRevClouds;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainCheckXrefs;
using static TYPSA.SharedLib.Autocad.Main.cls_00_MainGetPlogTag;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;


namespace TYPSA.PS.RibbonButton.Autocad
{
    public static class cls_00_ExportModelCheckerToHtml
    {
        
    }

    public class WarningCheckLogResult
    {
        public string FileName { get; set; }
        public string CheckName { get; set; }
        public string Message { get; set; }
    }

    public class cls_00_MainTngModelChecker
    {
        private static Dictionary<string, object> GetExportData(
            bool isSpanish,
            List<string> selectedOptions,
            ModelCheckerResults results
        )
        {
            Dictionary<string, object> exportData = new Dictionary<string, object>();

            // -----------------------------
            // Keys
            // -----------------------------

            string keyProjectUnits = AteneaModelCheckerOptionsLocalized.ProjectUnits(isSpanish);
            string keyLayersInUse = AteneaModelCheckerOptionsLocalized.LayersInUse(isSpanish);
            string keyLayerZero = AteneaModelCheckerOptionsLocalized.LayerZero(isSpanish);
            string keyVersion = AteneaModelCheckerOptionsLocalized.Version(isSpanish);
            string keyXrefs = AteneaModelCheckerOptionsLocalized.Xrefs(isSpanish);
            string keyPaperTextFont = AteneaModelCheckerOptionsLocalized.PaperTextFont(isSpanish);
            string keyEntByLayer = AteneaModelCheckerOptionsLocalized.ByLayerProperties(isSpanish);
            string keyRevCloud = AteneaModelCheckerOptionsLocalized.RevisionClouds(isSpanish);
            string keyAttrBlockRef = AteneaModelCheckerOptionsLocalized.BlockAttributes(isSpanish);
            string keyPlotTag = AteneaModelCheckerOptionsLocalized.PlotTag(isSpanish);

            // -----------------------------
            // Project Units
            // -----------------------------

            if (selectedOptions.Contains(keyProjectUnits) && results.ProjectUnits.Any())
            {
                exportData.Add(keyProjectUnits, results.ProjectUnits);
            }

            // -----------------------------
            // Version
            // -----------------------------

            if (selectedOptions.Contains(keyVersion) && results.Version.Any())
            {
                exportData.Add(keyVersion, results.Version);
            }

            // -----------------------------
            // Xrefs
            // -----------------------------

            if (selectedOptions.Contains(keyXrefs) && results.Xrefs.Any())
            {
                exportData.Add(keyXrefs, results.Xrefs);
            }

            // -----------------------------
            // Layer Zero
            // -----------------------------

            if (selectedOptions.Contains(keyLayerZero) && results.LayerZero.Any())
            {
                exportData.Add(keyLayerZero, results.LayerZero);
            }

            // -----------------------------
            // Layers In Use
            // -----------------------------

            if (selectedOptions.Contains(keyLayersInUse) && results.LayersInUse.Any())
            {
                exportData.Add(keyLayersInUse, results.LayersInUse);
            }

            // -----------------------------
            // Paper Text Font
            // -----------------------------

            if (selectedOptions.Contains(keyPaperTextFont) && results.PaperTextFont.Any())
            {
                exportData.Add(keyPaperTextFont, results.PaperTextFont);
            }

            // -----------------------------
            // ByLayer
            // -----------------------------

            if (selectedOptions.Contains(keyEntByLayer) && results.ByLayer.Any())
            {
                exportData.Add(keyEntByLayer, results.ByLayer);
            }

            // -----------------------------
            // Revision Clouds
            // -----------------------------

            if (selectedOptions.Contains(keyRevCloud) && results.RevisionClouds.Any())
            {
                exportData.Add(keyRevCloud, results.RevisionClouds);
            }

            // -----------------------------
            // Block Attributes
            // -----------------------------

            if (selectedOptions.Contains(keyAttrBlockRef) && results.BlockAttributes.Any())
            {
                exportData.Add(keyAttrBlockRef, results.BlockAttributes);
            }

            // -----------------------------
            // Plot Tags
            // -----------------------------

            if (selectedOptions.Contains(keyPlotTag) && results.PlotTags.Any())
            {
                exportData.Add(keyPlotTag, results.PlotTags);
            }

            // return
            return exportData;
        }

        public ProcessResult MainTgnModelChecker(
            string[] selectedFiles,
            string projectCode,
            List<string> selectedOptions,
            bool isSpanish,
            CadSessionInfo info
        )
        {
            // Acumulador
            ModelCheckerResults results = new ModelCheckerResults();
            // Variables para recopilar métricas
            int totalSelectedFiles = selectedFiles.Length;
            int filesSelectedProcessed = 0;
            int percentage = 0;

            // Claves segun idioma
            string keyProjectUnits = AteneaModelCheckerOptionsLocalized.ProjectUnits(isSpanish);
            string keyLayersInUse = AteneaModelCheckerOptionsLocalized.LayersInUse(isSpanish);
            string keyLayerZero = AteneaModelCheckerOptionsLocalized.LayerZero(isSpanish);
            string keyVersion = AteneaModelCheckerOptionsLocalized.Version(isSpanish);
            string keyXrefs = AteneaModelCheckerOptionsLocalized.Xrefs(isSpanish);
            string keyPaperTextFont = AteneaModelCheckerOptionsLocalized.PaperTextFont(isSpanish);
            string keyEntByLayer = AteneaModelCheckerOptionsLocalized.ByLayerProperties(isSpanish);
            string keyRevCloud = AteneaModelCheckerOptionsLocalized.RevisionClouds(isSpanish);
            string keyAttrBlockRef = AteneaModelCheckerOptionsLocalized.BlockAttributes(isSpanish);
            string keyPlotTag = AteneaModelCheckerOptionsLocalized.PlotTag(isSpanish);

            // Creamos una lista vacia para almacenar diccionario global
            List<Dictionary<string, object>> dataJsonByModel = new List<Dictionary<string, object>>();

            // Log global
            List<WarningCheckLogResult> warningChecksLog = new List<WarningCheckLogResult>();

            List<List<string>> dataToExcelGlobal = new List<List<string>>();
            // Crear el formulario de la barra de progreso
            using (ProgressBarControl progressBarForm = new ProgressBarControl())
            {
                // Mostramos barra de progreso
                progressBarForm.Show();

                // try
                try
                {
                    // Iterar sobre los archivos seleccionados
                    foreach (string file in selectedFiles)
                    {
                        // Obtener el nombre sin extensión
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(file);

                        // try
                        try
                        {
                            // Abrir el documento
                            using (Document openedDoc = Application.DocumentManager.Open(file, false))
                            {
                                // Validamos
                                if (openedDoc == null)
                                {
                                    // Mensaje
                                    new AutoCloseMessageForm(
                                        $"Error opening document:\n{file}"
                                    ).ShowDialog();
                                    // Actualizar la barra de progreso
                                    filesSelectedProcessed++;
                                    percentage = (int)((double)filesSelectedProcessed / totalSelectedFiles * 100);
                                    progressBarForm.ProgressValue = percentage;
                                    // Obviamos
                                    continue;
                                }

                                // Mostramos
                                new AutoCloseMessageForm(
                                    $"Processing file {filesSelectedProcessed + 1} of {totalSelectedFiles}:\n\n{fileName}.dwg", 1000
                                ).ShowDialog();

                                // Obtenemos variables
                                Database db = openedDoc.Database;
                                Editor ed = cls_00_DocumentInfo.GetEditor(openedDoc);

                                // Bloquear el documento
                                using (openedDoc.LockDocument())
                                using (Transaction tr = openedDoc.TransactionManager.StartTransaction())
                                {
                                    // try
                                    try
                                    {
                                        // Obtener BlockTable
                                        BlockTable bt = cls_00_DocumentInfo.GetBlockTableForRead(tr, db);

                                        // Inicializamos la lista
                                        List<Dictionary<string, object>> extractedData = new List<Dictionary<string, object>>();

                                        // -----------------------------
                                        // Project Units
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyProjectUnits))
                                        {
                                            ProjectUnitsResult units = AnalyzeUnits(db, fileName);
                                            // Validamos
                                            if (units == null)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyProjectUnits,
                                                    Message = "Selected check was executed, but no project units information was detected."
                                                });
                                            }
                                            else
                                            {
                                                // Validamos que sean metros
                                                if (!units.IsMeters)
                                                {
                                                    warningChecksLog.Add(new WarningCheckLogResult
                                                    {
                                                        FileName = fileName,
                                                        CheckName = keyProjectUnits,
                                                        Message = $"Project units are set to '{units.Units}' instead of meters."
                                                    });
                                                }

                                                // Almacenamos
                                                results.ProjectUnits.Add(units);
                                            }
                                           
                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyProjectUnits, units }
                                            });
                                        }

                                        // -----------------------------
                                        // Layers in Use
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyLayersInUse))
                                        {
                                            List<LayerUsageResult> layersInUse = AnalyzeLayers(
                                                tr, db, bt, fileName
                                            );
                                            // Validamos
                                            if (layersInUse == null || layersInUse.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyLayersInUse,
                                                    Message = "Selected check was executed, but no layers in use were detected."
                                                });
                                            }
                                            else
                                            {
                                                // -----------------------------
                                                // Buscar capas sin uso
                                                // -----------------------------

                                                bool hasUnusedLayers = layersInUse.Any(x => x != null && !x.IsUsed);
                                                // Validamos
                                                if (hasUnusedLayers)
                                                {
                                                    warningChecksLog.Add(new WarningCheckLogResult
                                                    {
                                                        FileName = fileName,
                                                        CheckName = keyLayersInUse,
                                                        Message = "One or more layers exist in the drawing but are not in use."
                                                    });
                                                }

                                                // Almacenamos
                                                results.LayersInUse.AddRange(layersInUse);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyLayersInUse, layersInUse }
                                            });
                                        }

                                        // -----------------------------
                                        // Layer Zero
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyLayerZero))
                                        {
                                            LayerZeroUsageResult entLayerZero = AnalyzeLayerZero(
                                                tr, db, bt, fileName
                                            );
                                            // Validamos
                                            if (entLayerZero == null)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyLayerZero,
                                                    Message = "Selected check was executed, but no layer zero information was detected."
                                                });
                                            }
                                            else
                                            {
                                                // Validamos si hay entidades en capa 0
                                                if (entLayerZero.IsUsed)
                                                {
                                                    warningChecksLog.Add(new WarningCheckLogResult
                                                    {
                                                        FileName = fileName,
                                                        CheckName = keyLayerZero,
                                                        Message = "One or more entities exist in layer 0."
                                                    });
                                                }

                                                // Almacenamos
                                                results.LayerZero.Add(entLayerZero);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyLayerZero, entLayerZero }
                                            });
                                        }

                                        // -----------------------------
                                        // Version Archivo
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyVersion))
                                        {
                                            AcadVersionResult version = AnalyzeVersionMapped(
                                                db, fileName
                                            );
                                            // Validamos
                                            if (version == null)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyVersion,
                                                    Message = "Selected check was executed, but no AutoCAD version information was detected."
                                                });
                                            }
                                            else
                                            {
                                                // Almacenamos
                                                results.Version.Add(version);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyVersion, version }
                                            });
                                        }

                                        // -----------------------------
                                        // Xref
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyXrefs))
                                        {
                                            List<XrefStatusResult> xrefs = AnalyzeXrefs(
                                                bt, tr, fileName
                                            );
                                            // Validamos que existan Xrefs
                                            if (xrefs != null && xrefs.Count > 0)
                                            {
                                                // -----------------------------
                                                // Verificar si alguna Xref está descargada
                                                // -----------------------------

                                                bool hasUnloadedXrefs = xrefs.Any(x =>
                                                    x != null && !x.IsLoaded
                                                );
                                                // Añadimos unico warning
                                                if (hasUnloadedXrefs)
                                                {
                                                    warningChecksLog.Add(new WarningCheckLogResult
                                                    {
                                                        FileName = fileName,
                                                        CheckName = keyXrefs,
                                                        Message = "One or more external references are unloaded in this file."
                                                    });
                                                }

                                                // Almacenamos
                                                results.Xrefs.AddRange(xrefs);
                                            }
                                           
                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyXrefs, xrefs }
                                            });
                                        }

                                        // -----------------------------
                                        // Labels Font
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyPaperTextFont))
                                        {
                                            List<PaperTextFontResult> paperFonts = AnalyzeTextFont(
                                                tr, db, fileName
                                            );
                                            // Validamos
                                            if (paperFonts == null || paperFonts.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyPaperTextFont,
                                                    Message = "Selected check was executed, but no paper space text entities were detected."
                                                });
                                            }
                                            else
                                            {
                                                // -----------------------------
                                                // Buscar fuentes distintas a la requerida
                                                // -----------------------------

                                                bool hasUnexpectedFonts = paperFonts.Any(x => x != null && !x.IsExpected);
                                                // Validamos
                                                if (hasUnexpectedFonts)
                                                {
                                                    warningChecksLog.Add(new WarningCheckLogResult
                                                    {
                                                        FileName = fileName,
                                                        CheckName = keyPaperTextFont,
                                                        Message = "One or more paper space text entities use a font different from the required font."
                                                    });
                                                }

                                                // Almacenamos
                                                results.PaperTextFont.AddRange(paperFonts);
                                            }
                                           
                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyPaperTextFont, paperFonts }
                                            });
                                        }

                                        // -----------------------------
                                        // Properties ByLayer
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyEntByLayer))
                                        {
                                            //SetByLayerProperties(tr, db, bt);
                                            List<ByLayerEntityResult> byLayerResults = AnalyzeByLayer(
                                                tr, db, bt, fileName, isSpanish
                                            );
                                            // Validamos
                                            if (byLayerResults == null || byLayerResults.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyEntByLayer,
                                                    Message = "Selected check was executed, but no ByLayer validation results were detected."
                                                });
                                            }
                                            else
                                            {
                                                // -----------------------------
                                                // Buscar entidades con propiedades no ByLayer
                                                // -----------------------------

                                                bool hasNotByLayerProperties = byLayerResults.Any(x => x != null &&
                                                (
                                                    x.IsColorByLayer == false ||
                                                    x.IsLinetypeByLayer == false ||
                                                    x.IsLineweightByLayer == false
                                                ));
                                                // Validamos
                                                if (hasNotByLayerProperties)
                                                {
                                                    warningChecksLog.Add(new WarningCheckLogResult
                                                    {
                                                        FileName = fileName,
                                                        CheckName = keyEntByLayer,
                                                        Message = "One or more entities have properties that are not set to ByLayer."
                                                    });
                                                }

                                                // Almacenamos
                                                results.ByLayer.AddRange(byLayerResults);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyEntByLayer, byLayerResults }
                                            });
                                        }

                                        // -----------------------------
                                        // Revision cloud
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyRevCloud))
                                        {
                                            List<RevisionCloudResult> clouds = AnalyzeRevisionClouds(
                                                tr, db, bt, fileName
                                            );
                                            // Validamos
                                            if (clouds != null && clouds.Count > 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyRevCloud,
                                                    Message = "One or more revision clouds were detected in this file."
                                                });

                                                // Almacenamos
                                                results.RevisionClouds.AddRange(clouds);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyRevCloud, clouds }
                                            });
                                        }

                                        // -----------------------------
                                        // Block Attributes
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyAttrBlockRef))
                                        {
                                            List<BlockAttributesResult> blockAttrs = AnalyzeBlockAttributes(
                                                tr, db, bt, fileName, "FUT_Namnruta_"
                                            );
                                            // Validamos
                                            if (blockAttrs == null || blockAttrs.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyAttrBlockRef,
                                                    Message = "Selected check was executed, but no matching block references containing 'FUT_Namnruta_' were detected."
                                                });
                                            }
                                            else
                                            {
                                                // Almacenamos
                                                results.BlockAttributes.AddRange(blockAttrs);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyAttrBlockRef, blockAttrs }
                                            });
                                        }

                                        // -----------------------------
                                        // Plot Tag
                                        // -----------------------------

                                        if (selectedOptions.Contains(keyPlotTag))
                                        {
                                            // Textos referencia
                                            List<string> referenceTexts = 
                                                new List<string> {"Ritningsfil:", "Plottdatum:", "Plottad av:"};
                                            List<PlotInfoResult> plotTags = AnalyzePlotTagInfo(
                                                tr, db, bt, fileName, referenceTexts, "FUT_Ritningsram_"
                                            );
                                            // Validamos
                                            if (plotTags == null || plotTags.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keyPlotTag,
                                                    Message = "Selected check was executed, but no matching plot information block references containing 'FUT_Ritningsram_' were detected."
                                                });
                                            }
                                            else
                                            {
                                                // Almacenamos
                                                results.PlotTags.AddRange(plotTags);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keyPlotTag, plotTags }
                                            });
                                        }

                                        // -----------------------------
                                        // Crear Estructura Json By File
                                        // -----------------------------

                                        // Creamos la estructura
                                        Dictionary<string, object> fileDataByDoc = new Dictionary<string, object>
                                        {
                                            { AteneaJson.FileName, fileName },
                                            { AteneaJson.CivilElementData, extractedData }
                                        };

                                        // -----------------------------
                                        // Añadir Json
                                        // -----------------------------

                                        dataJsonByModel.Add(fileDataByDoc);

                                        // -----------------------------
                                        // Cerrar transaccion
                                        // -----------------------------

                                        tr.Commit();
                                    }
                                    // catch
                                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                                    {
                                        warningChecksLog.Add(new WarningCheckLogResult
                                        {
                                            FileName = fileName,
                                            CheckName = "Processing Error",
                                            Message = ex.Message
                                        });

                                        // Mensaje
                                        new AutoCloseMessageForm(
                                            $"Error while processing '{file}':\n{ex.Message}"
                                        ).ShowDialog();
                                    }
                                }

                                // Cerramos y descartamos documento
                                openedDoc.CloseAndDiscard();

                                // Actualizar la barra de progreso
                                filesSelectedProcessed++;
                                percentage = (int)((double)filesSelectedProcessed / totalSelectedFiles * 100);
                                progressBarForm.ProgressValue = percentage;
                            }
                        }
                        // catch
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                            warningChecksLog.Add(new WarningCheckLogResult
                            {
                                FileName = fileName,
                                CheckName = "Fatal File Error",
                                Message = ex.Message
                            });

                            // Mensaje
                            MessageBox.Show(
                                $"EXCEPTION:\n{ex.Message}\n{ex.StackTrace}"
                            );
                        }
                    }

                    // Cerramos
                    progressBarForm.Close();

                    // Comprobamos info a exportar
                    bool hasData = 
                        results.ProjectUnits.Any() || results.LayersInUse.Any() || results.LayerZero.Any() || 
                        results.Version.Any() || results.Xrefs.Any() ||
                        results.PaperTextFont.Any() || results.ByLayer.Any() || results.RevisionClouds.Any() ||
                        results.BlockAttributes.Any() || results.PlotTags.Any();
                    // Validamos
                    if (hasData || warningChecksLog.Any())
                    {
                        // ---------------------------------
                        // Preparar datos exportacion
                        // ---------------------------------

                        Dictionary<string, object> exportData = GetExportData(
                            isSpanish, selectedOptions, results
                        );

                        // ---------------------------------
                        // Validar
                        // ---------------------------------

                        if (warningChecksLog.Any())
                        {
                            exportData.Add("Warning Selected Checks Log", warningChecksLog);
                        }

                        // ---------------------------------
                        // Exportar Excel
                        // ---------------------------------

                        cls_00_ExportModCheckToExcel_OpenXml.ExportDataToExcel(
                            exportData
                        );

                        // ---------------------------------
                        // Exportar html
                        // ---------------------------------

                        cls_00_ExportTgnCheckToHtml.ExportToHtml(
                            exportData, warningChecksLog, projectCode, totalSelectedFiles, filesSelectedProcessed
                        );
                    }
                    else
                    {
                        // Mensaje
                        MessageBox.Show(
                            "No data found for the selected checks.", "Atenea Model Checker",
                            MessageBoxButtons.OK, MessageBoxIcon.Information
                        );
                    }

                    // try
                    try
                    {
                        // Preparar diccionario a enviar
                        Dictionary<string, object> dictDataByFileToJson = GetFinalJsonDictionary(
                            projectCode, dataJsonByModel
                        );
                        // Serializar el diccionario a formato JSON con indentación
                        string jsonContent = JsonConvert.SerializeObject(
                            dictDataByFileToJson, Formatting.Indented
                        );
                        // Guardamos el Json
                        SaveJsonToDesktop(
                            isSpanish, jsonContent, projectCode, info.RootFolderName, info.JsonFileNameDataExtraction
                        );
                    }
                    // catch
                    catch (Exception ex)
                    {
                        string inner = ex.InnerException != null
                            ? $"\n\nInner:\n{ex.InnerException.Message}"
                            : "";
                        // Mostramos
                        MessageBox.Show(
                            "Error generating JSON\n\n" + ex.Message + inner + "\n\nStack:\n" + ex.StackTrace,
                            "JSON Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                        );
                    }

                    // Enviar Metricas Serapis
                    SerapisMetrics.InitializeMetricsAsync("6a3541d327f9ca81c8d1fa0d");

                    // return
                    return new ProcessResult
                    {
                        TotalFilesProcessed = filesSelectedProcessed,
                        ParametersAnalyzed = filesSelectedProcessed
                    };
                }
                // catch
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                // Por defecto
                return new ProcessResult();
            }
        }

    }
}
