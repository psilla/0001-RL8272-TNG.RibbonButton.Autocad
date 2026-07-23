using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
    public static class TngModelCheckerDefaults
    {
        // -----------------------------
        // Fonts
        // -----------------------------

        public const string ExpectedPaperTextFont = "ISOCPEUR";

        // -----------------------------
        // Block Names
        // -----------------------------

        public const string PlotTagBlockName = "FUT_Ritningsram_";
        public const string BlockAttributesName = "FUT_Namnruta_";

        // -----------------------------
        // Plot Tag Reference Texts
        // -----------------------------

        public const string PlotFileText = "Ritningsfil:";
        public const string PlotDateText = "Plottdatum:";
        public const string PlotUserText = "Plottad av:";

        public static readonly List<string> PlotTagReferenceTexts =
            new List<string> {PlotFileText, PlotDateText, PlotUserText};
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

        public ProcessResult MainTgnModelChecker(
            string[] selectedFiles,
            string projectCode,
            List<string> selectedOptions,
            bool isSpanish,
            CadSessionInfo info
        )
        {
            // Acumulador
            ModelCheckerResults resultsfromcad = new ModelCheckerResults();
            ModelCheckerKeys keys = new ModelCheckerKeys(isSpanish);

            // Variables para recopilar métricas
            int totalSelectedFiles = selectedFiles.Length;
            int filesSelectedProcessed = 0;
            int percentage = 0;

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

                                        if (selectedOptions.Contains(keys.ProjectUnits))
                                        {
                                            ProjectUnitsResult units = AnalyzeUnits(db, fileName);
                                            // Validamos
                                            if (units == null)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keys.ProjectUnits,
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
                                                        CheckName = keys.ProjectUnits,
                                                        Message = $"Project units are set to '{units.Units}' instead of meters."
                                                    });
                                                }

                                                // Almacenamos
                                                resultsfromcad.ProjectUnits.Add(units);
                                            }
                                           
                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.ProjectUnits, units }
                                            });
                                        }

                                        // -----------------------------
                                        // Layers in Use
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.LayersInUse))
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
                                                    CheckName = keys.LayersInUse,
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
                                                        CheckName = keys.LayersInUse,
                                                        Message = "One or more layers exist in the drawing but are not in use."
                                                    });
                                                }

                                                // Almacenamos
                                                resultsfromcad.LayersInUse.AddRange(layersInUse);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.LayersInUse, layersInUse }
                                            });
                                        }

                                        // -----------------------------
                                        // Layer Zero
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.LayerZero))
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
                                                    CheckName = keys.LayerZero,
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
                                                        CheckName = keys.LayerZero,
                                                        Message = "One or more entities exist in layer 0."
                                                    });
                                                }

                                                // Almacenamos
                                                resultsfromcad.LayerZero.Add(entLayerZero);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.LayerZero, entLayerZero }
                                            });
                                        }

                                        // -----------------------------
                                        // Version Archivo
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.Version))
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
                                                    CheckName = keys.Version,
                                                    Message = "Selected check was executed, but no AutoCAD version information was detected."
                                                });
                                            }
                                            else
                                            {
                                                // Almacenamos
                                                resultsfromcad.Version.Add(version);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.Version, version }
                                            });
                                        }

                                        // -----------------------------
                                        // Xref
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.Xrefs))
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
                                                        CheckName = keys.Xrefs,
                                                        Message = "One or more external references are unloaded in this file."
                                                    });
                                                }

                                                // Almacenamos
                                                resultsfromcad.Xrefs.AddRange(xrefs);
                                            }
                                           
                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.Xrefs, xrefs }
                                            });
                                        }

                                        // -----------------------------
                                        // Labels Font
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.PaperTextFont))
                                        {
                                            List<PaperTextFontResult> paperFonts = AnalyzeTextFont(
                                                tr, db, fileName, TngModelCheckerDefaults.ExpectedPaperTextFont
                                            );
                                            // Validamos
                                            if (paperFonts == null || paperFonts.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keys.PaperTextFont,
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
                                                        CheckName = keys.PaperTextFont,
                                                        Message = "One or more paper space text entities use a font different from the required font."
                                                    });
                                                }

                                                // Almacenamos
                                                resultsfromcad.PaperTextFont.AddRange(paperFonts);
                                            }
                                           
                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.PaperTextFont, paperFonts }
                                            });
                                        }

                                        // -----------------------------
                                        // Properties ByLayer
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.EntByLayer))
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
                                                    CheckName = keys.EntByLayer,
                                                    Message = "Selected check was executed, but no ByLayer validation resultsfromcad were detected."
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
                                                        CheckName = keys.EntByLayer,
                                                        Message = "One or more entities have properties that are not set to ByLayer."
                                                    });
                                                }

                                                // Almacenamos
                                                resultsfromcad.ByLayer.AddRange(byLayerResults);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.EntByLayer, byLayerResults }
                                            });
                                        }

                                        // -----------------------------
                                        // Revision cloud
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.RevCloud))
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
                                                    CheckName = keys.RevCloud,
                                                    Message = "One or more revision clouds were detected in this file."
                                                });

                                                // Almacenamos
                                                resultsfromcad.RevisionClouds.AddRange(clouds);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.RevCloud, clouds }
                                            });
                                        }

                                        // -----------------------------
                                        // Block Attributes
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.AttrBlockRef))
                                        {
                                            List<BlockAttributesResult> blockAttrs = AnalyzeBlockAttributes(
                                                tr, db, bt, fileName, 
                                                TngModelCheckerDefaults.BlockAttributesName
                                            );
                                            // Validamos
                                            if (blockAttrs == null || blockAttrs.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keys.AttrBlockRef,
                                                    Message = "Selected check was executed, but no matching block references containing 'FUT_Namnruta_' were detected."
                                                });
                                            }
                                            else
                                            {
                                                // Almacenamos
                                                resultsfromcad.BlockAttributes.AddRange(blockAttrs);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.AttrBlockRef, blockAttrs }
                                            });
                                        }

                                        // -----------------------------
                                        // Plot Tag
                                        // -----------------------------

                                        if (selectedOptions.Contains(keys.PlotTag))
                                        {
                                            List<PlotInfoResult> plotTags = AnalyzePlotTagInfo(
                                                tr, db, bt, fileName, 
                                                TngModelCheckerDefaults.PlotTagReferenceTexts,
                                                TngModelCheckerDefaults.PlotTagBlockName
                                            );
                                            // Validamos
                                            if (plotTags == null || plotTags.Count == 0)
                                            {
                                                warningChecksLog.Add(new WarningCheckLogResult
                                                {
                                                    FileName = fileName,
                                                    CheckName = keys.PlotTag,
                                                    Message = "Selected check was executed, but no matching plot information block references containing 'FUT_Ritningsram_' were detected."
                                                });
                                            }
                                            else
                                            {
                                                // -----------------------------
                                                // Buscar referencias no encontradas
                                                // -----------------------------

                                                bool hasMissingReferenceTexts = plotTags.Any(
                                                    x => x != null && !x.IsFound
                                                );
                                                // Validamos
                                                if (hasMissingReferenceTexts)
                                                {
                                                    warningChecksLog.Add(new WarningCheckLogResult
                                                    {
                                                        FileName = fileName,
                                                        CheckName = keys.PlotTag,
                                                        Message = $"One or more required plot tag references ({string.Join(", ", TngModelCheckerDefaults.PlotTagReferenceTexts)}) " +
                                                            $"are missing or do not contain a valid associated value."
                                                    });
                                                }

                                                // Almacenamos
                                                resultsfromcad.PlotTags.AddRange(plotTags);
                                            }

                                            // Almacenamos
                                            extractedData.Add(new Dictionary<string, object>
                                            {
                                                { keys.PlotTag, plotTags }
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
                        resultsfromcad.ProjectUnits.Any() || resultsfromcad.LayersInUse.Any() || resultsfromcad.LayerZero.Any() || 
                        resultsfromcad.Version.Any() || resultsfromcad.Xrefs.Any() || resultsfromcad.PaperTextFont.Any() || resultsfromcad.ByLayer.Any() || 
                        resultsfromcad.RevisionClouds.Any() || resultsfromcad.BlockAttributes.Any() || resultsfromcad.PlotTags.Any();
                    // Validamos
                    if (hasData || warningChecksLog.Any())
                    {
                        // ---------------------------------
                        // Preparar datos exportacion
                        // ---------------------------------

                        Dictionary<string, object> exportData = GetExportData(
                            keys, selectedOptions, resultsfromcad
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
